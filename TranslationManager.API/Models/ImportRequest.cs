using System.ComponentModel.DataAnnotations;

namespace TranslationManager.API.Models
{
    public class ImportRequest
    {
        [Required]
        public Dictionary<string, string> Translations { get; set; } = new();
        
        [Required]
        public string Language { get; set; } = string.Empty;
        
        public string? Platform { get; set; }
        
        public bool? MobileSynced { get; set; }
    }
}
