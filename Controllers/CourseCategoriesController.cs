using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGECES.Data;
using SIGECES.Models;

namespace SIGECES.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CourseCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CourseCategoriesController> _logger;

        public CourseCategoriesController(ApplicationDbContext context, ILogger<CourseCategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: CourseCategories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.CourseCategories
                .Include(c => c.Courses)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // GET: CourseCategories/Create
        public IActionResult Create()
        {
            var model = new CourseCategory
            {
                IsActive = true
            };

            return View(model);
        }

        // POST: CourseCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseCategory category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            try
            {
                _context.CourseCategories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error creando categoría");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar la categoría.");
                return View(category);
            }
        }

        // GET: CourseCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.CourseCategories.FindAsync(id.Value);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: CourseCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseCategory category)
        {
            if (id != category.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error actualizando categoría {CategoryId}", category.Id);

                if (!CategoryExists(category.Id))
                    return NotFound();

                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error actualizando categoría {CategoryId}", category.Id);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar los cambios.");
                return View(category);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: CourseCategories/Delete/5
        // En realidad, desactivar: IsActive = false
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await _context.CourseCategories
                .Include(c => c.Courses)
                .FirstOrDefaultAsync(c => c.Id == id.Value);

            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: CourseCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.CourseCategories
                .Include(c => c.Courses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return RedirectToAction(nameof(Index));

            // Soft delete: marcar como inactiva
            category.IsActive = false;

            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error desactivando categoría {CategoryId}", category.Id);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al desactivar la categoría.");
                return View(category);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.CourseCategories.Any(c => c.Id == id);
        }
    }
}
