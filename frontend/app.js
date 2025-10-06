// Translation Manager Frontend JavaScript
class TranslationManager {
    constructor() {
        this.apiBaseUrl = 'http://localhost:5090/api';
        this.translations = [];
        this.selectedTranslations = new Set();
        this.currentEditingId = null;
        this.showWarningsOnly = false; // Add warning filter state
        
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadTranslations();
    }

    bindEvents() {
        // Modal events
        document.getElementById('addTranslationBtn').addEventListener('click', () => this.openModal());
        document.getElementById('closeModal').addEventListener('click', () => this.closeModal());
        document.getElementById('cancelBtn').addEventListener('click', () => this.closeModal());
        document.getElementById('translationForm').addEventListener('submit', (e) => this.saveTranslation(e));

        // Import/Export events
        document.getElementById('importBtn').addEventListener('click', () => this.openImportModal());
        document.getElementById('closeImportModal').addEventListener('click', () => this.closeImportModal());
        document.getElementById('cancelImportBtn').addEventListener('click', () => this.closeImportModal());
        document.getElementById('confirmImportBtn').addEventListener('click', () => this.importTranslations());

        // Filter events
        document.getElementById('searchBtn').addEventListener('click', () => this.filterTranslations());
        document.getElementById('searchInput').addEventListener('keypress', (e) => {
            if (e.key === 'Enter') this.filterTranslations();
        });
        document.getElementById('platformFilter').addEventListener('change', () => this.filterTranslations());

        // Select all checkbox
        document.getElementById('selectAll').addEventListener('change', (e) => {
            const checkboxes = document.querySelectorAll('.translation-checkbox');
            checkboxes.forEach(checkbox => {
                checkbox.checked = e.target.checked;
                const id = parseInt(checkbox.dataset.id);
                if (e.target.checked) {
                    this.selectedTranslations.add(id);
                } else {
                    this.selectedTranslations.delete(id);
                }
            });
            this.updateRemoveSelectedButton();
        });

        // Event delegation for translation checkboxes
        document.addEventListener('change', (e) => {
            if (e.target.classList.contains('translation-checkbox')) {
                const id = parseInt(e.target.dataset.id);
                if (e.target.checked) {
                    this.selectedTranslations.add(id);
                } else {
                    this.selectedTranslations.delete(id);
                }
                this.updateSelectAllState();
                this.updateRemoveSelectedButton();
            }
        });

        // File type change
        document.getElementById('fileType').addEventListener('change', (e) => {
            const jsonFileDiv = document.getElementById('jsonFileDiv');
            const excelFileDiv = document.getElementById('excelFileDiv');
            const jsonImportTypeDiv = document.getElementById('jsonImportTypeDiv');
            const singleLanguageDiv = document.getElementById('singleLanguageDiv');
            
            if (e.target.value === 'excel') {
                jsonFileDiv.classList.add('hidden');
                excelFileDiv.classList.remove('hidden');
                jsonImportTypeDiv.classList.add('hidden');
                singleLanguageDiv.classList.add('hidden');
            } else {
                jsonFileDiv.classList.remove('hidden');
                excelFileDiv.classList.add('hidden');
                jsonImportTypeDiv.classList.remove('hidden');
                singleLanguageDiv.classList.remove('hidden');
            }
        });

        // Import type change
        document.getElementById('importType').addEventListener('change', (e) => {
            const singleLanguageDiv = document.getElementById('singleLanguageDiv');
            if (e.target.value === 'bulk') {
                singleLanguageDiv.style.display = 'none';
            } else {
                singleLanguageDiv.style.display = 'block';
            }
        });

        // Remove All button
        document.getElementById('removeAllBtn').addEventListener('click', () => {
            if (confirm('Are you sure you want to delete all translations?')) {
                this.removeAllTranslations();
            }
        });

        // Refresh Recent button
        document.getElementById('refreshRecentBtn').addEventListener('click', () => {
            this.loadRecentTranslations();
        });

        // Recent button
        document.getElementById('recentBtn').addEventListener('click', () => {
            this.openRecentModal();
        });

        // Close Recent Modal
        document.getElementById('closeRecentModal').addEventListener('click', () => {
            this.closeRecentModal();
        });

        // Warning filter button
        document.getElementById('showWarningsBtn').addEventListener('click', () => {
            this.toggleWarningFilter();
        });
    }

