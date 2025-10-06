using Microsoft.Extensions.Logging;
using TranslationManager.API.Interfaces;
using TranslationManager.API.Models;

namespace TranslationManager.API.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly ITranslationRepository _repository;
        private readonly ILogger<TranslationService> _logger;

        public TranslationService(ITranslationRepository repository, ILogger<TranslationService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<Translation>> GetTranslationsAsync(string? platform, string? search)
        {
            try
            {
                _logger.LogInformation("Getting translations with platform: {Platform}, search: {Search}", platform, search);
                
                if (!string.IsNullOrEmpty(platform) && platform != "all")
                {
                    return await _repository.GetByPlatformAsync(platform);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    return await _repository.SearchAsync(search);
                }

                return await _repository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting translations with platform: {Platform}, search: {Search}", platform, search);
                throw;
            }
        }

        public async Task<IEnumerable<Translation>> GetRecentTranslationsAsync()
        {
            try
            {
                _logger.LogInformation("Getting recent translations");
                return await _repository.GetRecentAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent translations");
                throw;
            }
        }

        public async Task<Translation?> GetTranslationByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting translation by ID: {Id}", id);
                return await _repository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting translation by ID: {Id}", id);
                throw;
            }
        }

        public async Task<Translation> CreateTranslationAsync(Translation translation)
        {
            try
            {
                _logger.LogInformation("Creating translation with resource key: {ResourceKey}", translation.ResourceKey);
                
                if (await _repository.ExistsByResourceKeyAsync(translation.ResourceKey))
                {
                    _logger.LogWarning("Translation with resource key already exists: {ResourceKey}", translation.ResourceKey);
                    throw new InvalidOperationException("Resource key already exists");
                }

                var result = await _repository.CreateAsync(translation);
                _logger.LogInformation("Successfully created translation with ID: {Id}", result.Id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating translation with resource key: {ResourceKey}", translation.ResourceKey);
                throw;
            }
        }

        public async Task<Translation> UpdateTranslationAsync(int id, Translation translation)
        {
            try
            {
                _logger.LogInformation("Updating translation with ID: {Id}", id);
                
                if (!await _repository.ExistsAsync(id))
                {
                    _logger.LogWarning("Translation with ID not found: {Id}", id);
                    throw new KeyNotFoundException($"Translation with id {id} not found");
                }

                translation.Id = id;
                var result = await _repository.UpdateAsync(translation);
                _logger.LogInformation("Successfully updated translation with ID: {Id}", id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating translation with ID: {Id}", id);
                throw;
            }
        }

        public async Task DeleteTranslationAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting translation with ID: {Id}", id);
                
                if (!await _repository.ExistsAsync(id))
                {
                    _logger.LogWarning("Translation with ID not found for deletion: {Id}", id);
                    throw new KeyNotFoundException($"Translation with id {id} not found");
                }

                await _repository.DeleteAsync(id);
                _logger.LogInformation("Successfully deleted translation with ID: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting translation with ID: {Id}", id);
                throw;
            }
        }

        public async Task DeleteSelectedTranslationsAsync(IEnumerable<int> ids)
        {
            try
            {
                var idList = ids.ToList();
                _logger.LogInformation("Deleting {Count} selected translations", idList.Count);
                
                if (!idList.Any())
                {
                    _logger.LogWarning("No IDs provided for deletion");
                    throw new ArgumentException("No IDs provided");
                }

                await _repository.DeleteRangeAsync(idList);
                _logger.LogInformation("Successfully deleted {Count} translations", idList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting selected translations");
                throw;
            }
        }

        public async Task DeleteAllTranslationsAsync()
        {
            try
            {
                _logger.LogWarning("Deleting ALL translations - this is a destructive operation");
                await _repository.DeleteAllAsync();
                _logger.LogWarning("Successfully deleted ALL translations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all translations");
                throw;
            }
        }
    }
}