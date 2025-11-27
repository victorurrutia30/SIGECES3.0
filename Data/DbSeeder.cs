using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

            // Aseguramos que la BD está migrada
            context.Database.Migrate();

            if (!context.Users.Any())
            {
                // Usuarios demo
                var admin = new User
                {
                    FullName = "Admin SIGECES",
                    Email = "admin@sigeces.local",
                    PasswordHash = PasswordHasher.HashPassword("Admin123!"),
                    Role = UserRole.Admin,
                    IsActive = true
                };

                var instructor = new User
                {
                    FullName = "Instructor Demo",
                    Email = "instructor@sigeces.local",
                    PasswordHash = PasswordHasher.HashPassword("Instructor123!"),
                    Role = UserRole.Instructor,
                    IsActive = true
                };

                var student = new User
                {
                    FullName = "Estudiante Demo",
                    Email = "estudiante@sigeces.local",
                    PasswordHash = PasswordHasher.HashPassword("Estudiante123!"),
                    Role = UserRole.Student,
                    IsActive = true
                };

                context.Users.AddRange(admin, instructor, student);

                // Categorías demo
                var cat1 = new CourseCategory
                {
                    Name = "Tecnología",
                    Description = "Cursos tecnológicos para estudiantes UTEC.",
                    IsActive = true
                };

                var cat2 = new CourseCategory
                {
                    Name = "Habilidades Blandas",
                    Description = "Cursos de desarrollo personal y profesional.",
                    IsActive = true
                };

                context.CourseCategories.AddRange(cat1, cat2);

                context.SaveChanges();
            }
        }
    }
}