    async loadTranslations() {
        try {
            this.showLoading(true);
            const response = await fetch(`${this.apiBaseUrl}/translations`);
            if (!response.ok) throw new Error('API Error');
            
            this.translations = await response.json();
            this.selectedTranslations.clear(); // Clear selections when loading new data
            this.renderTranslations();
            this.updateStats();
        } catch (error) {
            this.showError('Çeviriler yüklenirken hata oluştu: ' + error.message);
        } finally {
            this.showLoading(false);
        }
    }

    async filterTranslations() {
        try {
            this.showLoading(true);
            const platform = document.getElementById('platformFilter').value;
            const search = document.getElementById('searchInput').value;
            
            let url = `${this.apiBaseUrl}/translations?`;
            // "all" means "All Platforms", no filtering
            if (platform !== 'all') url += `platform=${platform}&`;
            if (search) url += `search=${encodeURIComponent(search)}&`;
            
            const response = await fetch(url);
            if (!response.ok) throw new Error('API Error');
            
            this.translations = await response.json();
            this.renderTranslations();
        } catch (error) {
            this.showError('Error occurred during filtering: ' + error.message);
        } finally {
            this.showLoading(false);
        }
    }

    renderTranslations() {
        const tbody = document.getElementById('translationsTableBody');
        tbody.innerHTML = '';

        if (this.translations.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="8" class="px-6 py-4 text-center text-gray-500">
                        <i class="fas fa-inbox text-3xl mb-2"></i>
                        <p>No translations found yet</p>
                    </td>
                </tr>
            `;
            return;
        }

        this.translations.forEach(translation => {
            const hasMissing = this.hasMissingTranslations(translation);
            
            // Apply warning filter
            if (this.showWarningsOnly && !hasMissing) {
                return; // Skip this translation if showing warnings only and it's complete
            }
            
            const row = document.createElement('tr');
            row.className = 'hover:bg-gray-50 align-top';
            row.innerHTML = `
                <td class="px-2 py-2 whitespace-nowrap">
                    <input type="checkbox" class="translation-checkbox rounded" data-id="${translation.id}">
                </td>
                <td class="px-3 py-2">
                        <div class="flex items-center gap-2">
                            <div class="text-sm font-medium text-gray-900 break-words leading-tight" title="${translation.resourceKey}">${translation.resourceKey}</div>
                            ${hasMissing ? '<i class="fas fa-exclamation-triangle text-yellow-500 text-sm" title="Missing translations"></i>' : ''}
                        </div>
                </td>
                <td class="px-3 py-2">
                    <div class="text-sm text-gray-900 break-words leading-tight" title="${translation.en || ''}">${translation.en || '-'}</div>
                </td>
                <td class="px-3 py-2 whitespace-nowrap">
                    <span class="inline-flex px-1 py-0.5 text-xs font-semibold rounded-full ${
                        translation.platform === 'Android/iOS' ? 'bg-green-100 text-green-800' :
                        'bg-blue-100 text-blue-800'
                    }">
                        ${translation.platform === 'Android/iOS' ? 'Android/iOS' : 'Backend'}
                    </span>
                </td>
                <td class="px-3 py-2 whitespace-nowrap">
                    ${translation.platform === 'Backend' ? 
                        '<span class="text-gray-400 text-sm">-</span>' :
                        `<span class="inline-flex px-1 py-0.5 text-xs font-semibold rounded-full ${
                            translation.mobileSynced ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                        }">
                            ${translation.mobileSynced ? '✓' : '○'}
                        </span>`
                    }
                </td>
                    <td class="px-3 py-2 whitespace-nowrap">
                        ${hasMissing ? 
                            `<span class="inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full bg-yellow-100 text-yellow-800">
                                <i class="fas fa-exclamation-triangle mr-1 text-yellow-600"></i>Missing
                            </span>` :
                            `<span class="inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800">
                                <i class="fas fa-check-circle mr-1"></i>Complete
                            </span>`
                        }
                    </td>
                <td class="px-3 py-2">
                    <div class="text-xs text-gray-500">
                        <div class="truncate" title="${this.formatDate(translation.createdAt)}">
                            <span class="font-medium text-gray-700">C:</span> ${this.formatDate(translation.createdAt).split(' ')[0]}
                        </div>
                        <div class="truncate" title="${this.formatDate(translation.updatedAt)}">
                            <span class="font-medium text-gray-700">U:</span> ${this.formatDate(translation.updatedAt).split(' ')[0]}
                        </div>
                    </div>
                </td>
                <td class="px-3 py-2 whitespace-nowrap text-sm font-medium">
                    <button onclick="translationManager.editTranslation(${translation.id})" 
                            class="text-indigo-600 hover:text-indigo-900 mr-2" title="Edit">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button onclick="translationManager.deleteTranslation(${translation.id})" 
                            class="text-red-600 hover:text-red-900" title="Delete">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            `;
            tbody.appendChild(row);
        });

        // Update select all state after rendering
        this.updateSelectAllState();
        this.updateRemoveSelectedButton();
    }

    updateStats() {
        const total = this.translations.length;
        const backend = this.translations.filter(t => t.platform === 'Backend').length;
        const androidIos = this.translations.filter(t => t.platform === 'Android/iOS').length;

        document.getElementById('totalTranslations').textContent = total;
        document.getElementById('mobileSyncCount').textContent = backend;
        document.getElementById('androidCount').textContent = androidIos;
    }

    updateSelectAllState() {
        const selectAllCheckbox = document.getElementById('selectAll');
        const checkboxes = document.querySelectorAll('.translation-checkbox');
        
        if (this.selectedTranslations.size === 0) {
            selectAllCheckbox.indeterminate = false;
            selectAllCheckbox.checked = false;
        } else if (this.selectedTranslations.size === checkboxes.length) {
            selectAllCheckbox.indeterminate = false;
            selectAllCheckbox.checked = true;
        } else {
            selectAllCheckbox.indeterminate = true;
        }
    }

    updateRemoveSelectedButton() {
        const removeSelectedBtn = document.getElementById('removeSelectedBtn');
        removeSelectedBtn.disabled = this.selectedTranslations.size === 0;
    }

    openModal(translation = null) {
        this.currentEditingId = translation ? translation.id : null;
        
        if (translation) {
            document.getElementById('modalTitle').textContent = 'Edit Translation';
            document.getElementById('resourceKey').value = translation.resourceKey;
            document.getElementById('englishText').value = translation.en || '';
            document.getElementById('turkishText').value = translation.tr || '';
            document.getElementById('germanText').value = translation.de || '';
            document.getElementById('platformSelect').value = translation.platform;
            document.getElementById('mobileSyncCheck').checked = translation.mobileSynced;
            
            // Show/hide mobile sync div based on platform control
            const platform = translation.platform;
            const mobileSyncDiv = document.getElementById('mobileSyncDiv');
            if (platform === 'Android/iOS') {
                mobileSyncDiv.classList.remove('hidden');
            } else {
                mobileSyncDiv.classList.add('hidden');
            }
        } else {
            document.getElementById('modalTitle').textContent = 'Add New Translation';
            document.getElementById('translationForm').reset();
            
            // Hide mobile sync div for new translation
            document.getElementById('mobileSyncDiv').classList.add('hidden');
        }
        
        document.getElementById('translationModal').classList.remove('hidden');
    }

    closeModal() {
        document.getElementById('translationModal').classList.add('hidden');
        this.currentEditingId = null;
    }

    async saveTranslation(e) {
        e.preventDefault();
        
        const platform = document.getElementById('platformSelect').value;
        const formData = {
            resourceKey: document.getElementById('resourceKey').value,
            en: document.getElementById('englishText').value,
            tr: document.getElementById('turkishText').value,
            de: document.getElementById('germanText').value,
            platform: platform,
            mobileSynced: platform === 'Android/iOS' ? document.getElementById('mobileSyncCheck').checked : false
        };

        try {
            let response;
            if (this.currentEditingId) {
                // Update existing translation
                response = await fetch(`${this.apiBaseUrl}/translations/${this.currentEditingId}`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ ...formData, id: this.currentEditingId })
                });
            } else {
                // Create new translation
                response = await fetch(`${this.apiBaseUrl}/translations`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(formData)
                });
            }

            if (!response.ok) {
                const errorData = await response.text();
                throw new Error(errorData || 'API Error');
            }
            
            this.closeModal();
            this.loadTranslations();
            this.loadRecentTranslations();
            this.showSuccess(this.currentEditingId ? 'Translation updated!' : 'Translation added!');
        } catch (error) {
            this.showError('Error occurred during saving: ' + error.message);
        }
    }

    editTranslation(id) {
        const translation = this.translations.find(t => t.id === id);
        if (translation) {
            this.openModal(translation);
        }
    }

    async deleteTranslation(id) {
        if (!confirm('Are you sure you want to delete this translation?')) return;

        try {
            const response = await fetch(`${this.apiBaseUrl}/translations/${id}`, {
                method: 'DELETE'
            });

            if (!response.ok) throw new Error('API Error');
            
            this.loadTranslations();
            this.showSuccess('Translation deleted!');
        } catch (error) {
            this.showError('Error occurred during deletion: ' + error.message);
        }
    }

    openImportModal() {
        document.getElementById('importModal').classList.remove('hidden');
    }

    closeImportModal() {
        document.getElementById('importModal').classList.add('hidden');
        document.getElementById('importFile').value = '';
        document.getElementById('importExcelFile').value = '';
    }


    async importExcelTranslations() {
        const fileInput = document.getElementById('importExcelFile');
        const file = fileInput.files[0];
        
        if (!file) {
            this.showError('Please select an Excel file!');
            return;
        }

        if (!file.name.endsWith('.xlsx')) {
            this.showError('Please select a valid .xlsx file!');
            return;
        }

        try {
            const platform = document.getElementById('importPlatform').value;
            const mobileSynced = false; // Default olarak her zaman false

            const formData = new FormData();
            formData.append('file', file);
            formData.append('platform', platform);
            formData.append('mobileSynced', mobileSynced);

            const response = await fetch(`${this.apiBaseUrl}/translations/import-excel`, {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || 'Excel Import API Error');
            }
            
            const result = await response.text();
            this.showSuccess('Excel import successful! ' + result);
            
            this.closeImportModal();
            this.loadTranslations();
            this.loadRecentTranslations();
        } catch (error) {
            this.showError('Error occurred during Excel import: ' + error.message);
        }
    }

    async importTranslations() {
        const fileType = document.getElementById('fileType').value;
        
        if (fileType === 'excel') {
            await this.importExcelTranslations();
            return;
        }

        const fileInput = document.getElementById('importFile');
        const file = fileInput.files[0];
        const importType = document.getElementById('importType').value;
        
        if (!file) {
            this.showError('Please select a file!');
            return;
        }

        try {
            const text = await file.text();
            const data = JSON.parse(text);
            
            if (importType === 'bulk') {
                // Bulk import - import all languages at once
                const platform = document.getElementById('importPlatform').value;
                const bulkData = {
                    translations: {},
                    platform: platform,
                    mobileSynced: false // Default olarak her zaman false
                };

                // Check JSON format and convert
                if (data.strings) {
                    // Convert from single language format to bulk format
                    Object.keys(data.strings).forEach(key => {
                        bulkData.translations[key] = {
                            en: data.strings[key],
                            tr: '',
                            de: ''
                        };
                    });
                } else {
                    // Bulk format is already correct
                    bulkData.translations = data.translations || data;
                }

                const response = await fetch(`${this.apiBaseUrl}/translations/import-bulk`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(bulkData)
                });

                if (!response.ok) throw new Error('Bulk Import API Error');
                
                const result = await response.text();
                
                // JSON response kontrolü
                try {
                    const jsonResult = JSON.parse(result);
                    this.showSuccess(`Bulk import completed! ${jsonResult.added} new, ${jsonResult.updated} updated.`);
                } catch {
                    // Text response (empty import case)
                    this.showSuccess('Import successful! ' + result);
                }
            } else {
                // Single language import - Legacy format (old project compatible)
                // Only accept strings format
                if (!data.strings) {
                    throw new Error('Invalid JSON format! Only {"strings": {"key": "value"}} format is accepted.');
                }
                
                const platform = document.getElementById('importPlatform').value;
                const language = document.getElementById('importLanguage').value;
                const importData = {
                    strings: data.strings,
                    language: language,
                    platform: platform,
                    mobileSynced: false // Default olarak her zaman false
                };

                const response = await fetch(`${this.apiBaseUrl}/translations/import`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(importData)
                });

                if (!response.ok) throw new Error('API Error');
                
                const result = await response.text();
                this.showSuccess('Import successful! ' + result);
            }
            
            this.closeImportModal();
            this.loadTranslations();
            this.loadRecentTranslations();
        } catch (error) {
            this.showError('Error occurred during import: ' + error.message);
        }
    }

    async exportTranslations(language) {
        try {
            const response = await fetch(`${this.apiBaseUrl}/translations/export/${language}`);
            
            if (!response.ok) throw new Error('API Error');
            
            const data = await response.json();
            
            // Create download
            const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `translations_${language}_${new Date().toISOString().split('T')[0]}.json`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
            
            this.showSuccess(`${language.toUpperCase()} export successful!`);
        } catch (error) {
            this.showError('Error occurred during export: ' + error.message);
        }
    }

    async exportBulkTranslations() {
        try {
            const response = await fetch(`${this.apiBaseUrl}/translations/export-bulk`);
            
            if (!response.ok) throw new Error('API Error');
            
            const data = await response.json();
            
            // Create download
            const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `translations_all_${new Date().toISOString().split('T')[0]}.json`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
            
            this.showSuccess('Bulk export successful!');
        } catch (error) {
            this.showError('Error occurred during bulk export: ' + error.message);
        }
    }

    toggleExportDropdown() {
        const dropdown = document.getElementById('exportDropdown');
        dropdown.classList.toggle('hidden');
    }

    closeExportDropdown() {
        const dropdown = document.getElementById('exportDropdown');
        dropdown.classList.add('hidden');
    }


    showLoading(show) {
        const spinner = document.getElementById('loadingSpinner');
        const tableBody = document.getElementById('translationsTableBody');
        
        if (show) {
            spinner.classList.remove('hidden');
            tableBody.style.display = 'none';
        } else {
            spinner.classList.add('hidden');
            tableBody.style.display = '';
        }
    }

    showSuccess(message) {
        this.showNotification(message, 'success');
    }

    showError(message) {
        this.showNotification(message, 'error');
    }

    showNotification(message, type) {
        // Create notification element
        const notification = document.createElement('div');
        notification.className = `fixed top-4 right-4 z-50 p-4 rounded-lg shadow-lg ${
            type === 'success' ? 'bg-green-500 text-white' : 'bg-red-500 text-white'
        }`;
        notification.innerHTML = `
            <div class="flex items-center">
                <i class="fas ${type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle'} mr-2"></i>
                <span>${message}</span>
            </div>
        `;
        
        document.body.appendChild(notification);
        
        // Remove after 3 seconds
        setTimeout(() => {
            notification.remove();
        }, 3000);
    }

    async loadRecentTranslations() {
        try {
            const response = await fetch(`${this.apiBaseUrl}/translations/latest`);
            if (!response.ok) throw new Error('API Error');
            
            const recentTranslations = await response.json();
            this.renderRecentTranslations(recentTranslations);
        } catch (error) {
            console.error('Error loading recent translations:', error);
        }
    }

    renderRecentTranslations(translations) {
        const container = document.getElementById('recentTranslations');
        
        if (translations.length === 0) {
            container.innerHTML = `
                <tr>
                    <td colspan="6" class="px-6 py-4 text-center text-gray-500">
                        <i class="fas fa-inbox text-2xl mb-2"></i>
                        <p>No recent translations</p>
                    </td>
                </tr>
            `;
            return;
        }

        container.innerHTML = translations.map(translation => `
            <tr class="hover:bg-gray-50">
                <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm font-medium text-gray-900">${translation.resourceKey}</div>
                </td>
                <td class="px-6 py-4">
                    <div class="text-sm text-gray-900 break-words max-w-xs">
                        ${translation.en ? translation.en.substring(0, 100) + (translation.en.length > 100 ? '...' : '') : 'No English text'}
                    </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                    <span class="inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                        translation.platform === 'Android/iOS' ? 'bg-green-100 text-green-800' : 'bg-blue-100 text-blue-800'
                    }">
                        ${translation.platform === 'Android/iOS' ? 'Android/iOS' : 'Backend'}
                    </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                    ${translation.platform === 'Backend' ? 
                        '<span class="text-gray-400 text-sm">-</span>' :
                        `<span class="inline-flex px-2 py-1 text-xs font-semibold rounded-full ${
                            translation.mobileSynced ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                        }">
                            ${translation.mobileSynced ? '✓' : '○'}
                        </span>`
                    }
                </td>
                    <td class="px-6 py-4 whitespace-nowrap">
                        ${this.hasMissingTranslations(translation) ? 
                            `<span class="inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full bg-yellow-100 text-yellow-800">
                                <i class="fas fa-exclamation-triangle mr-1 text-yellow-600"></i>Missing
                            </span>` :
                            `<span class="inline-flex items-center px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800">
                                <i class="fas fa-check-circle mr-1"></i>Complete
                            </span>`
                        }
                    </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    ${this.formatDate(translation.updatedAt)}
                </td>
            </tr>
        `).join('');
    }

    openRecentModal() {
        document.getElementById('recentModal').classList.remove('hidden');
        this.loadRecentTranslations();
    }

    closeRecentModal() {
        document.getElementById('recentModal').classList.add('hidden');
    }

    updateSelectAllState() {
        const selectAllCheckbox = document.getElementById('selectAll');
        const checkboxes = document.querySelectorAll('.translation-checkbox');
        
        if (this.selectedTranslations.size === 0) {
            selectAllCheckbox.indeterminate = false;
            selectAllCheckbox.checked = false;
        } else if (this.selectedTranslations.size === checkboxes.length) {
            selectAllCheckbox.indeterminate = false;
            selectAllCheckbox.checked = true;
        } else {
            selectAllCheckbox.indeterminate = true;
        }
    }

    updateRemoveSelectedButton() {
        const removeSelectedBtn = document.getElementById('removeSelectedBtn');
        removeSelectedBtn.disabled = this.selectedTranslations.size === 0;
    }

    async removeSelectedTranslations() {
        if (this.selectedTranslations.size === 0) {
            this.showError('No translations selected!');
            return;
        }

        if (!confirm(`Are you sure you want to remove ${this.selectedTranslations.size} selected translations? This action cannot be undone!`)) {
            return;
        }

        try {
            const ids = Array.from(this.selectedTranslations);
            const response = await fetch(`${this.apiBaseUrl}/translations/selected`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(ids)
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.text();
            try {
                const jsonResult = JSON.parse(result);
                this.showSuccess(jsonResult.message || `${ids.length} translations removed successfully!`);
            } catch {
                this.showSuccess(result || `${ids.length} translations removed successfully!`);
            }
            this.selectedTranslations.clear();
            await this.loadTranslations();
        } catch (error) {
            console.error('Remove selected error:', error);
            this.showError('Error removing selected translations: ' + error.message);
        }
    }

    async removeAllTranslations() {
        if (!confirm('Are you sure you want to remove all translations? This action cannot be undone!')) {
            return;
        }

        try {
            const response = await fetch(`${this.apiBaseUrl}/translations/all`, {
                method: 'DELETE'
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            this.showSuccess('All translations removed successfully!');
            await this.loadTranslations();
        } catch (error) {
            console.error('Remove all error:', error);
            this.showError('Error removing all translations: ' + error.message);
        }
    }

    // Check if translation has missing languages
    hasMissingTranslations(translation) {
        return !translation.tr || !translation.de;
    }

    // Get status text for translation
    getTranslationStatus(translation) {
        return this.hasMissingTranslations(translation) ? 'Missing' : 'Complete';
    }

    // Toggle warning filter
    toggleWarningFilter() {
        this.showWarningsOnly = !this.showWarningsOnly;
        this.updateWarningFilterButton();
        this.renderTranslations();
    }

    // Update warning filter button appearance
    updateWarningFilterButton() {
        const btn = document.getElementById('showWarningsBtn');
        if (this.showWarningsOnly) {
            btn.className = 'w-full bg-yellow-50 border-yellow-200 text-yellow-700 px-4 py-2 rounded-lg hover:bg-yellow-100 transition-colors';
            btn.innerHTML = '<i class="fas fa-exclamation-triangle mr-2 text-yellow-600"></i>Show All';
        } else {
            btn.className = 'w-full bg-yellow-50 border-yellow-200 text-yellow-700 px-4 py-2 rounded-lg hover:bg-yellow-100 transition-colors';
            btn.innerHTML = '<i class="fas fa-exclamation-triangle mr-2 text-yellow-600"></i>Show Warnings Only';
        }
    }

    formatDate(dateString) {
        if (!dateString) return '-';
        const date = new Date(dateString);
        return date.toLocaleString('tr-TR', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    }
}

// Initialize the application
const translationManager = new TranslationManager();

// Event listeners
document.addEventListener('DOMContentLoaded', () => {
    // Remove Selected button
    document.getElementById('removeSelectedBtn').addEventListener('click', () => {
        translationManager.removeSelectedTranslations();
    });
    
    // Mobile sync control when platform selection changes
    document.getElementById('platformSelect').addEventListener('change', function() {
        const platform = this.value;
        const mobileSyncDiv = document.getElementById('mobileSyncDiv');
        
        if (platform === 'Android/iOS') {
            mobileSyncDiv.classList.remove('hidden');
        } else {
            mobileSyncDiv.classList.add('hidden');
            document.getElementById('mobileSyncCheck').checked = false;
        }
    });
    
    // Export dropdown
    document.getElementById('exportBtn').addEventListener('click', (e) => {
        e.stopPropagation();
        translationManager.toggleExportDropdown();
    });
    
    // Export All Languages
    document.getElementById('exportAllBtn').addEventListener('click', () => {
        translationManager.exportBulkTranslations();
        translationManager.closeExportDropdown();
    });
    
    // Export English
    document.getElementById('exportEnBtn').addEventListener('click', () => {
        translationManager.exportTranslations('en');
        translationManager.closeExportDropdown();
    });
    
    // Export Turkish
    document.getElementById('exportTrBtn').addEventListener('click', () => {
        translationManager.exportTranslations('tr');
        translationManager.closeExportDropdown();
    });
    
    // Export German
    document.getElementById('exportDeBtn').addEventListener('click', () => {
        translationManager.exportTranslations('de');
        translationManager.closeExportDropdown();
    });
    
    // Close dropdown when clicking outside
    document.addEventListener('click', () => {
        translationManager.closeExportDropdown();
    });
    
    // Remove All button listener is now in bindEvents
    // Select All checkbox listener is now in bindEvents
});
