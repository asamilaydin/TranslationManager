using TranslationManager.API.Models;

namespace TranslationManager.API.Interfaces
{
    public interface ITranslationRepository
    {
        Task<IEnumerable<Translation>> GetAllAsync();
        Task<IEnumerable<Translation>> GetByPlatformAsync(string platform);
        Task<IEnumerable<Translation>> SearchAsync(string searchTerm);
        Task<IEnumerable<Translation>> GetRecentAsync(int limit = 50);
        Task<Translation?> GetByIdAsync(int id);
        Task<Translation?> GetByResourceKeyAsync(string resourceKey);
        Task<IEnumerable<Translation>> GetByResourceKeysAsync(IEnumerable<string> resourceKeys);
        Task<Translation> CreateAsync(Translation translation);
        Task<IEnumerable<Translation>> CreateRangeAsync(IEnumerable<Translation> translations);
        Task<Translation> UpdateAsync(Translation translation);
        Task<IEnumerable<Translation>> UpdateRangeAsync(IEnumerable<Translation> translations);
        Task DeleteAsync(int id);
        Task DeleteRangeAsync(IEnumerable<int> ids);
        Task DeleteAllAsync();
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByResourceKeyAsync(string resourceKey);
    }
}
