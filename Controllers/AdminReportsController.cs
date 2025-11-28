using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGECES.Data;
using SIGECES.Models;

namespace SIGECES.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminReportsController> _logger;

        public AdminReportsController(ApplicationDbContext context, ILogger<AdminReportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Pantalla simple con links de exportación
        public IActionResult Index()
        {
            return View();
        }

        // ==============
        // CSV: USUARIOS
        // ==============
        public async Task<IActionResult> ExportUsersCsv()
        {
            var users = await _context.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("UserId;Nombre;Email;Rol;Activo");

            foreach (var u in users)
            {
                var activo = u.IsActive ? "Sí" : "No";

                sb.AppendLine(
                    $"{u.Id};" +
                    $"\"{u.FullName}\";" +
                    $"\"{u.Email}\";" +
                    $"{u.Role};" +
                    $"{activo}"
                );
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"usuarios_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }

        // ==============
        // CSV: CURSOS
        // ==============
        public async Task<IActionResult> ExportCoursesCsv()
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("CourseId;Título;Categoría;Instructor;Inscripciones;Completados;PorcentajeCompletado");

            foreach (var c in courses)
            {
                int total = c.Enrollments?.Count ?? 0;
                int completed = c.Enrollments?.Count(e => e.Status == EnrollmentStatus.Completed) ?? 0;
                int percent = total == 0 ? 0 : (completed * 100 / total);

                sb.AppendLine(
                    $"{c.Id};" +
                    $"\"{c.Title}\";" +
                    $"\"{c.Category?.Name}\";" +
                    $"\"{c.Instructor?.FullName}\";" +
                    $"{total};" +
                    $"{completed};" +
                    $"{percent}"
                );
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"cursos_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }

        // =================
        // CSV: INSCRIPCIONES
        // =================
        public async Task<IActionResult> ExportEnrollmentsCsv()
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)!.ThenInclude(c => c.Category)
                .Include(e => e.Student)
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
            var fileName = $"inscripciones_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }
    }
}
