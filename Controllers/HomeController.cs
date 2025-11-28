using System.Diagnostics;
using System.Security.Claims;
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
            // ======================
            // Admin: KPIs globales
            // ======================
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
            // ===========================
            // Instructor: KPIs personales
            // ===========================
            else if (User.IsInRole("Instructor"))
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int instructorId))
                {
                    var myCourses = await _context.Courses
                        .Include(c => c.Enrollments)
                        .Where(c => c.InstructorId == instructorId)
                        .ToListAsync();

                    var myCourseIds = myCourses.Select(c => c.Id).ToList();

                    var myEnrollmentsQuery = _context.Enrollments
                        .Where(e => myCourseIds.Contains(e.CourseId));

                    int myCoursesCount = myCourses.Count;
                    int myActiveCourses = myCourses.Count(c => c.IsActive);
                    int myTotalEnrollments = await myEnrollmentsQuery.CountAsync();
                    int myCompletedEnrollments = await myEnrollmentsQuery
                        .CountAsync(e => e.Status == EnrollmentStatus.Completed);

                    int myCompletionPercent = myTotalEnrollments == 0
                        ? 0
                        : (myCompletedEnrollments * 100 / myTotalEnrollments);

                    ViewBag.MyCoursesCount = myCoursesCount;
                    ViewBag.MyActiveCourses = myActiveCourses;
                    ViewBag.MyTotalEnrollments = myTotalEnrollments;
                    ViewBag.MyCompletedEnrollments = myCompletedEnrollments;
                    ViewBag.MyCompletionPercent = myCompletionPercent;
                }
            }
            // ==========================
            // Student: dashboard propio
            // ==========================
            else if (User.IsInRole("Student"))
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int studentId))
                {
                    // Todas sus inscripciones
                    var myEnrollments = await _context.Enrollments
                        .Include(e => e.Course)
                        .Where(e => e.StudentId == studentId)
                        .ToListAsync();

                    int myTotalEnrollments = myEnrollments.Count;
                    int myInProgress = myEnrollments.Count(e => e.Status == EnrollmentStatus.InProgress);
                    int myCompleted = myEnrollments.Count(e => e.Status == EnrollmentStatus.Completed);

                    // Cursos activos en los que está inscrito
                    var myActiveCourseIds = myEnrollments
                        .Where(e => e.Course != null && e.Course.IsActive)
                        .Select(e => e.CourseId)
                        .Distinct()
                        .ToList();

                    int totalLessons = 0;
                    int completedLessons = 0;
                    int lessonsPercent = 0;

                    if (myActiveCourseIds.Any())
                    {
                        totalLessons = await _context.Lessons
                            .Where(l => myActiveCourseIds.Contains(l.CourseId))
                            .CountAsync();

                        completedLessons = await _context.LessonProgresses
                            .Where(lp => lp.StudentId == studentId
                                         && myActiveCourseIds.Contains(lp.Lesson!.CourseId))
                            .CountAsync();

                        lessonsPercent = totalLessons == 0
                            ? 0
                            : (completedLessons * 100 / totalLessons);
                    }

                    ViewBag.MyTotalEnrollments_Student = myTotalEnrollments;
                    ViewBag.MyInProgressEnrollments_Student = myInProgress;
                    ViewBag.MyCompletedEnrollments_Student = myCompleted;
                    ViewBag.MyTotalLessons_Student = totalLessons;
                    ViewBag.MyCompletedLessons_Student = completedLessons;
                    ViewBag.MyLessonsCompletionPercent_Student = lessonsPercent;
                }
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
