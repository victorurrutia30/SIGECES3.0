// wwwroot/js/dashboard-student.js

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
     * 1) Estado de mis cursos (donut)
     *    Demo: cursos en progreso vs completados
     */
    createChart("#student-courses-status-chart", {
        chart: {
            type: "donut",
            height: 260
        },
        series: [3, 5], // DEMO: 3 en progreso, 5 completados
        labels: ["En progreso", "Completados"],
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
     * 2) Progreso global de lecciones (radialBar)
     *    Demo: porcentaje global de lecciones completadas
     */
    createChart("#student-lessons-progress-chart", {
        chart: {
            type: "radialBar",
            height: 260
        },
        series: [65], // DEMO: 65% de progreso global
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
});
