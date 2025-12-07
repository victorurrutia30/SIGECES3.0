using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models;

public class Enrollment
{
    public int Id { get; set; }

    [Required]
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    [Required]
    public int StudentId { get; set; }
    public User? Student { get; set; }

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.InProgress;
}

