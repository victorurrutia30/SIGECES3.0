using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SIGECES.Data;
using SIGECES.Models;

namespace SIGECES.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(ApplicationDbContext context, ILogger<CoursesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            var query = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .AsQueryable();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (role == UserRole.Instructor.ToString() && int.TryParse(userIdStr, out int instructorId))
            {
                // Instructores solo ven sus propios cursos
                query = query.Where(c => c.InstructorId == instructorId);
            }

            var courses = await query
                .OrderBy(c => c.Title)
                .ToListAsync();

            return View(courses);
        }

        // GET: Courses/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View(new Course { IsActive = true });
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Si es instructor, el InstructorId se forza al usuario logueado
            if (role == UserRole.Instructor.ToString() && int.TryParse(userIdStr, out int instructorId))
            {
                course.InstructorId = instructorId;
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(course.InstructorId);
                return View(course);
            }

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Curso creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses.FindAsync(id.Value);
            if (course == null)
                return NotFound();

            if (!UserCanManageCourse(course))
                return Forbid();

            await LoadDropdownsAsync(course.InstructorId);
            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course)
        {
            if (id != course.Id)
                return NotFound();

            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (role == UserRole.Instructor.ToString() && int.TryParse(userIdStr, out int instructorId))
            {
                // Instructor no puede reasignar el curso a otro instructor
                course.InstructorId = instructorId;
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(course.InstructorId);
                return View(course);
            }

            try
            {
                if (!UserCanManageCourse(course))
                    return Forbid();

                _context.Update(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Curso actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error actualizando curso {CourseId}", course.Id);

                if (!CourseExists(course.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null)
                return NotFound();

            if (!UserCanManageCourse(course))
                return Forbid();

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return RedirectToAction(nameof(Index));

            if (!UserCanManageCourse(course))
                return Forbid();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Curso eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // Gestionar inscripciones
        // =========================

        // Lista de estudiantes inscritos en un curso
        public async Task<IActionResult> Students(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments!)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            if (!UserCanManageCourse(course))
                return Forbid();

            return View(course);
        }

        // Agregar estudiante al curso POR CORREO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEnrollment(int courseId, string studentEmail)
        {
            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound();

            if (!UserCanManageCourse(course))
                return Forbid();

            if (string.IsNullOrWhiteSpace(studentEmail))
            {
                TempData["ErrorMessage"] = "Debes ingresar el correo del estudiante.";
                return RedirectToAction(nameof(Students), new { id = courseId });
            }

            studentEmail = studentEmail.Trim().ToLower();

            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == studentEmail
                                          && u.IsActive
                                          && u.Role == UserRole.Student);

            if (student == null)
            {
                TempData["ErrorMessage"] = "No se encontró un estudiante activo con ese correo.";
                return RedirectToAction(nameof(Students), new { id = courseId });
            }

            var alreadyEnrolled = course.Enrollments != null &&
                                  course.Enrollments.Any(e => e.StudentId == student.Id);
            if (alreadyEnrolled)
            {
                TempData["ErrorMessage"] = "El estudiante ya está inscrito en este curso.";
                return RedirectToAction(nameof(Students), new { id = courseId });
            }

            var enrollment = new Enrollment
            {
                CourseId = courseId,
                StudentId = student.Id,
                EnrolledAt = DateTime.UtcNow,
                Status = EnrollmentStatus.InProgress
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Estudiante inscrito correctamente.";
            return RedirectToAction(nameof(Students), new { id = courseId });
        }

        // Cambiar estado de una inscripción (InProgress / Completed)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEnrollmentStatus(int enrollmentId, EnrollmentStatus status)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
                return NotFound();

            if (enrollment.Course == null || !UserCanManageCourse(enrollment.Course))
                return Forbid();

            enrollment.Status = status;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Estado de la inscripción actualizado correctamente.";
            return RedirectToAction(nameof(Students), new { id = enrollment.CourseId });
        }

        // Quitar manualmente a un estudiante del curso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveEnrollment(int enrollmentId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
                return RedirectToAction(nameof(Index));

            if (enrollment.Course == null || !UserCanManageCourse(enrollment.Course))
                return Forbid();

            var courseId = enrollment.CourseId;

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Estudiante removido del curso.";
            return RedirectToAction(nameof(Students), new { id = courseId });
        }

        // =========================
        // Helpers privados
        // =========================

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }

        private async Task LoadDropdownsAsync(int? selectedInstructorId = null)
        {
            var categories = await _context.CourseCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var instructors = await _context.Users
                .Where(u => u.IsActive && u.Role == UserRole.Instructor)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
            ViewBag.InstructorId = new SelectList(instructors, "Id", "FullName", selectedInstructorId);
        }

        // Permisos: Admin todo, Instructor solo sus cursos
        private bool UserCanManageCourse(Course course)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (role == UserRole.Admin.ToString())
                return true;

            if (role == UserRole.Instructor.ToString() &&
                int.TryParse(userIdStr, out int instructorId))
            {
                return course.InstructorId == instructorId;
            }

            return false;
        }
    }
}
