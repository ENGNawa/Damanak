using System.ComponentModel.DataAnnotations;

namespace Damanak.Models
{
    public class GuaranteeActivity
    {
        public int Id { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int GuaranteeId { get; set; }

        public Guarantee? Guarantee { get; set; }
    }
}