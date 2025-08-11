// Image Gallery JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Get data from page header data attributes
    const pageHeader = document.querySelector('.page-header');
    const businessId = pageHeader?.dataset.businessId;
    const maxFileSize = parseInt(pageHeader?.dataset.maxFileSize || '0');
    const allowedTypes = pageHeader?.dataset.allowedTypes ? JSON.parse(pageHeader.dataset.allowedTypes) : [];

    // Logo upload handling
    const logoUploadArea = document.getElementById('logoUploadArea');
    const logoUpload = document.getElementById('logoUpload');
    
    if (logoUploadArea) {
        logoUploadArea.addEventListener('click', () => {
            logoUpload.click();
        });
    }

    if (logoUpload) {
        logoUpload.addEventListener('change', (e) => {
            if (e.target.files.length > 0) {
                uploadImage(e.target.files[0], 'Logo');
            }
        });
    }

    // Gallery upload handling
    const galleryUploadArea = document.getElementById('galleryUploadArea');
    const galleryUpload = document.getElementById('galleryUpload');
    
    if (galleryUploadArea) {
        galleryUploadArea.addEventListener('click', () => {
            galleryUpload.click();
        });
    }

    if (galleryUpload) {
        galleryUpload.addEventListener('change', (e) => {
            if (e.target.files.length > 0) {
                uploadMultipleImages(Array.from(e.target.files), 'Gallery');
            }
        });
    }

    // Drag and drop handling
    ['logoUploadArea', 'galleryUploadArea'].forEach(areaId => {
        const area = document.getElementById(areaId);
        if (!area) return;
        
        area.addEventListener('dragover', (e) => {
            e.preventDefault();
            area.classList.add('dragover');
        });

        area.addEventListener('dragleave', () => {
            area.classList.remove('dragover');
        });

        area.addEventListener('drop', (e) => {
            e.preventDefault();
            area.classList.remove('dragover');
            
            const files = Array.from(e.dataTransfer.files);
            const imageType = areaId === 'logoUploadArea' ? 'Logo' : 'Gallery';
            
            if (imageType === 'Logo' && files.length > 0) {
                uploadImage(files[0], 'Logo');
            } else if (imageType === 'Gallery') {
                uploadMultipleImages(files, 'Gallery');
            }
        });
    });

    // Upload single image
    async function uploadImage(file, imageType) {
        if (!validateFile(file)) return;

        const formData = new FormData();
        formData.append('BusinessId', businessId);
        formData.append('ImageType', imageType);
        formData.append('ImageFile', file);

        try {
            showProgress(true);
            const response = await fetch('/Image/Upload', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            
            if (result.success) {
                showMessage('Image uploaded successfully!', 'success');
                location.reload(); // Refresh to show new image
            } else {
                showMessage(result.message || 'Upload failed', 'error');
            }
        } catch (error) {
            showMessage('Upload failed: ' + error.message, 'error');
        } finally {
            showProgress(false);
        }
    }

    // Upload multiple images
    async function uploadMultipleImages(files, imageType) {
        const validFiles = files.filter(validateFile);
        if (validFiles.length === 0) return;

        const formData = new FormData();
        formData.append('businessId', businessId);
        formData.append('imageType', imageType);
        
        validFiles.forEach(file => {
            formData.append('files', file);
        });

        try {
            showProgress(true);
            const response = await fetch('/Image/UploadMultiple', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            
            if (result.success) {
                showMessage(result.message, 'success');
                location.reload(); // Refresh to show new images
            } else {
                showMessage(result.message || 'Upload failed', 'error');
            }
        } catch (error) {
            showMessage('Upload failed: ' + error.message, 'error');
        } finally {
            showProgress(false);
        }
    }

    // Setup event delegation for action buttons
    document.addEventListener('click', function(e) {
        const button = e.target.closest('[data-action]');
        if (!button) return;

        const action = button.dataset.action;
        const imageId = button.dataset.imageId;

        if (action === 'delete') {
            deleteImage(imageId);
        } else if (action === 'edit') {
            editImage(imageId);
        }
    });

    // Delete image
    async function deleteImage(imageId) {
        if (!confirm('Are you sure you want to delete this image?')) return;

        try {
            const response = await fetch('/Image/Delete', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ imageId: imageId })
            });

            const result = await response.json();
            
            if (result.success) {
                showMessage('Image deleted successfully!', 'success');
                const imageElement = document.querySelector(`[data-image-id="${imageId}"]`);
                if (imageElement) {
                    imageElement.remove();
                }
            } else {
                showMessage(result.message || 'Delete failed', 'error');
            }
        } catch (error) {
            showMessage('Delete failed: ' + error.message, 'error');
        }
    }

    // Edit image (placeholder)
    function editImage(imageId) {
        // This could open a modal for editing alt text, display order, etc.
        alert('Edit functionality coming soon!');
    }

    // Validate file
    function validateFile(file) {
        if (file.size > maxFileSize) {
            showMessage(`File size must be less than ${(maxFileSize / (1024 * 1024)).toFixed(1)}MB`, 'error');
            return false;
        }

        if (!allowedTypes.includes(file.type)) {
            showMessage(`File type '${file.type}' is not allowed`, 'error');
            return false;
        }

        return true;
    }

    // Show progress
    function showProgress(show) {
        const progressElement = document.querySelector('.upload-progress');
        if (progressElement) {
            progressElement.style.display = show ? 'block' : 'none';
        }
    }

    // Show message
    function showMessage(message, type) {
        // Create a simple toast notification
        const toast = document.createElement('div');
        toast.className = `alert alert-${type === 'success' ? 'success' : 'danger'} alert-dismissible fade show`;
        toast.style.position = 'fixed';
        toast.style.top = '20px';
        toast.style.right = '20px';
        toast.style.zIndex = '9999';
        toast.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        document.body.appendChild(toast);
        
        setTimeout(() => {
            toast.remove();
        }, 5000);
    }
});
