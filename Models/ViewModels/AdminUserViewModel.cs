using System.ComponentModel.DataAnnotations;
using SIGECES.Models;

namespace SIGECES.Models.ViewModels
{
    public class AdminUserViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Por favor ingresa el nombre completo del usuario.")]
        [StringLength(120, ErrorMessage = "El nombre completo no puede tener más de {1} caracteres.")]
        [Display(Name = "Nombre completo")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Por favor ingresa el correo electrónico.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
        [StringLength(200, ErrorMessage = "El correo electrónico no puede tener más de {1} caracteres.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "La contraseña debe tener al menos {2} caracteres.")]
        public string? Password { get; set; }

        [Display(Name = "Confirmar contraseña")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Selecciona un rol para el usuario.")]
        [Display(Name = "Rol")]
        public UserRole Role { get; set; }

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;
    }
}
