using TranslationManager.API.Models;

namespace TranslationManager.API.Interfaces
{
    public interface IImportService
    {
        Task<string> ImportSingleAsync(LegacyImportRequest request);
        Task<object> ImportBulkAsync(BulkImportRequest request);
        Task<string> ImportExcelAsync(IFormFile file, string? platform, bool? mobileSynced);
    }
}
