using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TranslationManager.API.Models
{
    public class Translation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string ResourceKey { get; set; } = string.Empty;

        [Required]
        public string En { get; set; } = string.Empty;

        public string? Tr { get; set; }

        public string? De { get; set; }

        [MaxLength(50)]
        public string Platform { get; set; } = "Backend";

        public bool MobileSynced { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
