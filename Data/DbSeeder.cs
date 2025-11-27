using Microsoft.EntityFrameworkCore;
using SIGECES.Models;
using SIGECES.Services;

namespace SIGECES.Data
{
    public static class DbSeeder
    {
        public static void Seed(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Aseguramos que la BD está al día con las migraciones
            context.Database.Migrate();

            // =========================
            // 1) USUARIOS BASE
            // =========================

            // Admin
            var admin = context.Users
                .FirstOrDefault(u => u.Email == "admin@sigeces.local");

            if (admin == null)
            {
                admin = new User
                {
                    FullName = "Admin SIGECES",
                    Email = "admin@sigeces.local",
                    PasswordHash = PasswordHasher.HashPassword("Admin123!"),
                    Role = UserRole.Admin,
                    IsActive = true
                };
                context.Users.Add(admin);
            }

            // Instructor demo
            var instructor = context.Users
                .FirstOrDefault(u => u.Email == "instructor@sigeces.local");

            if (instructor == null)
            {
                instructor = new User
                {
                    FullName = "Instructor Demo",
                    Email = "instructor@sigeces.local",
                    PasswordHash = PasswordHasher.HashPassword("Instructor123!"),
                    Role = UserRole.Instructor,
                    IsActive = true
                };
                context.Users.Add(instructor);
            }

            // Estudiante demo
            var student = context.Users
                .FirstOrDefault(u => u.Email == "estudiante@sigeces.local");

            if (student == null)
            {
                student = new User
                {
                    FullName = "Estudiante Demo",
                    Email = "estudiante@sigeces.local",
                    PasswordHash = PasswordHasher.HashPassword("Estudiante123!"),
                    Role = UserRole.Student,
                    IsActive = true
                };
                context.Users.Add(student);
            }

            // =========================
            // 2) CATEGORÍAS BASE
            // =========================

            var techCategory = context.CourseCategories
                .FirstOrDefault(c => c.Name == "Tecnología");

            if (techCategory == null)
            {
                techCategory = new CourseCategory
                {
                    Name = "Tecnología",
                    Description = "Cursos tecnológicos para estudiantes UTEC.",
                    IsActive = true
                };
                context.CourseCategories.Add(techCategory);
            }

            var softSkillsCategory = context.CourseCategories
                .FirstOrDefault(c => c.Name == "Habilidades Blandas");

            if (softSkillsCategory == null)
            {
                softSkillsCategory = new CourseCategory
                {
                    Name = "Habilidades Blandas",
                    Description = "Cursos de desarrollo personal y profesional.",
                    IsActive = true
                };
                context.CourseCategories.Add(softSkillsCategory);
            }

            // Guardamos hasta aquí por si necesitamos IDs
            context.SaveChanges();

            // =========================
            // 3) CURSO DEMO: Intro C#
            // =========================

            var introCSharp = context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefault(c => c.Title == "Introducción a C# para UTEC");

            if (introCSharp == null)
            {
                introCSharp = new Course
                {
                    Title = "Introducción a C# para UTEC",
                    Description = "Curso introductorio a C# y .NET para estudiantes de la UTEC. " +
                                  "Incluye sintaxis básica, tipos de datos y primeros programas.",
                    CategoryId = techCategory.Id,
                    InstructorId = instructor.Id,
                    IsActive = true
                };

                context.Courses.Add(introCSharp);
                context.SaveChanges();
            }

            // =========================
            // 4) LECCIONES DEMO
            // =========================

            if (introCSharp.Lessons == null || !introCSharp.Lessons.Any())
            {
                var lessons = new List<Lesson>
                {
                    new Lesson
                    {
                        CourseId = introCSharp.Id,
                        Title = "Bienvenida al curso",
                        Description = "Objetivos del curso, dinámica de trabajo y requisitos.",
                        Order = 1,
                        ResourceUrl = "https://learn.microsoft.com/dotnet/csharp/tour-of-csharp/"
                    },
                    new Lesson
                    {
                        CourseId = introCSharp.Id,
                        Title = "Sintaxis básica de C#",
                        Description = "Variables, tipos de datos, operadores y estructura de un programa.",
                        Order = 2,
                        ResourceUrl = "https://learn.microsoft.com/dotnet/csharp/fundamentals/types/"
                    },
                    new Lesson
                    {
                        CourseId = introCSharp.Id,
                        Title = "Colecciones y ciclos",
                        Description = "Listas, arreglos, foreach, for y while.",
                        Order = 3,
                        ResourceUrl = "https://learn.microsoft.com/dotnet/csharp/programming-guide/arrays/"
                    }
                };

                context.Lessons.AddRange(lessons);
            }

            // =========================
            // 5) INSCRIPCIÓN DEL ESTUDIANTE DEMO
            // =========================

            if (student != null)
            {
                var enrollmentExists = context.Enrollments
                    .Any(e => e.CourseId == introCSharp.Id && e.StudentId == student.Id);

                if (!enrollmentExists)
                {
                    var enrollment = new Enrollment
                    {
                        CourseId = introCSharp.Id,
                        StudentId = student.Id,
                        EnrolledAt = DateTime.UtcNow,
                        Status = EnrollmentStatus.InProgress
                    };

                    context.Enrollments.Add(enrollment);
                }
            }

            context.SaveChanges();
        }
    }
}
