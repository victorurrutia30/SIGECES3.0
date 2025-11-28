using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models;

public class Lesson
{
    public int Id { get; set; }

    [Required]
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Título de la lección")]
    public string Title { get; set; } = null!;

    [StringLength(2000)]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [StringLength(500)]
    [Display(Name = "URL del recurso")]
    public string? ResourceUrl { get; set; }

    [Display(Name = "Orden")]
    public int Order { get; set; } = 1;
}
