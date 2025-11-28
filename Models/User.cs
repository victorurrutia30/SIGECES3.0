using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    [Display(Name = "Nombre completo")]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = null!;

    [Required]
    [Display(Name = "Contraseña (hash)")]
    public string PasswordHash { get; set; } = null!;

    [Required]
    [Display(Name = "Rol")]
    public UserRole Role { get; set; }

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;

    // Navegación
    public ICollection<Course>? CoursesTaught { get; set; }
    public ICollection<Enrollment>? Enrollments { get; set; }
}
