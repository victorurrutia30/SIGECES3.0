using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGECES.Data;
using SIGECES.Models;

namespace SIGECES.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InstructorReportsController> _logger;

        public InstructorReportsController(ApplicationDbContext context, ILogger<InstructorReportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Vista general de reportes del instructor
        public async Task<IActionResult> Index()
        {
            int instructorId = GetCurrentInstructorId();

            var myCourses = await _context.Courses
                .Include(c => c.Enrollments)
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();

            var myCourseIds = myCourses.Select(c => c.Id).ToList();

            var enrollmentsQuery = _context.Enrollments
                .Where(e => myCourseIds.Contains(e.CourseId));

            int totalCourses = myCourses.Count;
            int activeCourses = myCourses.Count(c => c.IsActive);
            int totalEnrollments = await enrollmentsQuery.CountAsync();
            int completedEnrollments = await enrollmentsQuery
                .CountAsync(e => e.Status == EnrollmentStatus.Completed);

            int completionPercent = totalEnrollments == 0
                ? 0
                : (completedEnrollments * 100 / totalEnrollments);

            ViewBag.TotalCourses = totalCourses;
            ViewBag.ActiveCourses = activeCourses;
            ViewBag.TotalEnrollments = totalEnrollments;
            ViewBag.CompletedEnrollments = completedEnrollments;
            ViewBag.CompletionPercent = completionPercent;

            return View();
        }

        // ==================
        // CSV: MIS CURSOS
        // ==================
        public async Task<IActionResult> ExportCoursesCsv()
        {
            int instructorId = GetCurrentInstructorId();

            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("CourseId;Título;Categoría;Activo;Inscripciones;Completados;PorcentajeCompletado");

            foreach (var c in courses)
            {
                int total = c.Enrollments?.Count ?? 0;
                int completed = c.Enrollments?.Count(e => e.Status == EnrollmentStatus.Completed) ?? 0;
                int percent = total == 0 ? 0 : (completed * 100 / total);

                sb.AppendLine(
                    $"{c.Id};" +
                    $"\"{c.Title}\";" +
                    $"\"{c.Category?.Name}\";" +
                    $"{(c.IsActive ? "Sí" : "No")};" +
                    $"{total};" +
                    $"{completed};" +
                    $"{percent}"
                );
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"mis_cursos_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }

        // ==================
        // CSV: INSCRIPCIONES
        // SOLO DE MIS CURSOS
        // ==================
        public async Task<IActionResult> ExportEnrollmentsCsv()
        {
            int instructorId = GetCurrentInstructorId();

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)!.ThenInclude(c => c.Category)
                .Include(e => e.Student)
                .Where(e => e.Course!.InstructorId == instructorId)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("EnrollmentId;CourseId;Curso;Categoría;StudentId;Estudiante;Email;FechaInscripción;Estado");

            foreach (var e in enrollments)
            {
                var fecha = e.EnrolledAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

                sb.AppendLine(
                    $"{e.Id};" +
                    $"{e.CourseId};" +
                    $"\"{e.Course?.Title}\";" +
                    $"\"{e.Course?.Category?.Name}\";" +
                    $"{e.StudentId};" +
                    $"\"{e.Student?.FullName}\";" +
                    $"\"{e.Student?.Email}\";" +
                    $"{fecha};" +
                    $"{e.Status}"
                );
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"inscripciones_mis_cursos_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }

        // ==================
        // Helpers
        // ==================
        private int GetCurrentInstructorId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(idStr) || !int.TryParse(idStr, out var id))
            {
                throw new InvalidOperationException("No se pudo obtener el id del instructor actual.");
            }

            return id;
        }
    }
}
