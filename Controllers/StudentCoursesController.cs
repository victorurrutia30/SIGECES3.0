using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGECES.Data;
using SIGECES.Models;


namespace SIGECES.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentCoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentCoursesController> _logger;

        public StudentCoursesController(ApplicationDbContext context, ILogger<StudentCoursesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Catálogo de cursos disponibles
        // GET: StudentCourses
        // Catálogo de cursos para estudiantes con búsqueda + filtros cliente (sin paginación servidor)
        public async Task<IActionResult> Index(string? q)
        {
            var studentId = GetCurrentUserId();
            if (studentId == null)
                return Unauthorized();

            // Cursos donde el estudiante ya está inscrito
            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.StudentId == studentId.Value)
                .Select(e => e.CourseId)
                .ToListAsync();

            // Query base de cursos activos
            var coursesQuery = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Where(c => c.IsActive)
                .AsQueryable();

            // Filtro opcional en servidor (para no traer pura basura)
            string? originalSearch = q;
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();

                coursesQuery = coursesQuery.Where(c =>
                    c.Title.ToLower().Contains(term) ||
                    (c.Description != null && c.Description.ToLower().Contains(term)) ||
                    (c.Category != null && c.Category.Name.ToLower().Contains(term)) ||
                    (c.Instructor != null && c.Instructor.FullName.ToLower().Contains(term)));
            }

            var courses = await coursesQuery
                .OrderBy(c => c.Category!.Name)
                .ThenBy(c => c.Title)
                .ToListAsync();

            ViewBag.EnrolledCourseIds = enrolledCourseIds;
            ViewBag.Search = originalSearch ?? string.Empty;

            return View(courses);
        }


        // Detalle de un curso (info + lecciones)
        public async Task<IActionResult> Details(int id)
        {
            var studentId = GetCurrentUserId();
            if (studentId == null)
                return Unauthorized();

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            // Obtener progreso del estudiante
            var completedLessonIds = await _context.LessonProgresses
                .Where(lp => lp.StudentId == studentId.Value && lp.Lesson!.CourseId == id)
                .Select(lp => lp.LessonId)
                .ToListAsync();

            ViewBag.CompletedLessonIds = completedLessonIds;


            if (course == null)
                return NotFound();

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == id && e.StudentId == studentId.Value);

            ViewBag.IsEnrolled = enrollment != null;
            ViewBag.EnrollmentStatus = enrollment?.Status;

            return View(course);
        }

        // Mis cursos (lo que ya está inscrito)
        public async Task<IActionResult> MyCourses()
        {
            var studentId = GetCurrentUserId();
            if (studentId == null)
                return Unauthorized();

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)!.ThenInclude(c => c.Category)
                .Include(e => e.Course)!.ThenInclude(c => c.Instructor)
                .Where(e => e.StudentId == studentId.Value)
                .OrderBy(e => e.EnrolledAt)
                .ToListAsync();

            return View(enrollments);
        }

        // Inscribirse a un curso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var studentId = GetCurrentUserId();
            if (studentId == null)
                return Unauthorized();

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.IsActive);

            if (course == null)
                return NotFound();

            var exists = await _context.Enrollments
                .AnyAsync(e => e.CourseId == courseId && e.StudentId == studentId.Value);

            if (!exists)
            {
                var enrollment = new Enrollment
                {
                    CourseId = courseId,
                    StudentId = studentId.Value,
                    EnrolledAt = DateTime.UtcNow,
                    Status = EnrollmentStatus.InProgress
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyCourses));
        }

        private int? GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
                return userId;

            return null;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteLesson(int lessonId)
        {
            var studentId = GetCurrentUserId();
            if (studentId == null)
                return Unauthorized();

            // Validar que la lección existe y pertenece a un curso activo
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null || lesson.Course == null || !lesson.Course.IsActive)
                return NotFound();

            // Validar inscripción del estudiante
            bool enrolled = await _context.Enrollments
                .AnyAsync(e => e.CourseId == lesson.CourseId && e.StudentId == studentId.Value);

            if (!enrolled)
                return Forbid();

            // Ver si ya está completada
            bool alreadyCompleted = await _context.LessonProgresses
                .AnyAsync(lp => lp.LessonId == lessonId && lp.StudentId == studentId.Value);

            if (!alreadyCompleted)
            {
                _context.LessonProgresses.Add(new LessonProgress
                {
                    LessonId = lessonId,
                    StudentId = studentId.Value
                });

                await _context.SaveChangesAsync();
            }

            // 👉 Si ya completó TODAS las lecciones, marcar el curso como Completed
            await UpdateEnrollmentStatusIfCourseCompleted(lesson.CourseId, studentId.Value);

            return RedirectToAction(nameof(Details), new { id = lesson.CourseId });
        }

        private async Task UpdateEnrollmentStatusIfCourseCompleted(int courseId, int studentId)
        {
            // Total de lecciones del curso
            var totalLessons = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .CountAsync();

            if (totalLessons == 0)
                return;

            // Cuántas lecciones tiene marcadas como completadas este estudiante
            var completedCount = await _context.LessonProgresses
                .Where(lp => lp.StudentId == studentId && lp.Lesson!.CourseId == courseId)
                .CountAsync();

            // Si aún no ha completado todas, no hacemos nada
            if (completedCount < totalLessons)
                return;

            // Buscar la inscripción
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);

            if (enrollment == null || enrollment.Status == EnrollmentStatus.Completed)
                return;

            enrollment.Status = EnrollmentStatus.Completed;
            await _context.SaveChangesAsync();
        }





    }
}
