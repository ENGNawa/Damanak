using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Damanak.Models
{
    public class Guarantee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المنتج مطلوب")]
        [StringLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "التصنيف مطلوب")]
        [StringLength(80)]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم المتجر مطلوب")]
        [StringLength(100)]
        public string StoreName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? ModelName { get; set; }

        [StringLength(100)]
        public string? SerialNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "تاريخ الشراء مطلوب")]
        public DateTime PurchaseDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "تاريخ انتهاء الضمان مطلوب")]
        public DateTime WarrantyEndDate { get; set; } = DateTime.Today.AddYears(1);

        [StringLength(30)]
        public string Status { get; set; } = "Active";

        public string? InvoiceImagePath { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? AppUserId { get; set; }

        public AppUser? AppUser { get; set; }

        public List<GuaranteeActivity> Activities { get; set; } = new();
    }
}