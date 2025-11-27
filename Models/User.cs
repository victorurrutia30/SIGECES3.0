using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = null!;

    // Más adelante haremos el hash, por ahora lo dejamos como string normal
    [Required]
    public string PasswordHash { get; set; } = null!;

    [Required]
    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    // Navegación
    public ICollection<Course>? CoursesTaught { get; set; }
    public ICollection<Enrollment>? Enrollments { get; set; }
}
