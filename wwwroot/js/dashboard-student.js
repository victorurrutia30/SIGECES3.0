// wwwroot/js/dashboard-student.js

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

    // 1) Estado de mis cursos (donut)
    (function () {
        var selector = "#student-courses-status-chart";
        var el = document.querySelector(selector);
        if (!el) return;

        var series = el.dataset.series ? JSON.parse(el.dataset.series) : [3, 5];
        var labels = el.dataset.labels ? JSON.parse(el.dataset.labels) : ["En progreso", "Completados"];

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

    // 2) Progreso global de lecciones (radialBar)
    (function () {
        var selector = "#student-lessons-progress-chart";
        var el = document.querySelector(selector);
        if (!el) return;

        var series = el.dataset.series ? JSON.parse(el.dataset.series) : [65];

        createChart(selector, {
            chart: {
                type: "radialBar",
                height: 260
            },
            series: series,
            labels: ["Progreso global"],
            plotOptions: {
                radialBar: {
                    hollow: {
                        size: "60%"
                    },
                    dataLabels: {
                        name: {
                            fontSize: "14px"
                        },
                        value: {
                            fontSize: "24px",
                            formatter: function (val) {
                                return val + "%";
                            }
                        }
                    }
                }
            },
            stroke: {
                lineCap: "round"
            }
        });
    })();
});
