using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models;

public class Course
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = null!;

    // Relación con categoría
    [Required]
    public int CategoryId { get; set; }
    public CourseCategory? Category { get; set; }

    // Instructor asignado
    [Required]
    public int InstructorId { get; set; }
    public User? Instructor { get; set; }

    public bool IsActive { get; set; } = true;

    // Navegación
    public ICollection<Lesson>? Lessons { get; set; }
    public ICollection<Enrollment>? Enrollments { get; set; }
}
