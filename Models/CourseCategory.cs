using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models;

public class CourseCategory
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Nombre de la categoría")]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Display(Name = "Activa")]
    public bool IsActive { get; set; } = true;

    // Navegación
    public ICollection<Course>? Courses { get; set; }
}
