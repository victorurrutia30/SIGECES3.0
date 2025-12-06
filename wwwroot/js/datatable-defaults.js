@model IEnumerable(SIGECES.Models.Course)

@{
    ViewData["Title"] = "Catálogo de cursos";
    var enrolledIds = ViewBag.EnrolledCourseIds as List<int> ?? new List<int>();
    string search = ViewBag.Search as string ?? string.Empty;
    int currentPage = ViewBag.Page is int ? (int)ViewBag.Page : 1;
    int totalPages = ViewBag.TotalPages is int ? (int)ViewBag.TotalPages : 1;
}

<div class="page-header d-print-none mb-3">
    <div class="row align-items-center">
        <div class="col">
            <div class="page-pretitle">
                Estudiante
            </div>
            <h2 class="page-title">
                Catálogo de cursos
            </h2>
            <div class="text-muted mt-1">
                Explora los cursos extracurriculares disponibles y apúntate a los que te interesen.
            </div>
        </div>
        <div class="col-auto ms-auto d-print-none">
            <a asp-controller="StudentCourses" asp-action="MyCourses" class="btn btn-outline-primary">
                <i class="ti ti-bookmarks me-1"></i>
                Mis cursos
            </a>
        </div>
    </div>
</div>

<div class="card">
    <div class="card-header">
        <form method="get" class="row g-2 w-100">
            <div class="col-md-6">
                <div class="input-icon">
                    <span class="input-icon-addon">
                        <i class="ti ti-search"></i>
                    </span>
                    <input type="text"
                           id="catalog-search-input"   @* <- ID para el JS *@
                           name="q"
                           value="@search"
                           class="form-control"
                           placeholder="Buscar por título, categoría o instructor..." />
                </div>
            </div>
            <div class="col-md-3">
                <button type="submit" class="btn btn-outline-secondary w-100">
                    <i class="ti ti-filter me-1"></i>
                    Aplicar filtros
                </button>
            </div>
        </form>
    </div>

    <div class="card-body">
        @if (!Model.Any())
        {
            <div class="text-center text-muted py-5">
                <i class="ti ti-mood-empty mb-2" style="font-size: 2rem;"></i>
                <div>No se encontraron cursos con los filtros actuales.</div>
            </div>
        }
        else
        {
            <div class="row row-cards">
                @foreach (var course in Model)
                {
                    var isEnrolled = enrolledIds.Contains(course.Id);

                    <div class="col-md-6 col-lg-4">
                        <div class="card h-100 shadow-sm border-0">
                            <div class="card-body d-flex flex-column">
                                <div class="d-flex justify-content-between align-items-start mb-2">
                                    <div>
                                        <div class="text-muted text-uppercase fs-6">
                                            @course.Category?.Name
                                        </div>
                                        <h3 class="card-title mb-0">
                                            @course.Title
                                        </h3>
                                    </div>
                                    <span class="badge @(isEnrolled ? "bg-success-lt" : "bg-secondary-lt")">
                                        <i class="ti @(isEnrolled ? "ti-check" : "ti-plus") me-1"></i>
                                        @(isEnrolled ? "Inscrito" : "Disponible")
                                    </span>
                                </div>

                                @if (!string.IsNullOrWhiteSpace(course.Description))
                                {
                                    <p class="text-muted mb-3 flex-grow-1">
                                        @course.Description
                                    </p>
                                }
                                else
                                {
                                    <p class="text-muted mb-3 flex-grow-1">
                                        Curso sin descripción detallada.
                                    </p>
                                }

                                <div class="d-flex justify-content-between align-items-center mt-auto">
                                    <div class="small text-muted">
                                        <i class="ti ti-user-circle me-1"></i>
                                        @course.Instructor?.FullName
                                    </div>
                                    <div class="btn-group">
                                        <a asp-action="Details"
                                           asp-route-id="@course.Id"
                                           class="btn btn-outline-primary btn-sm">
                                            <i class="ti ti-eye me-1"></i>
                                            Ver detalles
                                        </a>

                                        @if (!isEnrolled)
                                        {
                                            <form asp-action="Enroll" method="post" class="d-inline">
                                                @Html.AntiForgeryToken()
                                                <input type="hidden" name="courseId" value="@course.Id" />
                                                <button type="submit" class="btn btn-primary btn-sm">
                                                    <i class="ti ti-login me-1"></i>
                                                    Inscribirme
                                                </button>
                                            </form>
                                        }
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                }
            </div>

            @* Paginación *@
            @if (totalPages > 1)
            {
                <div class="mt-4 d-flex justify-content-between align-items-center">
                    <div class="text-muted small">
                        Página @currentPage de @totalPages
                    </div>

                    <nav>
                        <ul class="pagination mb-0">
                            <li class="page-item @(currentPage <= 1 ? "disabled" : "")">
                                <a class="page-link"
                                   asp-action="Index"
                                   asp-route-q="@search"
                                   asp-route-page="@(currentPage - 1)">
                                    Anterior
                                </a>
                            </li>
                            <li class="page-item @(currentPage >= totalPages ? "disabled" : "")">
                                <a class="page-link"
                                   asp-action="Index"
                                   asp-route-q="@search"
                                   asp-route-page="@(currentPage + 1)">
                                    Siguiente
                                </a>
                            </li>
                        </ul>
                    </nav>
                </div>
            }
        }
    </div>
</div>

@section Scripts {
    <script>
        $(function () {
            var $input = $('#catalog-search-input');
            if ($input.length === 0) return;

            var timeoutId = null;

            // Auto-submit con debounce mientras escribe
            $input.on('input', function () {
                var $form = $(this).closest('form');

                if (timeoutId) {
                    clearTimeout(timeoutId);
                }

                timeoutId = setTimeout(function () {
                    $form.trigger('submit');
                }, 400); // milisegundos de espera tras dejar de teclear
            });
        });
    </script>
}
