using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models;

public class Course
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Por favor ingresa el título del curso.")]
    [StringLength(200, ErrorMessage = "El título del curso no puede tener más de {1} caracteres.")]
    [Display(Name = "Título del curso")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Por favor ingresa una descripción para el curso.")]
    [StringLength(2000, ErrorMessage = "La descripción no puede tener más de {1} caracteres.")]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Selecciona una categoría para el curso.")]
    [Display(Name = "Categoría")]
    public int CategoryId { get; set; }
    public CourseCategory? Category { get; set; }

    [Required(ErrorMessage = "Selecciona un instructor para el curso.")]
    [Display(Name = "Instructor")]
    public int InstructorId { get; set; }
    public User? Instructor { get; set; }

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;

    // Navegación
    public ICollection<Lesson>? Lessons { get; set; }
    public ICollection<Enrollment>? Enrollments { get; set; }
}
