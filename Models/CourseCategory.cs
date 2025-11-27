using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models;

public class CourseCategory
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Navegación
    public ICollection<Course>? Courses { get; set; }
}
