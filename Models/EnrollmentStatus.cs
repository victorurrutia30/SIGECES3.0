using System.ComponentModel.DataAnnotations;

namespace SIGECES.Models;

public enum EnrollmentStatus
{
    [Display(Name = "En progreso")]
    InProgress = 1,

    [Display(Name = "Completado")]
    Completed = 2
}
