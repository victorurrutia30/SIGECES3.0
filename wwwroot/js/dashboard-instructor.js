// wwwroot/js/dashboard-instructor.js

document.addEventListener("DOMContentLoaded", function () {
    if (typeof ApexCharts === "undefined") {
        console.error("ApexCharts no está cargado.");
        return;
    }

    function createChart(selector, options) {
        var el = document.querySelector(selector);
        if (!el) {
            return;
        }

        var chart = new ApexCharts(el, options);
        chart.render();
    }

    // 1) Inscripciones por curso (barra)
    (function () {
        var selector = "#instructor-enrollments-per-course-chart";
        var el = document.querySelector(selector);
        if (!el) return;

        var data = el.dataset.series ? JSON.parse(el.dataset.series) : [18, 25, 12, 30];
        var categories = el.dataset.categories
            ? JSON.parse(el.dataset.categories)
            : ["Curso 1", "Curso 2", "Curso 3", "Curso 4"];

        createChart(selector, {
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
                    data: data
                }
            ],
            xaxis: {
                categories: categories,
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
    })();

    // 2) Inscripciones por estado (donut)
    (function () {
        var selector = "#instructor-enrollments-status-chart";
        var el = document.querySelector(selector);
        if (!el) return;

        var series = el.dataset.series ? JSON.parse(el.dataset.series) : [35, 20];
        var labels = el.dataset.labels ? JSON.parse(el.dataset.labels) : ["En progreso", "Completadas"];

        createChart(selector, {
            chart: {
                type: "donut",
                height: 260
            },
            series: series,
            labels: labels,
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
    })();

    // 3) % de finalización por curso (barra horizontal)
    (function () {
        var selector = "#instructor-completion-per-course-chart";
        var el = document.querySelector(selector);
        if (!el) return;

        var data = el.dataset.series ? JSON.parse(el.dataset.series) : [40, 75, 55, 90];
        var categories = el.dataset.categories
            ? JSON.parse(el.dataset.categories)
            : ["Curso 1", "Curso 2", "Curso 3", "Curso 4"];

        createChart(selector, {
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
                    data: data
                }
            ],
            xaxis: {
                categories: categories
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
    })();

    // 4) Inscripciones por mes (línea)
    (function () {
        var selector = "#instructor-enrollments-by-month-chart";
        var el = document.querySelector(selector);
        if (!el) return;

        var data = el.dataset.series ? JSON.parse(el.dataset.series) : [5, 9, 7, 12, 10, 14];
        var categories = el.dataset.categories
            ? JSON.parse(el.dataset.categories)
            : ["Ene", "Feb", "Mar", "Abr", "May", "Jun"];

        createChart(selector, {
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
                    data: data
                }
            ],
            xaxis: {
                categories: categories
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
    })();
});
