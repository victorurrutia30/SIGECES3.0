// wwwroot/js/dashboard-admin.js

document.addEventListener("DOMContentLoaded", function () {
    // Contenedor de la gráfica solo existe en el dashboard del Admin
    var el = document.querySelector("#admin-enrollments-chart");
    if (!el) {
        return; // Si no existe, no hacemos nada (para Instructor/Student)
    }

    if (typeof ApexCharts === "undefined") {
        console.error("ApexCharts no está cargado.");
        return;
    }

    // Datos DEMO por ahora; luego podemos inyectar datos reales del backend
    var options = {
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
                data: [10, 25, 15, 30]
            }
        ],
        xaxis: {
            categories: ["Curso A", "Curso B", "Curso C", "Curso D"],
            labels: {
                rotate: -45
            }
        },
        plotOptions: {
            bar: {
                columnWidth: "50%",
                borderRadius: 4
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
    };

    var chart = new ApexCharts(el, options);
    chart.render();
});
