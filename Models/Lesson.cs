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
    public string Title { get; set; } = null!;

    [StringLength(2000)]
    public string? Description { get; set; }

    // Puede ser link a PDF, video, etc.
    [StringLength(500)]
    public string? ResourceUrl { get; set; }

    // Orden dentro del curso
    public int Order { get; set; } = 1;
}
