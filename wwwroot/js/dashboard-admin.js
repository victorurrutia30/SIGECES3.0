// wwwroot/js/dashboard-admin.js

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

    // 1) Usuarios por rol (donut)
    (function () {
        var selector = "#admin-users-by-role-chart";
        var el = document.querySelector(selector);
        if (!el) return;

        var series = el.dataset.series ? JSON.parse(el.dataset.series) : [3, 8, 45];
        var labels = el.dataset.labels ? JSON.parse(el.dataset.labels) : ["Admins", "Instructores", "Estudiantes"];

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

    // 2) Inscripciones por estado (donut)
    (function () {
        var selector = "#admin-enrollments-status-chart";
        var el = document.querySelector(selector);
        if (!el) return;

        var series = el.dataset.series ? JSON.parse(el.dataset.series) : [60, 40];
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

    // 3) Inscripciones por curso (barra)
    (function () {
        var selector = "#admin-enrollments-per-course-chart";
        var el = document.querySelector(selector);
        if (!el) return;

        var data = el.dataset.series ? JSON.parse(el.dataset.series) : [25, 40, 18, 32];
        var categories = el.dataset.categories
            ? JSON.parse(el.dataset.categories)
            : ["Curso A", "Curso B", "Curso C", "Curso D"];

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

    // 4) Inscripciones por mes (línea)
    (function () {
        var selector = "#admin-enrollments-by-month-chart";
        var el = document.querySelector(selector);
        if (!el) return;

        var data = el.dataset.series ? JSON.parse(el.dataset.series) : [15, 22, 18, 30, 27, 35];
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
