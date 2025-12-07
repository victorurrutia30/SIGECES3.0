using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models;

public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Por favor ingresa el nombre completo.")]
    [StringLength(120, ErrorMessage = "El nombre completo no puede tener más de {1} caracteres.")]
    [Display(Name = "Nombre completo")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Por favor ingresa el correo electrónico.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    [StringLength(200, ErrorMessage = "El correo electrónico no puede tener más de {1} caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [Display(Name = "Contraseña (hash)")]
    public string PasswordHash { get; set; } = null!;

    [Required(ErrorMessage = "Selecciona un rol para el usuario.")]
    [Display(Name = "Rol")]
    public UserRole Role { get; set; }

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;

    // Navegación
    public ICollection<Course>? CoursesTaught { get; set; }
    public ICollection<Enrollment>? Enrollments { get; set; }
}
