using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models;

public enum UserRole
{
    [Display(Name = "Administrador")]
    Admin = 1,

    [Display(Name = "Instructor")]
    Instructor = 2,

    [Display(Name = "Estudiante")]
    Student = 3
}
