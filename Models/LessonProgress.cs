using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models
{
    public class LessonProgress
    {
        public int Id { get; set; }

        [Required]
        public int LessonId { get; set; }
        public Lesson? Lesson { get; set; }

        [Required]
        public int StudentId { get; set; }
        public User? Student { get; set; }

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
