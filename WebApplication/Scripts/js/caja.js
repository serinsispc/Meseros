// ==========================================================
// caja.js
// ✅ PUENTE: dispara el botón oculto que llama el código C#
//    + Loading moderno (si existe)
//    + Antidoble click (bloqueo temporal)
//    + Render fix (requestAnimationFrame x2) para que el loading se vea
// ==========================================================

(function () {

    // Bloqueo para evitar doble disparo
    let _busy = false;

    // ==========================================================
    // ✅ EjecutarAccion: setea hidden fields y dispara el postback
    // Uso:
    //   EjecutarAccion("Actualizar", "")
    //   EjecutarAccion("Eliminar", "ID=15|CANT=3")
    //   EjecutarAccion("Actualizar", "", this)  // deshabilita el botón
    // ==========================================================
    window.EjecutarAccion = function (accion, argumento, btn) {

        // Evitar doble click / doble ejecución
        if (_busy) return;
        _busy = true;

        const hidAccion = document.getElementById("hidAccion");
        const hidArgumento = document.getElementById("hidArgumento");
        const btnBridge = document.getElementById("btnBridge");

        if (!hidAccion || !hidArgumento || !btnBridge) {
            console.warn("No se encontraron elementos puente:", {
                hidAccion, hidArgumento, btnBridge
            });
            _busy = false;
            return;
        }

        // Setear valores
        hidAccion.value = accion || "";
        hidArgumento.value = argumento || "";

        // Mostrar loading (si existe helper global)
        if (window.SerinsisLoading && typeof window.SerinsisLoading.show === "function") {
            window.SerinsisLoading.show();
        }

        // Deshabilitar botón (opcional) para evitar doble clic visual
        if (btn) {
            try {
                btn.disabled = true;
                btn.classList.add("disabled");
            } catch (e) { /* ignore */ }
        }

        // ✅ Dar tiempo a que el navegador "pinte" el overlay antes del postback
        requestAnimationFrame(() => {
            requestAnimationFrame(() => {
                btnBridge.click();
            });
        });

        // Plan B: si por alguna razón no navega / no hace postback,
        // liberamos el bloqueo a los ~7s para no "congelar" la UI.
        setTimeout(() => { _busy = false; }, 7000);
    };

    // ==========================================================
    // ✅ BuildArgs: construye argumentos tipo "ID=15|CANT=3"
    // Ejemplo:
    //   BuildArgs({ ID: 15, CANT: 3 })
    // ==========================================================
    window.BuildArgs = function (obj) {
        return Object.keys(obj || {})
            .map(k => k + "=" + encodeURIComponent(obj[k] ?? ""))
            .join("|");
    };

    // ==========================================================
    // ✅ Acción ejemplo: Actualizar
    // ==========================================================
    window.Actualizar = function () {
        EjecutarAccion("Actualizar", "");
    };

})();


function abrirModalCuenta(el, e) {
    // ✅ evitar que se dispare el onclick del contenedor
    if (e) {
        e.preventDefault();
        e.stopPropagation();
        e.stopImmediatePropagation();
    }

    // por si quedó loading visible
    if (window.SerinsisLoading) SerinsisLoading.hide();

    const cuentaEditandoId = el.getAttribute("data-id") || "";
    const nombre = el.getAttribute("data-nombre") || "";

    const titulo = document.getElementById("modalCuentaTitulo");
    const idEditar = document.getElementById("idCuentaModalEditar");
    const input = document.getElementById("txtCuentaNombre");
    const err = document.getElementById("cuentaError");
    const modalEl = document.getElementById("modalCuentaCliente");

    if (!modalEl) {
        console.warn("No existe el modal #modalCuentaCliente");
        return false;
    }

    if (titulo) titulo.innerText = cuentaEditandoId ? "Editar Cuenta" : "Nueva Cuenta";
    if (idEditar) idEditar.value = cuentaEditandoId;
    if (input) input.value = nombre;
    if (err) err.classList.add("d-none");

    // ✅ Bootstrap modal
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl, {
        backdrop: "static",
        keyboard: false
    });

    // 🔥 CLAVE: foco SOLO cuando el modal YA está visible
    modalEl.addEventListener(
        "shown.bs.modal",
        function onShown() {
            if (input) {
                input.focus();
                input.select();
            }
            // evitar múltiples listeners
            modalEl.removeEventListener("shown.bs.modal", onShown);
        },
        { once: true }
    );

    modal.show();
    return false;
}


function guardarCuentaDirecto(btn) {

    const nombre = document.getElementById("txtCuentaNombre").value.trim();
    const idCuenta = document.getElementById("idCuentaModalEditar").value;

    const error = document.getElementById("cuentaError");

    if (nombre.length < 2) {
        error.classList.remove("d-none");
        return;
    }

    error.classList.add("d-none");

    // cerrar modal
    const modalEl = document.getElementById("modalCuentaCliente");
    const modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) modal.hide();

    // llamar directamente tu puente
    EjecutarAccion(
        "EditarAliasCuenta",
        BuildArgs({
            ID: idCuenta,
            ALIAS: nombre
        }),
        btn
    );
}


(function () {

    document.addEventListener("DOMContentLoaded", function () {
        const input = document.getElementById("txtCuentaNombre");
        if (!input) return;

        input.addEventListener("keydown", function (e) {
            if (e.key === "Enter") {
                e.preventDefault();
                e.stopPropagation();

                const btn = document.getElementById("btnGuardarCuenta");
                if (btn) btn.click();
            }
        });
    });

})();






    document.addEventListener("DOMContentLoaded", function () {

    const contenedor = document.getElementById("<%= rpProductos.ClientID %>") || document;

    function normalizarCantidad(input) {
        let v = parseInt(input.value, 10);
    if (isNaN(v) || v < 1) v = 1;
    input.value = v;
    return v;
    }

    // ✅ Clicks (delegación)
    contenedor.addEventListener("click", function (e) {
        const btnRestar = e.target.closest(".js-restar");
    const btnSumar  = e.target.closest(".js-sumar");

    if (!btnRestar && !btnSumar) return;

    const item = e.target.closest(".producto-item");
    if (!item) return;

    const input = item.querySelector(".js-cantidad");
    if (!input) return;

    let cant = normalizarCantidad(input);

    if (btnRestar) {
        cant = Math.max(1, cant - 1);
        } else if (btnSumar) {
        cant = cant + 1;
        }

    input.value = cant;
    });

    // ✅ Escribir manualmente (mínimo 1)
    contenedor.addEventListener("input", function (e) {
        const input = e.target.closest(".js-cantidad");
    if (!input) return;

    // deja solo números (opcional, pero recomendado)
    input.value = input.value.replace(/[^\d]/g, "");
    normalizarCantidad(input);
    });

    // ✅ Si salen del input vacío, vuelve a 1
    contenedor.addEventListener("blur", function (e) {
        const input = e.target.closest(".js-cantidad");
    if (!input) return;

    normalizarCantidad(input);
    }, true);

});
