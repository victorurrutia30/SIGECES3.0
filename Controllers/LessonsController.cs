using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGECES.Data;
using SIGECES.Models;

namespace SIGECES.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    public class LessonsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LessonsController> _logger;

        public LessonsController(ApplicationDbContext context, ILogger<LessonsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Lista y administra las lecciones de un curso
        public async Task<IActionResult> Manage(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound();

            if (!UserCanEditCourse(course))
                return Forbid();

            return View(course);
        }

        // GET: Lessons/Create
        public async Task<IActionResult> Create(int courseId)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound();

            if (!UserCanEditCourse(course))
                return Forbid();

            var maxOrder = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .MaxAsync(l => (int?)l.Order) ?? 0;

            var model = new Lesson
            {
                CourseId = courseId,
                Order = maxOrder + 1
            };

            ViewBag.CourseTitle = course.Title;
            return View(model);
        }

        // POST: Lessons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lesson lesson)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == lesson.CourseId);

            if (course == null)
                return NotFound();

            if (!UserCanEditCourse(course))
                return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.CourseTitle = course.Title;
                return View(lesson);
            }

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage), new { courseId = lesson.CourseId });
        }

        // GET: Lessons/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
                return NotFound();

            if (!UserCanEditCourse(lesson.Course!))
                return Forbid();

            ViewBag.CourseTitle = lesson.Course!.Title;
            return View(lesson);
        }

        // POST: Lessons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Lesson lesson)
        {
            if (id != lesson.Id)
                return NotFound();

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == lesson.CourseId);

            if (course == null)
                return NotFound();

            if (!UserCanEditCourse(course))
                return Forbid();

            if (!ModelState.IsValid)
            {
                ViewBag.CourseTitle = course.Title;
                return View(lesson);
            }

            try
            {
                _context.Update(lesson);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error actualizando la lección {LessonId}", lesson.Id);

                if (!LessonExists(lesson.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Manage), new { courseId = lesson.CourseId });
        }

        // GET: Lessons/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)!.ThenInclude(c => c.Category)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
                return NotFound();

            if (!UserCanEditCourse(lesson.Course!))
                return Forbid();

            return View(lesson);
        }

        // POST: Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
                return RedirectToAction("Index", "Courses");

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == lesson.CourseId);

            if (course == null || !UserCanEditCourse(course))
                return Forbid();

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage), new { courseId = lesson.CourseId });
        }

        private bool UserCanEditCourse(Course course)
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

        private bool LessonExists(int id)
        {
            return _context.Lessons.Any(l => l.Id == id);
        }
    }
}
