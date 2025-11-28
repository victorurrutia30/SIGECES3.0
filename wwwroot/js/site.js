// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function () {
    const forms = document.querySelectorAll('form[data-confirm]');

    forms.forEach(function (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault(); // detenemos el submit normal

            const message = form.getAttribute('data-confirm') || '¿Estás seguro que deseas continuar?';

            Swal.fire({
                title: 'Confirmar acción',
                text: message,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sí, continuar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    form.submit(); // ahora sí, enviamos el formulario
                }
            });
        });
    });
});
