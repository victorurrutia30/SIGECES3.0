using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIGECES.Data;
using SIGECES.Models;

namespace SIGECES.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Solo calculamos KPIs si es Admin
            if (User.IsInRole("Admin"))
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalStudents = await _context.Users.CountAsync(u => u.Role == UserRole.Student && u.IsActive);
                var totalInstructors = await _context.Users.CountAsync(u => u.Role == UserRole.Instructor && u.IsActive);

                var totalCourses = await _context.Courses.CountAsync();
                var activeCourses = await _context.Courses.CountAsync(c => c.IsActive);

                var totalEnrollments = await _context.Enrollments.CountAsync();
                var completedEnrollments = await _context.Enrollments
                    .CountAsync(e => e.Status == EnrollmentStatus.Completed);

                int completionPercent = totalEnrollments == 0
                    ? 0
                    : (completedEnrollments * 100 / totalEnrollments);

                ViewBag.TotalUsers = totalUsers;
                ViewBag.TotalStudents = totalStudents;
                ViewBag.TotalInstructors = totalInstructors;
                ViewBag.TotalCourses = totalCourses;
                ViewBag.ActiveCourses = activeCourses;
                ViewBag.TotalEnrollments = totalEnrollments;
                ViewBag.CompletedEnrollments = completedEnrollments;
                ViewBag.GlobalCompletionPercent = completionPercent;
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
