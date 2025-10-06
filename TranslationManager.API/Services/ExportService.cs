using TranslationManager.API.Interfaces;
using TranslationManager.API.Models;

namespace TranslationManager.API.Services
{
    public class ExportService : IExportService
    {
        private readonly ITranslationRepository _repository;

        public ExportService(ITranslationRepository repository)
        {
            _repository = repository;
        }

        public async Task<object> ExportSingleLanguageAsync(string language)
        {
            var translations = await _repository.GetAllAsync();
            
            var exportData = new
            {
                strings = translations
                    .Where(t => !string.IsNullOrEmpty(GetTranslationByLanguage(t, language)))
                    .ToDictionary(t => t.ResourceKey, t => GetTranslationByLanguage(t, language)),
                updatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            return exportData;
        }

        public async Task<object> ExportBulkAsync()
        {
            var translations = await _repository.GetAllAsync();
            
            var exportData = new
            {
                strings = translations.ToDictionary(
                    t => t.ResourceKey,
                    t => new Dictionary<string, string>
                    {
                        { "en", t.En ?? "" },
                        { "tr", t.Tr ?? "" },
                        { "de", t.De ?? "" }
                    }.Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                     .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                ),
                updatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            return exportData;
        }

        private string GetTranslationByLanguage(Translation translation, string language)
        {
            return language.ToLower() switch
            {
                "en" => translation.En,
                "tr" => translation.Tr ?? string.Empty,
                "de" => translation.De ?? string.Empty,
                _ => string.Empty
            };
        }
    }
}
