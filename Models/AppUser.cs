using System.ComponentModel.DataAnnotations;

namespace Damanak.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الجوال مطلوب")]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(20)]
        public string? NationalId { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(20)]
        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<Guarantee> CreatedGuarantees { get; set; } = new();
    }
}