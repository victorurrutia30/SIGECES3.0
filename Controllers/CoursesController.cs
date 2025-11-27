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
                // Instructores solo ven sus cursos
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

            // Si es instructor, forzamos que el InstructorId sea él mismo
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

            // Instructores solo pueden editar sus propios cursos
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (role == UserRole.Instructor.ToString() &&
                int.TryParse(userIdStr, out int instructorId) &&
                course.InstructorId != instructorId)
            {
                return Forbid();
            }

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
                _context.Update(course);
                await _context.SaveChangesAsync();
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

            // Instructores solo pueden ver/eliminar sus propios cursos (si lo permites)
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (role == UserRole.Instructor.ToString() &&
                int.TryParse(userIdStr, out int instructorId) &&
                course.InstructorId != instructorId)
            {
                return Forbid();
            }

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

            var role = User.FindFirstValue(ClaimTypes.Role);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (role == UserRole.Instructor.ToString() &&
                int.TryParse(userIdStr, out int instructorId) &&
                course.InstructorId != instructorId)
            {
                return Forbid();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

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
    }
}
