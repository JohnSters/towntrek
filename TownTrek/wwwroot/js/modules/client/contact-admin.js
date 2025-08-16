/**
 * Contact Admin Form Handler
 * Handles topic selection, form validation, and dynamic content updates
 */

class ContactAdminManager {
    constructor() {
        this.topicSelect = document.getElementById('topicSelect');
        this.topicDetails = document.getElementById('topicDetails');
        this.messageTextarea = document.querySelector('textarea[name="Message"]');
        this.messageCounter = document.getElementById('messageCounter');
        this.subjectInput = document.querySelector('input[name="Subject"]');
        this.submitBtn = document.getElementById('submitBtn');
        
        this.init();
    }

    init() {
        this.bindEvents();
        this.updateMessageCounter();
    }

    bindEvents() {
        // Topic selection change
        if (this.topicSelect) {
            this.topicSelect.addEventListener('change', (e) => {
                this.handleTopicChange(e.target.value);
            });
        }

        // Message character counter
        if (this.messageTextarea) {
            this.messageTextarea.addEventListener('input', () => {
                this.updateMessageCounter();
            });
        }

        // Auto-populate subject based on topic
        if (this.topicSelect && this.subjectInput) {
            this.topicSelect.addEventListener('change', () => {
                this.autoPopulateSubject();
            });
        }

        // Form submission
        const form = document.getElementById('contactAdminForm');
        if (form) {
            form.addEventListener('submit', (e) => {
                this.handleFormSubmit(e);
            });
        }
    }

    async handleTopicChange(topicId) {
        if (!topicId) {
            this.hideTopicDetails();
            return;
        }

        try {
            const response = await fetch(`/Client/GetTopicDetails?topicId=${topicId}`);
            if (response.ok) {
                const topic = await response.json();
                this.showTopicDetails(topic);
            } else {
                console.error('Failed to fetch topic details');
                this.hideTopicDetails();
            }
        } catch (error) {
            console.error('Error fetching topic details:', error);
            this.hideTopicDetails();
        }
    }

    showTopicDetails(topic) {
        const iconElement = document.getElementById('topicIcon');
        const nameElement = document.getElementById('topicName');
        const descriptionElement = document.getElementById('topicDescription');
        const priorityElement = document.getElementById('topicPriority');

        if (iconElement) iconElement.className = topic.iconClass;
        if (nameElement) nameElement.textContent = topic.name;
        if (descriptionElement) descriptionElement.textContent = topic.description;
        if (priorityElement) {
            priorityElement.textContent = topic.priority;
            priorityElement.className = `badge badge-${this.getPriorityClass(topic.priority)}`;
        }

        this.topicDetails.style.display = 'block';
        this.topicDetails.classList.add('fade-in');
    }

    hideTopicDetails() {
        this.topicDetails.style.display = 'none';
        this.topicDetails.classList.remove('fade-in');
    }

    updateMessageCounter() {
        if (this.messageTextarea && this.messageCounter) {
            const currentLength = this.messageTextarea.value.length;
            this.messageCounter.textContent = currentLength;
            
            // Update counter color based on length
            if (currentLength > 1800) {
                this.messageCounter.style.color = 'var(--orange-pantone)';
            } else if (currentLength > 1500) {
                this.messageCounter.style.color = 'var(--hunyadi-yellow)';
            } else {
                this.messageCounter.style.color = 'var(--dark-gray)';
            }
        }
    }

    autoPopulateSubject() {
        if (!this.subjectInput.value && this.topicSelect.value) {
            const selectedOption = this.topicSelect.options[this.topicSelect.selectedIndex];
            const topicName = selectedOption.text;
            
            // Generate a default subject based on topic
            const subjectTemplates = {
                'Payment Issues': 'Payment Issue - ',
                'Technical Problems': 'Technical Issue - ',
                'Account Access': 'Account Access Issue - ',
                'Feature Requests': 'Feature Request - ',
                'Business Listing Issues': 'Business Listing Issue - ',
                'Subscription Changes': 'Subscription Change Request - ',
                'Data Corrections': 'Data Correction Request - ',
                'General Support': 'Support Request - ',
                'Feedback & Suggestions': 'Feedback - ',
                'Partnership Inquiries': 'Partnership Inquiry - '
            };

            const template = subjectTemplates[topicName] || `${topicName} - `;
            this.subjectInput.value = template;
            this.subjectInput.focus();
            this.subjectInput.setSelectionRange(template.length, template.length);
        }
    }

    handleFormSubmit(e) {
        // Add loading state to submit button
        if (this.submitBtn) {
            this.submitBtn.disabled = true;
            this.submitBtn.innerHTML = `
                <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24" class="animate-spin">
                    <circle cx="12" cy="12" r="10" stroke-width="2" stroke-dasharray="31.416" stroke-dashoffset="31.416">
                        <animate attributeName="stroke-dasharray" dur="2s" values="0 31.416;15.708 15.708;0 31.416" repeatCount="indefinite"/>
                        <animate attributeName="stroke-dashoffset" dur="2s" values="0;-15.708;-31.416" repeatCount="indefinite"/>
                    </circle>
                </svg>
                Sending...
            `;
        }

        // Form will submit normally, loading state will be reset on page reload
    }

    getPriorityClass(priority) {
        const priorityClasses = {
            'Critical': 'danger',
            'High': 'danger',
            'Medium': 'warning',
            'Low': 'secondary'
        };
        return priorityClasses[priority] || 'secondary';
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ContactAdminManager();
});

// Export for potential use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ContactAdminManager;
}