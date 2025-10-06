using TranslationManager.API.Models;

namespace TranslationManager.API.Interfaces
{
    public interface IExportService
    {
        Task<object> ExportSingleLanguageAsync(string language);
        Task<object> ExportBulkAsync();
    }
}
