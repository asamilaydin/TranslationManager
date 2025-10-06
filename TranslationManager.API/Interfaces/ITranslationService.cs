using TranslationManager.API.Models;

namespace TranslationManager.API.Interfaces
{
    public interface ITranslationService
    {
        Task<IEnumerable<Translation>> GetTranslationsAsync(string? platform, string? search);
        Task<IEnumerable<Translation>> GetRecentTranslationsAsync();
        Task<Translation?> GetTranslationByIdAsync(int id);
        Task<Translation> CreateTranslationAsync(Translation translation);
        Task<Translation> UpdateTranslationAsync(int id, Translation translation);
        Task DeleteTranslationAsync(int id);
        Task DeleteSelectedTranslationsAsync(IEnumerable<int> ids);
        Task DeleteAllTranslationsAsync();
    }
}
