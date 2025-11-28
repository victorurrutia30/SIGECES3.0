using SIGECES.Models;
using System.Collections.Generic;

namespace SIGECES.Models.ViewModels
{
    public class CourseReportItem
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;

        public int Enrollments { get; set; }
        public int Completed { get; set; }

        public int CompletionRatePercent =>
            Enrollments == 0 ? 0 : (Completed * 100 / Enrollments);
    }

    public class EnrollmentReportItem
    {
        public int EnrollmentId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public EnrollmentStatus Status { get; set; }
        public DateTime EnrolledAt { get; set; }
    }

    public class AdminDashboardViewModel
    {
        // Usuarios
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public Dictionary<UserRole, int> UsersByRole { get; set; } = new();

        // Cursos
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }

        // Inscripciones
        public int TotalEnrollments { get; set; }
        public int CompletedEnrollments { get; set; }

        // Estadísticas por curso
        public List<CourseReportItem> CourseStats { get; set; } = new();
    }
}
