// wwwroot/js/dashboard-instructor.js

document.addEventListener("DOMContentLoaded", function () {
    if (typeof ApexCharts === "undefined") {
        console.error("ApexCharts no está cargado.");
        return;
    }

    function createChart(selector, options) {
        var el = document.querySelector(selector);
        if (!el) {
            return; // Si el contenedor no existe (otro rol), no hacemos nada
        }

        var chart = new ApexCharts(el, options);
        chart.render();
    }

    /**
     * 1) Inscripciones por curso (barra)
     *    Demo: cursos del instructor con cantidad de inscripciones
     */
    createChart("#instructor-enrollments-per-course-chart", {
        chart: {
            type: "bar",
            height: 260,
            toolbar: {
                show: false
            }
        },
        series: [
            {
                name: "Inscripciones",
                data: [18, 25, 12, 30] // DEMO
            }
        ],
        xaxis: {
            categories: ["Curso 1", "Curso 2", "Curso 3", "Curso 4"],
            labels: {
                rotate: -30
            }
        },
        plotOptions: {
            bar: {
                columnWidth: "50%",
                borderRadius: 6
            }
        },
        dataLabels: {
            enabled: false
        },
        grid: {
            strokeDashArray: 3
        },
        tooltip: {
            y: {
                formatter: function (val) {
                    return val + " inscripciones";
                }
            }
        }
    });

    /**
     * 2) Inscripciones por estado (donut)
     *    Demo: En progreso vs Completadas en los cursos del instructor
     */
    createChart("#instructor-enrollments-status-chart", {
        chart: {
            type: "donut",
            height: 260
        },
        series: [35, 20], // DEMO: 35 en progreso, 20 completadas
        labels: ["En progreso", "Completadas"],
        dataLabels: {
            enabled: true
        },
        legend: {
            position: "bottom"
        },
        stroke: {
            width: 1
        }
    });

    /**
     * 3) % de finalización por curso (barra horizontal)
     *    Demo: 4 cursos con distintos porcentajes de finalización
     */
    createChart("#instructor-completion-per-course-chart", {
        chart: {
            type: "bar",
            height: 260,
            toolbar: {
                show: false
            }
        },
        series: [
            {
                name: "Finalización (%)",
                data: [40, 75, 55, 90] // DEMO
            }
        ],
        xaxis: {
            categories: ["Curso 1", "Curso 2", "Curso 3", "Curso 4"]
        },
        plotOptions: {
            bar: {
                horizontal: true,
                barHeight: "60%",
                borderRadius: 4
            }
        },
        dataLabels: {
            enabled: true,
            formatter: function (val) {
                return val + "%";
            }
        },
        grid: {
            strokeDashArray: 3
        },
        tooltip: {
            x: {
                show: true
            },
            y: {
                formatter: function (val) {
                    return val + "% completado";
                }
            }
        }
    });

    /**
     * 4) Inscripciones por mes (línea) para el instructor
     *    Demo: últimos 6 meses
     */
    createChart("#instructor-enrollments-by-month-chart", {
        chart: {
            type: "line",
            height: 260,
            toolbar: {
                show: false
            }
        },
        series: [
            {
                name: "Inscripciones",
                data: [5, 9, 7, 12, 10, 14] // DEMO
            }
        ],
        xaxis: {
            categories: ["Ene", "Feb", "Mar", "Abr", "May", "Jun"]
        },
        stroke: {
            curve: "smooth",
            width: 3
        },
        markers: {
            size: 4
        },
        dataLabels: {
            enabled: false
        },
        grid: {
            strokeDashArray: 3
        },
        tooltip: {
            y: {
                formatter: function (val) {
                    return val + " inscripciones";
                }
            }
        }
    });
});
