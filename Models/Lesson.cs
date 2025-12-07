using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models;

public class Lesson
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El curso es obligatorio.")]
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    [Required(ErrorMessage = "Por favor ingresa el título de la lección.")]
    [StringLength(200, ErrorMessage = "El título no puede tener más de {1} caracteres.")]
    [Display(Name = "Título de la lección")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Por favor ingresa una descripción para la lección.")]
    [StringLength(2000, ErrorMessage = "La descripción no puede tener más de {1} caracteres.")]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [StringLength(500, ErrorMessage = "La URL no puede tener más de {1} caracteres.")]
    [Url(ErrorMessage = "La URL del recurso no es válida. Debe empezar con http:// o https://.")]
    [Display(Name = "URL del recurso")]
    public string? ResourceUrl { get; set; }

    [Required(ErrorMessage = "El orden es obligatorio.")]
    [Range(1, 9999, ErrorMessage = "El orden debe ser un número entre {1} y {2}.")]
    [Display(Name = "Orden")]
    public int Order { get; set; } = 1;
}
