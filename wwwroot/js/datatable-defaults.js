// wwwroot/js/datatable-defaults.js
// Configuración global y helpers para DataTables en SIGECES

(function (window, $) {
    'use strict';

    if (!$) {
        console.warn('SIGECES DataTables: jQuery no está disponible.');
        return;
    }

    function hasDataTables() {
        return $.fn && $.fn.dataTable;
    }

    if (!hasDataTables()) {
        console.warn('SIGECES DataTables: DataTables no está cargado. Revisa los scripts en _Layout.cshtml.');
        return;
    }

    // ============================
    // Defaults globales
    // ============================
    $.extend(true, $.fn.dataTable.defaults, {
        // Idioma español
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
        },

        // Paginación estándar
        pageLength: 10,
        lengthMenu: [
            [10, 25, 50, -1],
            [10, 25, 50, 'Todos']
        ],

        // Sin ordenar por default (dejamos que cada vista lo decida)
        order: [],

        // Evitar que calcule anchos raros
        autoWidth: false,

        // Layout de controles (filtros arriba, info + paginación abajo)
        dom:
            "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>"
    });

    // ============================
    // Auto-init básico
    // ============================
    // Cualquier tabla con data-datatable="true"
    // se inicializa sola con los defaults globales.
    $(function () {
        $('table[data-datatable="true"]').each(function () {
            var $table = $(this);

            // Evitar doble inicialización
            if ($.fn.dataTable.isDataTable($table)) {
                return;
            }

            $table.DataTable();
        });
    });

    // ============================
    // Helpers globales SIGECES
    // ============================
    window.Sigeces = window.Sigeces || {};

    // Inicialización explícita desde una vista
    // Ejemplo: var dt = Sigeces.initDataTable('#tabla-cursos', { order: [[0, 'asc']] });
    window.Sigeces.initDataTable = function (selector, options) {
        if (!hasDataTables()) {
            console.warn('SIGECES DataTables: DataTables no está cargado.');
            return null;
        }

        var $table = $(selector);
        if (!$table.length) {
            console.warn('SIGECES DataTables: no se encontró la tabla: ' + selector);
            return null;
        }

        // Evitar múltiple inicialización
        if ($.fn.dataTable.isDataTable($table)) {
            return $table.DataTable();
        }

        return $table.DataTable(options || {});
    };

    // Vincular filtros externos (inputs/select) a una columna
    // Ejemplo:
    //   var dt = Sigeces.initDataTable('#tabla', {});
    //   Sigeces.bindColumnFilter(dt, '#filtroCategoria', 1, false);
    window.Sigeces.bindColumnFilter = function (dataTable, filterSelector, columnIndex, useRegex) {
        if (!dataTable) return;

        var $filter = $(filterSelector);
        if (!$filter.length) return;

        var events = $filter.is('input') ? 'keyup change' : 'change';

        $filter.on(events, function () {
            var value = $(this).val() || '';

            if (useRegex) {
                dataTable
                    .column(columnIndex)
                    .search(value, true, false)
                    .draw();
            } else {
                dataTable
                    .column(columnIndex)
                    .search(value, false, false)
                    .draw();
            }
        });
    };

})(window, window.jQuery);
