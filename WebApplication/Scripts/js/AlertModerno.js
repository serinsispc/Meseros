/* ===========================================================
   ALERTMODERNO.JS
   Librería global de alertas modernas (SweetAlert2)
   Compatible con ASP.NET WebForms / Bootstrap 5.2.3
   -----------------------------------------------------------
   Funciones:
     - AlertModerno.Success(titulo, mensaje, ms)
     - AlertModerno.Error(titulo, mensaje, ms)
     - AlertModerno.Warning(titulo, mensaje, ms)
     - AlertModerno.Info(titulo, mensaje, ms)
     - AlertModerno.Confirm(titulo, mensaje, callback)
   =========================================================== */

// Asegura que SweetAlert2 esté cargado
if (typeof Swal === 'undefined') {
    console.error('SweetAlert2 no se ha cargado. Asegúrate de incluir el CDN antes de este script.');
}

// Namespace global
window.AlertModerno = {

    // ========== ALERTAS BÁSICAS ==========
    Success: function (titulo, mensaje, ms = 3000) {
        this._toast('success', titulo, mensaje, ms);
    },

    Error: function (titulo, mensaje, ms = 3500) {
        this._toast('error', titulo, mensaje, ms);
    },

    Warning: function (titulo, mensaje, ms = 4000) {
        this._toast('warning', titulo, mensaje, ms);
    },

    Info: function (titulo, mensaje, ms = 3000) {
        this._toast('info', titulo, mensaje, ms);
    },

    // ========== CONFIRMACIÓN ==========
    Confirm: function (titulo, mensaje, callback) {
        Swal.fire({
            title: titulo || '¿Estás seguro?',
            text: mensaje || '',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sí, continuar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            backdrop: true,
            allowOutsideClick: false,
            allowEscapeKey: true
        }).then((result) => {
            if (result.isConfirmed && typeof callback === 'function') {
                callback(true);
            } else if (typeof callback === 'function') {
                callback(false);
            }
        });
    },

    // ========== MÉTODO PRIVADO ==========
    _toast: function (tipo, titulo, mensaje, ms, posicion = 'center', esToast = false) {
        Swal.fire({
            icon: tipo,
            title: titulo || '',
            text: mensaje || '',
            timer: ms,
            timerProgressBar: true,
            toast: esToast,
            position: posicion,
            showConfirmButton: false,
            background: '#fff',
            color: '#000',
            customClass: {
                popup: 'shadow border-0 rounded-4 p-3'
            },
        });
    }

};
