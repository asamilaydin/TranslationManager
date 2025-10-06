using System.ComponentModel.DataAnnotations;

namespace TranslationManager.API.Models
{
    public class LegacyImportRequest
    {
        [Required]
        public Dictionary<string, string>? Strings { get; set; }
        
        public string? Language { get; set; }
        
        public string? Platform { get; set; }
        
        public bool? MobileSynced { get; set; }
    }
}
