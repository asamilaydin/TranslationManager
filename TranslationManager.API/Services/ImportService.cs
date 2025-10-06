using ClosedXML.Excel;
using TranslationManager.API.Interfaces;
using TranslationManager.API.Models;

namespace TranslationManager.API.Services
{
    public class ImportService : IImportService
    {
        private readonly ITranslationRepository _repository;

        public ImportService(ITranslationRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> ImportSingleAsync(LegacyImportRequest request)
        {
            if (request.Strings == null || !request.Strings.Any())
            {
                return "No translations provided";
            }

            var platform = request.Platform ?? "Backend";
            var mobileSynced = request.MobileSynced ?? false; // Default olarak her zaman false
            var language = request.Language ?? "en";

            // PERFORMANCE FIX: Get all existing translations in one query
            var resourceKeys = request.Strings.Keys.ToList();
            var existingTranslations = await _repository.GetByResourceKeysAsync(resourceKeys);
            var existingDict = existingTranslations.ToDictionary(t => t.ResourceKey);

            var addedCount = 0;
            var updatedCount = 0;

            foreach (var kvp in request.Strings)
            {
                if (existingDict.TryGetValue(kvp.Key, out var existingTranslation))
                {
                    // Update existing translation
                    SetTranslationByLanguage(existingTranslation, language, kvp.Value);
                    existingTranslation.Platform = platform;
                    existingTranslation.MobileSynced = mobileSynced;
                    existingTranslation.UpdatedAt = DateTime.UtcNow;
                    
                    await _repository.UpdateAsync(existingTranslation);
                    updatedCount++;
                }
                else
                {
                    // Create new translation
                    var newTranslation = new Translation
                    {
                        ResourceKey = kvp.Key,
                        Platform = platform,
                        MobileSynced = mobileSynced,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    // Set the specified language field
                    SetTranslationByLanguage(newTranslation, language, kvp.Value);
                    
                    // If not English, set English to "[NEEDS TRANSLATION]" like old project
                    if (language != "en")
                    {
                        newTranslation.En = "[NEEDS TRANSLATION]";
                    }
                    
                    await _repository.CreateAsync(newTranslation);
                    addedCount++;
                }
            }

            return $"Imported {request.Strings.Count} translations";
        }

        public async Task<object> ImportBulkAsync(BulkImportRequest request)
        {
            if (request.Translations == null || !request.Translations.Any())
            {
                return "No translations provided";
            }

            var platform = request.Platform ?? "Backend";
            var mobileSynced = request.MobileSynced ?? false; // Default olarak her zaman false
            
            // PERFORMANCE FIX: Get all existing translations in one query
            var resourceKeys = request.Translations.Keys.ToList();
            var existingTranslations = await _repository.GetByResourceKeysAsync(resourceKeys);
            var existingDict = existingTranslations.ToDictionary(t => t.ResourceKey);
            
            var translationsToUpdate = new List<Translation>();
            var translationsToAdd = new List<Translation>();

            foreach (var translationData in request.Translations)
            {
                var resourceKey = translationData.Key;
                var translations = translationData.Value;

                if (existingDict.TryGetValue(resourceKey, out var existingTranslation))
                {
                    // Update existing translation
                    if (translations.ContainsKey("en"))
                        existingTranslation.En = translations["en"];
                    if (translations.ContainsKey("tr"))
                        existingTranslation.Tr = translations["tr"];
                    if (translations.ContainsKey("de"))
                        existingTranslation.De = translations["de"];
                    
                    existingTranslation.Platform = platform;
                    existingTranslation.MobileSynced = mobileSynced;
                    existingTranslation.UpdatedAt = DateTime.UtcNow;
                    translationsToUpdate.Add(existingTranslation);
                }
                else
                {
                    // Add new translation
                    var newTranslation = new Translation
                    {
                        ResourceKey = resourceKey,
                        Platform = platform,
                        MobileSynced = mobileSynced,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    if (translations.ContainsKey("en"))
                        newTranslation.En = translations["en"];
                    if (translations.ContainsKey("tr"))
                        newTranslation.Tr = translations["tr"];
                    if (translations.ContainsKey("de"))
                        newTranslation.De = translations["de"];

                    translationsToAdd.Add(newTranslation);
                }
            }

            // PERFORMANCE FIX: Batch operations
            if (translationsToAdd.Any())
            {
                await _repository.CreateRangeAsync(translationsToAdd);
            }
            
            if (translationsToUpdate.Any())
            {
                await _repository.UpdateRangeAsync(translationsToUpdate);
            }

            return new
            {
                message = "Bulk import completed successfully",
                added = translationsToAdd.Count,
                updated = translationsToUpdate.Count,
                total = translationsToAdd.Count + translationsToUpdate.Count
            };
        }

        public async Task<string> ImportExcelAsync(IFormFile file, string? platform, bool? mobileSynced)
        {
            // SECURITY FIX: Input validation
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file provided");
            }

            // SECURITY FIX: File size limit (10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                throw new ArgumentException("File size cannot exceed 10MB");
            }

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only .xlsx files are supported");
            }

            // SECURITY FIX: Validate platform
            var validPlatforms = new[] { "Backend", "Android/iOS" };
            var defaultPlatform = platform ?? "Backend";
            if (!validPlatforms.Contains(defaultPlatform))
            {
                throw new ArgumentException($"Invalid platform. Must be one of: {string.Join(", ", validPlatforms)}");
            }

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            if (worksheet == null)
            {
                throw new ArgumentException("Excel file has no worksheets");
            }

            var rowCount = worksheet.RowsUsed().Count();
            if (rowCount < 2)
            {
                throw new ArgumentException("Excel file must have at least 2 rows (header + data)");
            }

            // SECURITY FIX: Row limit (1000 rows)
            if (rowCount > 1000)
            {
                throw new ArgumentException("Excel file cannot have more than 1000 rows");
            }

            // Read header row to find column positions
            var headers = new Dictionary<string, int>();
            var headerRow = worksheet.FirstRowUsed();
            var usedRange = worksheet.RangeUsed();
            
            for (int col = 1; col <= usedRange.ColumnCount(); col++)
            {
                var header = worksheet.Cell(headerRow.RowNumber(), col).GetString()?.ToLower().Trim();
                if (!string.IsNullOrEmpty(header))
                {
                    headers[header] = col;
                }
            }

            // Validate required columns
            if (!headers.ContainsKey("resource key"))
            {
                throw new ArgumentException("Excel file must have 'Resource Key' column");
            }

            var translations = new List<Translation>();
            var defaultMobileSynced = false; // Excel import için her zaman false

            // Process data rows
            var dataRows = worksheet.RowsUsed().Skip(1); // Skip header row
            foreach (var row in dataRows)
            {
                var resourceKey = worksheet.Cell(row.RowNumber(), headers["resource key"]).GetString()?.Trim();
                if (string.IsNullOrEmpty(resourceKey))
                    continue;

                // SECURITY FIX: Validate resource key format
                if (resourceKey.Length > 255)
                {
                    throw new ArgumentException($"Resource key too long: {resourceKey}. Maximum length is 255 characters.");
                }

                if (!System.Text.RegularExpressions.Regex.IsMatch(resourceKey, @"^[a-zA-Z0-9._-]+$"))
                {
                    throw new ArgumentException($"Invalid resource key format: {resourceKey}. Only alphanumeric characters, dots, underscores, and hyphens are allowed.");
                }

                var translation = new Translation
                {
                    ResourceKey = resourceKey,
                    Platform = defaultPlatform,
                    MobileSynced = defaultMobileSynced,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Read language columns if they exist
                if (headers.ContainsKey("english") || headers.ContainsKey("en"))
                {
                    var col = headers.ContainsKey("english") ? headers["english"] : headers["en"];
                    translation.En = worksheet.Cell(row.RowNumber(), col).GetString()?.Trim() ?? "";
                }

                if (headers.ContainsKey("turkish") || headers.ContainsKey("tr"))
                {
                    var col = headers.ContainsKey("turkish") ? headers["turkish"] : headers["tr"];
                    translation.Tr = worksheet.Cell(row.RowNumber(), col).GetString()?.Trim();
                }

                if (headers.ContainsKey("german") || headers.ContainsKey("de"))
                {
                    var col = headers.ContainsKey("german") ? headers["german"] : headers["de"];
                    translation.De = worksheet.Cell(row.RowNumber(), col).GetString()?.Trim();
                }

                translations.Add(translation);
            }

            if (translations.Count == 0)
            {
                return "No valid translations found in Excel file";
            }

            // PERFORMANCE FIX: Batch operations for Excel import
            var resourceKeys = translations.Select(t => t.ResourceKey).ToList();
            var existingTranslations = await _repository.GetByResourceKeysAsync(resourceKeys);
            var existingDict = existingTranslations.ToDictionary(t => t.ResourceKey);

            var addedCount = 0;
            var updatedCount = 0;
            var translationsToAdd = new List<Translation>();
            var translationsToUpdate = new List<Translation>();

            foreach (var translation in translations)
            {
                if (existingDict.TryGetValue(translation.ResourceKey, out var existing))
                {
                    // Update existing
                    existing.En = translation.En;
                    existing.Tr = translation.Tr;
                    existing.De = translation.De;
                    existing.Platform = translation.Platform;
                    existing.MobileSynced = translation.MobileSynced;
                    existing.UpdatedAt = DateTime.UtcNow;
                    
                    translationsToUpdate.Add(existing);
                    updatedCount++;
                }
                else
                {
                    // Create new
                    translationsToAdd.Add(translation);
                    addedCount++;
                }
            }

            // Batch save operations
            if (translationsToAdd.Any())
            {
                await _repository.CreateRangeAsync(translationsToAdd);
            }
            
            if (translationsToUpdate.Any())
            {
                await _repository.UpdateRangeAsync(translationsToUpdate);
            }

            return $"Excel import completed! {addedCount} new, {updatedCount} updated translations.";
        }

        private void SetTranslationByLanguage(Translation translation, string language, string value)
        {
            switch (language.ToLower())
            {
                case "en":
                    translation.En = value;
                    break;
                case "tr":
                    translation.Tr = value;
                    break;
                case "de":
                    translation.De = value;
                    break;
            }
        }
    }
}
