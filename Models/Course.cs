using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models;

public class Course
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Título del curso")]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(2000)]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = null!;

    [Required]
    [Display(Name = "Categoría")]
    public int CategoryId { get; set; }
    public CourseCategory? Category { get; set; }

    [Required]
    [Display(Name = "Instructor")]
    public int InstructorId { get; set; }
    public User? Instructor { get; set; }

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;

    // Navegación
    public ICollection<Lesson>? Lessons { get; set; }
    public ICollection<Enrollment>? Enrollments { get; set; }
}
