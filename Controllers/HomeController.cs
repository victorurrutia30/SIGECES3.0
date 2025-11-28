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

                // ============
                // Charts Admin
                // ============

                // 1) Usuarios por rol (Admins / Instructores / Estudiantes)
                var adminCount = await _context.Users
                    .CountAsync(u => u.Role == UserRole.Admin && u.IsActive);

                var usersByRoleSeries = new[] { adminCount, totalInstructors, totalStudents };
                var usersByRoleLabels = new[] { "Admins", "Instructores", "Estudiantes" };

                ViewBag.AdminUsersByRoleSeries = JsonSerializer.Serialize(usersByRoleSeries);
                ViewBag.AdminUsersByRoleLabels = JsonSerializer.Serialize(usersByRoleLabels);

                // 2) Inscripciones por estado (En progreso / Completadas)
                var inProgressEnrollments = await _context.Enrollments
                    .CountAsync(e => e.Status == EnrollmentStatus.InProgress);

                var enrollmentsStatusSeries = new[] { inProgressEnrollments, completedEnrollments };
                var enrollmentsStatusLabels = new[] { "En progreso", "Completadas" };

                ViewBag.AdminEnrollmentsStatusSeries = JsonSerializer.Serialize(enrollmentsStatusSeries);
                ViewBag.AdminEnrollmentsStatusLabels = JsonSerializer.Serialize(enrollmentsStatusLabels);

                // 3) Inscripciones por curso (top 6)
                var enrollmentsPerCourse = await _context.Enrollments
                    .Include(e => e.Course)
                    .Where(e => e.Course != null)
                    .GroupBy(e => e.Course!.Title)
                    .Select(g => new { CourseTitle = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(6)
                    .ToListAsync();

                ViewBag.AdminEnrollmentsPerCourseCategories = JsonSerializer.Serialize(
                    enrollmentsPerCourse.Select(x => x.CourseTitle).ToArray()
                );
                ViewBag.AdminEnrollmentsPerCourseSeries = JsonSerializer.Serialize(
                    enrollmentsPerCourse.Select(x => x.Count).ToArray()
                );

                // 4) Inscripciones por mes (últimos 6 meses)
                var monthNames = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
                var now = DateTime.UtcNow;
                var startMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-5);

                var rawMonthlyAdmin = await _context.Enrollments
                    .Where(e => e.EnrolledAt >= startMonth)
                    .GroupBy(e => new { e.EnrolledAt.Year, e.EnrolledAt.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                    .ToListAsync();

                var adminMonthsLabels = new List<string>();
                var adminMonthsSeries = new List<int>();

                for (int i = 0; i < 6; i++)
                {
                    var monthDate = startMonth.AddMonths(i);
                    var label = monthNames[monthDate.Month - 1];
                    var match = rawMonthlyAdmin
                        .FirstOrDefault(x => x.Year == monthDate.Year && x.Month == monthDate.Month);

                    adminMonthsLabels.Add(label);
                    adminMonthsSeries.Add(match?.Count ?? 0);
                }

                ViewBag.AdminEnrollmentsByMonthCategories = JsonSerializer.Serialize(adminMonthsLabels);
                ViewBag.AdminEnrollmentsByMonthSeries = JsonSerializer.Serialize(adminMonthsSeries);
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

                    // ============
                    // Charts Instructor
                    // ============

                    // 1) Inscripciones por curso (top 6)
                    var instructorEnrollmentsPerCourse = await myEnrollmentsQuery
                        .Include(e => e.Course)
                        .Where(e => e.Course != null)
                        .GroupBy(e => e.Course!.Title)
                        .Select(g => new { CourseTitle = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .Take(6)
                        .ToListAsync();

                    ViewBag.InstructorEnrollmentsPerCourseCategories = JsonSerializer.Serialize(
                        instructorEnrollmentsPerCourse.Select(x => x.CourseTitle).ToArray()
                    );
                    ViewBag.InstructorEnrollmentsPerCourseSeries = JsonSerializer.Serialize(
                        instructorEnrollmentsPerCourse.Select(x => x.Count).ToArray()
                    );

                    // 2) Inscripciones por estado (En progreso / Completadas)
                    var instructorInProgress = await myEnrollmentsQuery
                        .CountAsync(e => e.Status == EnrollmentStatus.InProgress);
                    var instructorCompleted = await myEnrollmentsQuery
                        .CountAsync(e => e.Status == EnrollmentStatus.Completed);

                    ViewBag.InstructorEnrollmentsStatusSeries = JsonSerializer.Serialize(
                        new[] { instructorInProgress, instructorCompleted }
                    );
                    ViewBag.InstructorEnrollmentsStatusLabels = JsonSerializer.Serialize(
                        new[] { "En progreso", "Completadas" }
                    );

                    // 3) % de finalización por curso
                    var completionPerCourseLabels = new List<string>();
                    var completionPerCourseSeries = new List<int>();

                    foreach (var course in myCourses)
                    {
                        var totalCourseEnrollments = course.Enrollments?.Count ?? 0;
                        var completedCourseEnrollments = course.Enrollments?
                            .Count(e => e.Status == EnrollmentStatus.Completed) ?? 0;

                        int coursePercent = totalCourseEnrollments == 0
                            ? 0
                            : (completedCourseEnrollments * 100 / totalCourseEnrollments);

                        completionPerCourseLabels.Add(course.Title);
                        completionPerCourseSeries.Add(coursePercent);
                    }

                    ViewBag.InstructorCompletionPerCourseCategories =
                        JsonSerializer.Serialize(completionPerCourseLabels);
                    ViewBag.InstructorCompletionPerCourseSeries =
                        JsonSerializer.Serialize(completionPerCourseSeries);

                    // 4) Inscripciones por mes (últimos 6 meses) para este instructor
                    var monthNamesInst = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
                    var nowInst = DateTime.UtcNow;
                    var startMonthInst = new DateTime(nowInst.Year, nowInst.Month, 1).AddMonths(-5);

                    var rawMonthlyInstructor = await myEnrollmentsQuery
                        .Where(e => e.EnrolledAt >= startMonthInst)
                        .GroupBy(e => new { e.EnrolledAt.Year, e.EnrolledAt.Month })
                        .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                        .ToListAsync();

                    var instructorMonthsLabels = new List<string>();
                    var instructorMonthsSeries = new List<int>();

                    for (int i = 0; i < 6; i++)
                    {
                        var monthDate = startMonthInst.AddMonths(i);
                        var label = monthNamesInst[monthDate.Month - 1];
                        var match = rawMonthlyInstructor
                            .FirstOrDefault(x => x.Year == monthDate.Year && x.Month == monthDate.Month);

                        instructorMonthsLabels.Add(label);
                        instructorMonthsSeries.Add(match?.Count ?? 0);
                    }

                    ViewBag.InstructorEnrollmentsByMonthCategories =
                        JsonSerializer.Serialize(instructorMonthsLabels);
                    ViewBag.InstructorEnrollmentsByMonthSeries =
                        JsonSerializer.Serialize(instructorMonthsSeries);
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

                    // ============
                    // Charts Student
                    // ============

                    // 1) Estado de mis cursos (En progreso / Completados)
                    ViewBag.StudentCoursesStatusSeries = JsonSerializer.Serialize(
                        new[] { myInProgress, myCompleted }
                    );
                    ViewBag.StudentCoursesStatusLabels = JsonSerializer.Serialize(
                        new[] { "En progreso", "Completados" }
                    );

                    // 2) Progreso global de lecciones (radial)
                    ViewBag.StudentLessonsProgressSeries = JsonSerializer.Serialize(
                        new[] { lessonsPercent }
                    );
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
