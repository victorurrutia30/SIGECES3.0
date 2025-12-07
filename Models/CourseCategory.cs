using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models;

public class CourseCategory
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Por favor ingresa el nombre de la categoría.")]
    [StringLength(100, ErrorMessage = "El nombre de la categoría no puede tener más de {1} caracteres.")]
    [Display(Name = "Nombre de la categoría")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "La descripción no puede tener más de {1} caracteres.")]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Display(Name = "Activa")]
    public bool IsActive { get; set; } = true;

    // Navegación
    public ICollection<Course>? Courses { get; set; }
}
