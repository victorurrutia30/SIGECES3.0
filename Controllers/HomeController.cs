using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
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
            // Admin: KPIs + charts
            // ======================
            if (User.IsInRole("Admin"))
            {
                // --- KPIs globales ---
                var totalUsers = await _context.Users.CountAsync();
                var totalStudents = await _context.Users
                    .CountAsync(u => u.Role == UserRole.Student && u.IsActive);
                var totalInstructors = await _context.Users
                    .CountAsync(u => u.Role == UserRole.Instructor && u.IsActive);

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

                // --- Chart 1: Usuarios por rol ---
                var adminCount = await _context.Users
                    .CountAsync(u => u.Role == UserRole.Admin && u.IsActive);
                var instructorCount = totalInstructors;
                var studentCount = totalStudents;

                ViewBag.AdminUsersByRoleSeries = JsonSerializer.Serialize(new[]
                {
                    adminCount,
                    instructorCount,
                    studentCount
                });

                ViewBag.AdminUsersByRoleLabels = JsonSerializer.Serialize(new[]
                {
                    "Admins",
                    "Instructores",
                    "Estudiantes"
                });

                // --- Chart 2: Inscripciones por estado ---
                var adminInProgress = await _context.Enrollments
                    .CountAsync(e => e.Status == EnrollmentStatus.InProgress);

                ViewBag.AdminEnrollmentsStatusSeries = JsonSerializer.Serialize(new[]
                {
                    adminInProgress,
                    completedEnrollments
                });

                ViewBag.AdminEnrollmentsStatusLabels = JsonSerializer.Serialize(new[]
                {
                    "En progreso",
                    "Completadas"
                });

                // --- Chart 3: Inscripciones por curso ---
                var enrollmentsPerCourse = await _context.Enrollments
                    .Include(e => e.Course)
                    .Where(e => e.Course != null)
                    .GroupBy(e => e.Course!.Title)
                    .OrderByDescending(g => g.Count())
                    .Take(6)
                    .Select(g => new
                    {
                        CourseTitle = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                ViewBag.AdminEnrollmentsPerCourseSeries =
                    JsonSerializer.Serialize(enrollmentsPerCourse.Select(x => x.Count));

                ViewBag.AdminEnrollmentsPerCourseCategories =
                    JsonSerializer.Serialize(enrollmentsPerCourse.Select(x => x.CourseTitle));

                // --- Chart 4: Inscripciones por mes ---
                // Últimos 6 meses (incluyendo el actual)
                var now = DateTime.UtcNow;
                var firstMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-5); // hace 5 meses
                string[] monthShortNames = { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

                var rawAdminMonthly = await _context.Enrollments
                    .Where(e => e.EnrolledAt >= firstMonth)
                    .GroupBy(e => new { e.EnrolledAt.Year, e.EnrolledAt.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var adminMonthlySeries = new List<int>();
                var adminMonthlyCategories = new List<string>();

                // Construimos 6 meses seguidos (aunque no haya datos, ponemos 0)
                for (int i = 0; i < 6; i++)
                {
                    var monthDate = firstMonth.AddMonths(i);
                    var match = rawAdminMonthly.FirstOrDefault(x =>
                        x.Year == monthDate.Year && x.Month == monthDate.Month);

                    adminMonthlySeries.Add(match?.Count ?? 0);
                    adminMonthlyCategories.Add(monthShortNames[monthDate.Month - 1]);
                }

                ViewBag.AdminEnrollmentsByMonthSeries =
                    JsonSerializer.Serialize(adminMonthlySeries);

                ViewBag.AdminEnrollmentsByMonthCategories =
                    JsonSerializer.Serialize(adminMonthlyCategories);
            }
            // ===========================
            // Instructor: KPIs + charts
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

                    // --- Chart 1: Inscripciones por curso ---
                    var instructorEnrollmentsPerCourse = await myEnrollmentsQuery
                        .Include(e => e.Course)
                        .Where(e => e.Course != null)
                        .GroupBy(e => e.Course!.Title)
                        .OrderByDescending(g => g.Count())
                        .Select(g => new
                        {
                            CourseTitle = g.Key,
                            Count = g.Count()
                        })
                        .ToListAsync();

                    ViewBag.InstructorEnrollmentsPerCourseSeries =
                        JsonSerializer.Serialize(instructorEnrollmentsPerCourse.Select(x => x.Count));

                    ViewBag.InstructorEnrollmentsPerCourseCategories =
                        JsonSerializer.Serialize(instructorEnrollmentsPerCourse.Select(x => x.CourseTitle));

                    // --- Chart 2: Inscripciones por estado ---
                    var myInProgressEnrollments = await myEnrollmentsQuery
                        .CountAsync(e => e.Status == EnrollmentStatus.InProgress);

                    ViewBag.InstructorEnrollmentsStatusSeries =
                        JsonSerializer.Serialize(new[]
                        {
                            myInProgressEnrollments,
                            myCompletedEnrollments
                        });

                    ViewBag.InstructorEnrollmentsStatusLabels =
                        JsonSerializer.Serialize(new[]
                        {
                            "En progreso",
                            "Completadas"
                        });

                    // --- Chart 3: % finalización por curso ---
                    var completionPerCourse = myCourses
                        .Select(c =>
                        {
                            var total = c.Enrollments?.Count ?? 0;
                            var completed = c.Enrollments?
                                .Count(e => e.Status == EnrollmentStatus.Completed) ?? 0;

                            int percent = total == 0
                                ? 0
                                : (int)Math.Round(completed * 100.0 / total);

                            return new
                            {
                                CourseTitle = c.Title,
                                CompletionPercent = percent
                            };
                        })
                        .OrderByDescending(x => x.CompletionPercent)
                        .ToList();

                    ViewBag.InstructorCompletionPerCourseSeries =
                        JsonSerializer.Serialize(completionPerCourse.Select(x => x.CompletionPercent));

                    ViewBag.InstructorCompletionPerCourseCategories =
                        JsonSerializer.Serialize(completionPerCourse.Select(x => x.CourseTitle));

                    // --- Chart 4: Inscripciones por mes (solo mis cursos) ---
                    var now = DateTime.UtcNow;
                    var firstMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
                    string[] monthShortNames = { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

                    var rawInstructorMonthly = await myEnrollmentsQuery
                        .Where(e => e.EnrolledAt >= firstMonth)
                        .GroupBy(e => new { e.EnrolledAt.Year, e.EnrolledAt.Month })
                        .Select(g => new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            Count = g.Count()
                        })
                        .ToListAsync();

                    var instructorMonthlySeries = new List<int>();
                    var instructorMonthlyCategories = new List<string>();

                    for (int i = 0; i < 6; i++)
                    {
                        var monthDate = firstMonth.AddMonths(i);
                        var match = rawInstructorMonthly.FirstOrDefault(x =>
                            x.Year == monthDate.Year && x.Month == monthDate.Month);

                        instructorMonthlySeries.Add(match?.Count ?? 0);
                        instructorMonthlyCategories.Add(monthShortNames[monthDate.Month - 1]);
                    }

                    ViewBag.InstructorEnrollmentsByMonthSeries =
                        JsonSerializer.Serialize(instructorMonthlySeries);

                    ViewBag.InstructorEnrollmentsByMonthCategories =
                        JsonSerializer.Serialize(instructorMonthlyCategories);
                }
            }
            // ==========================
            // Student: KPIs + charts
            // ==========================
            else if (User.IsInRole("Student"))
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int studentId))
                {
                    var myEnrollments = await _context.Enrollments
                        .Include(e => e.Course)
                        .Where(e => e.StudentId == studentId)
                        .ToListAsync();

                    int myTotalEnrollments = myEnrollments.Count;
                    int myInProgress = myEnrollments
                        .Count(e => e.Status == EnrollmentStatus.InProgress);
                    int myCompleted = myEnrollments
                        .Count(e => e.Status == EnrollmentStatus.Completed);

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
                                         && lp.Lesson != null
                                         && myActiveCourseIds.Contains(lp.Lesson.CourseId))
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

                    // --- Chart 1: Estado de mis cursos ---
                    ViewBag.StudentCoursesStatusSeries =
                        JsonSerializer.Serialize(new[] { myInProgress, myCompleted });

                    ViewBag.StudentCoursesStatusLabels =
                        JsonSerializer.Serialize(new[] { "En progreso", "Completados" });

                    // --- Chart 2: Progreso global de lecciones ---
                    ViewBag.StudentLessonsProgressSeries =
                        JsonSerializer.Serialize(new[] { lessonsPercent });
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
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
