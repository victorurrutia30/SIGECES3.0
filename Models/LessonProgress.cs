using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models
{
    public class LessonProgress
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La lección es obligatoria.")]
        public int LessonId { get; set; }
        public Lesson? Lesson { get; set; }

        [Required(ErrorMessage = "El estudiante es obligatorio.")]
        public int StudentId { get; set; }
        public User? Student { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
