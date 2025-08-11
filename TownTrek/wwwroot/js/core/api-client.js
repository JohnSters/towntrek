/**
 * @fileoverview API client for server communication
 * @author TownTrek Development Team
 * @version 2.0.0
 */

/**
 * API client for handling server communication
 * @class
 */
class ApiClient {
  /**
   * @constructor
   * @param {Object} options - Configuration options
   */
  constructor(options = {}) {
    this.options = {
      baseUrl: window.APP_CONFIG?.api?.baseUrl || '/api',
      timeout: window.APP_CONFIG?.api?.timeout || 30000,
      retryAttempts: window.APP_CONFIG?.api?.retryAttempts || 3,
      ...options
    };
  }

  /**
   * Get anti-forgery token from the page
   * @static
   * @returns {string|null} Anti-forgery token
   */
  static getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : null;
  }

  /**
   * Make HTTP request
   * @param {string} endpoint - API endpoint
   * @param {Object} options - Request options
   * @returns {Promise<Object>} Response data
   */
  async request(endpoint, options = {}) {
    const url = endpoint.startsWith('http') ? endpoint : `${this.options.baseUrl}${endpoint}`;
    
    const defaultHeaders = {
      'Content-Type': 'application/json',
      'X-Requested-With': 'XMLHttpRequest'
    };

    // Add anti-forgery token for non-GET requests
    if (options.method && options.method.toUpperCase() !== 'GET') {
      const token = ApiClient.getAntiForgeryToken();
      if (token) {
        defaultHeaders['RequestVerificationToken'] = token;
      }
    }

    const config = {
      method: 'GET',
      headers: defaultHeaders,
      ...options,
      headers: { ...defaultHeaders, ...options.headers }
    };

    // Add timeout
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), this.options.timeout);
    config.signal = controller.signal;

    try {
      const response = await this.makeRequestWithRetry(url, config);
      clearTimeout(timeoutId);
      
      if (!response.ok) {
        throw response; // Will be handled by ErrorHandler.handleApiError
      }

      // Handle different content types
      const contentType = response.headers.get('content-type');
      if (contentType && contentType.includes('application/json')) {
        return await response.json();
      } else {
        return await response.text();
      }
    } catch (error) {
      clearTimeout(timeoutId);
      
      if (error.name === 'AbortError') {
        const timeoutError = ErrorHandler.createError(
          'Request timed out',
          'TIMEOUT_ERROR',
          { endpoint, timeout: this.options.timeout }
        );
        throw timeoutError;
      }
      
      throw error;
    }
  }

  /**
   * Make request with retry logic
   * @private
   * @param {string} url - Request URL
   * @param {Object} config - Request configuration
   * @returns {Promise<Response>} Response object
   */
  async makeRequestWithRetry(url, config) {
    let lastError;
    
    for (let attempt = 1; attempt <= this.options.retryAttempts; attempt++) {
      try {
        return await fetch(url, config);
      } catch (error) {
        lastError = error;
        
        // Don't retry on certain errors
        if (error.name === 'AbortError' || attempt === this.options.retryAttempts) {
          throw error;
        }
        
        // Wait before retry with exponential backoff
        const delay = Math.min(1000 * Math.pow(2, attempt - 1), 10000);
        await Utils.sleep(delay);
      }
    }
    
    throw lastError;
  }

  /**
   * GET request
   * @param {string} endpoint - API endpoint
   * @param {Object} params - Query parameters
   * @param {Object} options - Request options
   * @returns {Promise<Object>} Response data
   */
  async get(endpoint, params = {}, options = {}) {
    const url = new URL(endpoint, window.location.origin);
    Object.entries(params).forEach(([key, value]) => {
      if (value !== null && value !== undefined) {
        url.searchParams.append(key, value);
      }
    });

    return this.request(url.pathname + url.search, {
      method: 'GET',
      ...options
    });
  }

  /**
   * POST request
   * @param {string} endpoint - API endpoint
   * @param {Object|FormData} data - Request data
   * @param {Object} options - Request options
   * @returns {Promise<Object>} Response data
   */
  async post(endpoint, data = {}, options = {}) {
    const config = {
      method: 'POST',
      ...options
    };

    if (data instanceof FormData) {
      // Don't set Content-Type for FormData, let browser set it with boundary
      delete config.headers?.['Content-Type'];
      config.body = data;
    } else {
      config.body = JSON.stringify(data);
    }

    return this.request(endpoint, config);
  }

  /**
   * PUT request
   * @param {string} endpoint - API endpoint
   * @param {Object|FormData} data - Request data
   * @param {Object} options - Request options
   * @returns {Promise<Object>} Response data
   */
  async put(endpoint, data = {}, options = {}) {
    const config = {
      method: 'PUT',
      ...options
    };

    if (data instanceof FormData) {
      delete config.headers?.['Content-Type'];
      config.body = data;
    } else {
      config.body = JSON.stringify(data);
    }

    return this.request(endpoint, config);
  }

  /**
   * DELETE request
   * @param {string} endpoint - API endpoint
   * @param {Object} options - Request options
   * @returns {Promise<Object>} Response data
   */
  async delete(endpoint, options = {}) {
    return this.request(endpoint, {
      method: 'DELETE',
      ...options
    });
  }

  /**
   * Upload file(s)
   * @param {string} endpoint - Upload endpoint
   * @param {File|FileList|Array<File>} files - File(s) to upload
   * @param {Object} additionalData - Additional form data
   * @param {Function} onProgress - Progress callback
   * @returns {Promise<Object>} Upload result
   */
  async uploadFiles(endpoint, files, additionalData = {}, onProgress = null) {
    const formData = new FormData();
    
    // Add files
    if (files instanceof FileList || Array.isArray(files)) {
      Array.from(files).forEach((file, index) => {
        formData.append(`files[${index}]`, file);
      });
    } else if (files instanceof File) {
      formData.append('file', files);
    }

    // Add additional data
    Object.entries(additionalData).forEach(([key, value]) => {
      if (value !== null && value !== undefined) {
        formData.append(key, value);
      }
    });

    const config = {
      method: 'POST',
      body: formData
    };

    // Add progress tracking if supported and callback provided
    if (onProgress && typeof onProgress === 'function') {
      return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        
        xhr.upload.addEventListener('progress', (event) => {
          if (event.lengthComputable) {
            const percentComplete = (event.loaded / event.total) * 100;
            onProgress(percentComplete, event.loaded, event.total);
          }
        });

        xhr.addEventListener('load', () => {
          if (xhr.status >= 200 && xhr.status < 300) {
            try {
              const response = JSON.parse(xhr.responseText);
              resolve(response);
            } catch (error) {
              resolve(xhr.responseText);
            }
          } else {
            reject(new Error(`HTTP ${xhr.status}: ${xhr.statusText}`));
          }
        });

        xhr.addEventListener('error', () => {
          reject(new Error('Upload failed'));
        });

        xhr.addEventListener('timeout', () => {
          reject(new Error('Upload timed out'));
        });

        xhr.timeout = this.options.timeout;
        xhr.open('POST', endpoint);
        
        // Add anti-forgery token
        const token = ApiClient.getAntiForgeryToken();
        if (token) {
          xhr.setRequestHeader('RequestVerificationToken', token);
        }
        
        xhr.send(formData);
      });
    }

    return this.request(endpoint, config);
  }
}

