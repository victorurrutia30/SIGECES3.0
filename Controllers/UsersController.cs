using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGECES.Data;
using SIGECES.Models;
using SIGECES.Models.ViewModels;
using SIGECES.Services;

namespace SIGECES.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View(users);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            var model = new AdminUserViewModel
            {
                IsActive = true
            };

            return View(model);
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminUserViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "La contraseña es obligatoria para nuevos usuarios.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validar email único
            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Ya existe un usuario con ese correo.");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Role = model.Role,
                IsActive = model.IsActive,
                PasswordHash = PasswordHasher.HashPassword(model.Password!)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _context.Users.FindAsync(id.Value);
            if (user == null)
                return NotFound();

            var model = new AdminUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
                // Password y ConfirmPassword se dejan vacías
            };

            return View(model);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminUserViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Validar email único (excluyendo el propio usuario)
            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email && u.Id != user.Id);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Ya existe otro usuario con ese correo.");
                return View(model);
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Role = model.Role;
            user.IsActive = model.IsActive;

            // Si se ingresó nueva contraseña, se actualiza
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                user.PasswordHash = PasswordHasher.HashPassword(model.Password);
            }

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error actualizando usuario {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar los cambios.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Delete/5  (en realidad: desactivar)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id.Value);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: Users/Delete/5  (soft delete: IsActive = false)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return RedirectToAction(nameof(Index));

            // Soft delete
            user.IsActive = false;

            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
