using System.ComponentModel.DataAnnotations;

namespace TranslationManager.API.Models
{
    public class BulkImportRequest
    {
        [Required]
        public Dictionary<string, Dictionary<string, string>> Translations { get; set; } = new();
        
        public string? Platform { get; set; }
        
        public bool? MobileSynced { get; set; }
    }
}
