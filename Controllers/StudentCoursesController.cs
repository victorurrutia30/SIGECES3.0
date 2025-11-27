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
        public async Task<IActionResult> Index()
        {
            var studentId = GetCurrentUserId();
            if (studentId == null)
                return Unauthorized();

            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Category!.Name)
                .ThenBy(c => c.Title)
                .ToListAsync();

            var enrolledIds = await _context.Enrollments
                .Where(e => e.StudentId == studentId.Value)
                .Select(e => e.CourseId)
                .ToListAsync();

            ViewBag.EnrolledCourseIds = enrolledIds;

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
    }
}