/**
 * Static methods for convenience
 */
ApiClient.instance = new ApiClient();

/**
 * Static GET method
 * @static
 * @param {string} endpoint - API endpoint
 * @param {Object} params - Query parameters
 * @param {Object} options - Request options
 * @returns {Promise<Object>} Response data
 */
ApiClient.get = (endpoint, params, options) => {
  return ApiClient.instance.get(endpoint, params, options);
};

/**
 * Static POST method
 * @static
 * @param {string} endpoint - API endpoint
 * @param {Object|FormData} data - Request data
 * @param {Object} options - Request options
 * @returns {Promise<Object>} Response data
 */
ApiClient.post = (endpoint, data, options) => {
  return ApiClient.instance.post(endpoint, data, options);
};

/**
 * Static PUT method
 * @static
 * @param {string} endpoint - API endpoint
 * @param {Object|FormData} data - Request data
 * @param {Object} options - Request options
 * @returns {Promise<Object>} Response data
 */
ApiClient.put = (endpoint, data, options) => {
  return ApiClient.instance.put(endpoint, data, options);
};

/**
 * Static DELETE method
 * @static
 * @param {string} endpoint - API endpoint
 * @param {Object} options - Request options
 * @returns {Promise<Object>} Response data
 */
ApiClient.delete = (endpoint, options) => {
  return ApiClient.instance.delete(endpoint, options);
};

/**
 * Static upload method
 * @static
 * @param {string} endpoint - Upload endpoint
 * @param {File|FileList|Array<File>} files - File(s) to upload
 * @param {Object} additionalData - Additional form data
 * @param {Function} onProgress - Progress callback
 * @returns {Promise<Object>} Upload result
 */
ApiClient.upload = (endpoint, files, additionalData, onProgress) => {
  return ApiClient.instance.uploadFiles(endpoint, files, additionalData, onProgress);
};

// Export for module use
if (typeof module !== 'undefined' && module.exports) {
  module.exports = ApiClient;
}

// Global registration
window.ApiClient = ApiClient;