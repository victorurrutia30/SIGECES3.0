using System.ComponentModel.DataAnnotations;
using SIGECES.Models;

namespace SIGECES.Models.ViewModels
{
    public class AdminUserViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(120)]
        [Display(Name = "Nombre completo")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Confirmar contraseña")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string? ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Rol")]
        public UserRole Role { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;
    }
}
