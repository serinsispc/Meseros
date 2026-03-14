(function () {
    let busy = false;

    window.EjecutarAccion = function (accion, argumento, btn) {
        if (busy) return;
        busy = true;

        const hidAccion = document.getElementById("hidAccion");
        const hidArgumento = document.getElementById("hidArgumento");
        const btnBridge = document.getElementById("btnBridge");

        if (!hidAccion || !hidArgumento || !btnBridge) {
            busy = false;
            return;
        }

        hidAccion.value = accion || "";
        hidArgumento.value = argumento || "";

        if (window.SerinsisLoading && typeof window.SerinsisLoading.show === "function") {
            window.SerinsisLoading.show();
        }

        if (btn) {
            try {
                btn.disabled = true;
                btn.classList.add("disabled");
            } catch (e) {
            }
        }

        requestAnimationFrame(function () {
            requestAnimationFrame(function () {
                btnBridge.click();
            });
        });

        setTimeout(function () {
            busy = false;
        }, 7000);
    };

    window.BuildArgs = function (obj) {
        return Object.keys(obj || {})
            .map(function (k) { return k + "=" + encodeURIComponent(obj[k] ?? ""); })
            .join("|");
    };

    window.Actualizar = function () {
        EjecutarAccion("Actualizar", "");
    };

    window.ConfirmarLiberarMesa = function (btn) {
        if (!window.Swal || typeof window.Swal.fire !== "function") {
            const ok = window.confirm("\u00bfSeguro que desea liberar esta mesa?");
            if (!ok) {
                return false;
            }

            EjecutarAccion("LiberarMesa", "", btn);
            return true;
        }

        Swal.fire({
            icon: "warning",
            title: "Liberar mesa",
            text: "\u00bfSeguro que desea liberar esta mesa?",
            showCancelButton: true,
            confirmButtonText: "S\u00ed, liberar",
            cancelButtonText: "Cancelar",
            confirmButtonColor: "#dc3545",
            cancelButtonColor: "#94a3b8",
            reverseButtons: true,
            focusCancel: true,
            customClass: {
                popup: "shadow-lg rounded-4"
            }
        }).then(function (result) {
            if (result.isConfirmed) {
                EjecutarAccion("LiberarMesa", "", btn);
            }
        });

        return false;
    };

    window.ConfirmarEliminarServicio = function (btn) {
        if (!window.Swal || typeof window.Swal.fire !== "function") {
            const ok = window.confirm("\u00bfSeguro que desea eliminar el servicio activo?");
            if (!ok) {
                return false;
            }

            EjecutarAccion("EliminarServicio", "", btn);
            return true;
        }

        Swal.fire({
            icon: "warning",
            title: "Eliminar servicio",
            text: "\u00bfSeguro que desea eliminar el servicio activo?",
            showCancelButton: true,
            confirmButtonText: "S\u00ed, eliminar",
            cancelButtonText: "Cancelar",
            confirmButtonColor: "#dc3545",
            cancelButtonColor: "#94a3b8",
            reverseButtons: true,
            focusCancel: true,
            customClass: {
                popup: "shadow-lg rounded-4"
            }
        }).then(function (result) {
            if (result.isConfirmed) {
                EjecutarAccion("EliminarServicio", "", btn);
            }
        });

        return false;
    };
})();

function abrirModalCuenta(el, e) {
    if (e) {
        e.preventDefault();
        e.stopPropagation();
        e.stopImmediatePropagation();
    }

    if (window.SerinsisLoading) {
        SerinsisLoading.hide();
    }

    const cuentaEditandoId = el.getAttribute("data-id") || "";
    const nombre = el.getAttribute("data-nombre") || "";

    const titulo = document.getElementById("modalCuentaTitulo");
    const idEditar = document.getElementById("idCuentaModalEditar");
    const input = document.getElementById("txtCuentaNombre");
    const err = document.getElementById("cuentaError");
    const modalEl = document.getElementById("modalCuentaCliente");

    if (!modalEl) {
        return false;
    }

    if (titulo) titulo.innerText = cuentaEditandoId ? "Editar Cuenta" : "Nueva Cuenta";
    if (idEditar) idEditar.value = cuentaEditandoId;
    if (input) input.value = nombre;
    if (err) err.classList.add("d-none");

    const modal = bootstrap.Modal.getOrCreateInstance(modalEl, {
        backdrop: "static",
        keyboard: false
    });

    modalEl.addEventListener("shown.bs.modal", function onShown() {
        if (input) {
            input.focus();
            input.select();
        }
        modalEl.removeEventListener("shown.bs.modal", onShown);
    }, { once: true });

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

    const modalEl = document.getElementById("modalCuentaCliente");
    const modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) modal.hide();

    EjecutarAccion("EditarAliasCuenta", BuildArgs({
        ID: idCuenta,
        ALIAS: nombre
    }), btn);
}

