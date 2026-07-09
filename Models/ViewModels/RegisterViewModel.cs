using System.ComponentModel.DataAnnotations;

namespace Damanak.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الجوال مطلوب")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string Email { get; set; } = string.Empty;

        public string? City { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [Compare("Password", ErrorMessage = "كلمة المرور غير متطابقة")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}