(function () {
    document.addEventListener("DOMContentLoaded", function () {
        const inputCuenta = document.getElementById("txtCuentaNombre");
        if (inputCuenta) {
            inputCuenta.addEventListener("keydown", function (e) {
                if (e.key === "Enter") {
                    e.preventDefault();
                    e.stopPropagation();

                    const btn = document.getElementById("btnGuardarCuenta");
                    if (btn) btn.click();
                }
            });
        }

        configurarBuscadorCaja();
        configurarCantidadProductos();
    });
})();

function configurarBuscadorCaja() {
    const input = document.getElementById("btnbuscar");
    const categoriaBtns = Array.from(document.querySelectorAll(".categoria-btn[data-categoria-id]"));
    const productoItems = Array.from(document.querySelectorAll(".producto-item[data-categoria-id]"));

    if (!input || !categoriaBtns.length || !productoItems.length) {
        return;
    }

    const getTexto = function (value) {
        return (value || "").toString().trim().toLowerCase();
    };

    const getCategoriaActiva = function () {
        const activa = categoriaBtns.find(function (btn) {
            return btn.classList.contains("categoria-activa");
        });
        return activa ? activa.dataset.categoriaId : "";
    };

    const aplicarFiltro = function (texto) {
        const termino = getTexto(texto);
        const categoriaActiva = getCategoriaActiva();
        const categoriasConCoincidencia = new Set();

        productoItems.forEach(function (item) {
            const nombre = getTexto(item.dataset.productoNombre);
            const descripcion = getTexto(item.dataset.productoDescripcion);
            const codigo = getTexto(item.dataset.productoCodigo);
            const categoriaNombre = getTexto(item.dataset.categoriaNombre);
            const categoriaId = item.dataset.categoriaId || "";

            const coincideBusqueda = !termino ||
                nombre.includes(termino) ||
                descripcion.includes(termino) ||
                codigo.includes(termino) ||
                categoriaNombre.includes(termino);

            const coincideCategoria = termino ? true : (!categoriaActiva || categoriaActiva === categoriaId);
            const visible = coincideBusqueda && coincideCategoria;

            item.classList.toggle("producto-hidden", !visible);
            if (visible && categoriaId) {
                categoriasConCoincidencia.add(categoriaId);
            }
        });

        categoriaBtns.forEach(function (btn) {
            const categoriaId = btn.dataset.categoriaId || "";
            const categoriaNombre = getTexto(btn.dataset.categoriaNombre);
            const mostrarCategoria = !termino || categoriasConCoincidencia.has(categoriaId) || categoriaNombre.includes(termino);
            const resaltar = termino ? (categoriasConCoincidencia.has(categoriaId) || categoriaNombre.includes(termino)) : btn.classList.contains("categoria-activa");

            btn.classList.toggle("categoria-hidden", !mostrarCategoria);
            btn.classList.toggle("categoria-search-hit", !!resaltar && (!btn.classList.contains("categoria-activa") || !!termino));
        });
    };

    input.addEventListener("input", function () {
        aplicarFiltro(input.value);
    });

    input.addEventListener("keydown", function (e) {
        if (e.key !== "Enter") {
            return;
        }

        e.preventDefault();
        e.stopPropagation();

        const texto = (input.value || "").trim();
        if (!texto) {
            return;
        }

        EjecutarAccion("BuscarCodigoProducto", texto, input);
    });

    window.CajaBuscador = {
        apply: aplicarFiltro,
        clear: function (focus) {
            input.value = "";
            aplicarFiltro("");
            if (focus !== false) {
                input.focus();
            }
        }
    };

    aplicarFiltro(input.value || "");
}

function configurarCantidadProductos() {
    const contenedor = document.querySelector(".productos-list") || document;

    function normalizarCantidad(input) {
        let v = parseInt(input.value, 10);
        if (isNaN(v) || v < 1) v = 1;
        input.value = v;
        return v;
    }

    contenedor.addEventListener("click", function (e) {
        const btnRestar = e.target.closest(".js-restar");
        const btnSumar = e.target.closest(".js-sumar");

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

    contenedor.addEventListener("input", function (e) {
        const input = e.target.closest(".js-cantidad");
        if (!input) return;

        input.value = input.value.replace(/[^\d]/g, "");
        normalizarCantidad(input);
    });

    contenedor.addEventListener("blur", function (e) {
        const input = e.target.closest(".js-cantidad");
        if (!input) return;

        normalizarCantidad(input);
    }, true);
}


