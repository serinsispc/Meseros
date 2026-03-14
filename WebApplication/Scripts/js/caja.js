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

    window.ConfirmarLiberarMesa = function (btn) {
        mostrarConfirmacion({
            title: "Liberar mesa",
            text: "¿Seguro que desea liberar esta mesa?",
            confirmText: "Sí, liberar",
            confirmColor: "#dc3545"
        }, function () {
            EjecutarAccion("LiberarMesa", "", btn);
        });
        return false;
    };

    window.ConfirmarEliminarServicio = function (btn) {
        mostrarConfirmacion({
            title: "Eliminar servicio",
            text: "¿Seguro que desea eliminar el servicio activo?",
            confirmText: "Sí, eliminar",
            confirmColor: "#dc3545"
        }, function () {
            EjecutarAccion("EliminarServicio", "", btn);
        });
        return false;
    };

    window.ConfirmarCerrarCaja = function (btn) {
        mostrarConfirmacion({
            title: "Cerrar caja",
            text: "Se cerrará la caja activa y se enviará al cierre de sesión. ¿Desea continuar?",
            confirmText: "Sí, cerrar",
            confirmColor: "#d97706"
        }, function () {
            EjecutarAccion("CerrarCaja", "", btn);
        });
        return false;
    };
})();

function mostrarConfirmacion(config, onConfirm) {
    if (!window.Swal || typeof window.Swal.fire !== "function") {
        const ok = window.confirm(config.text || "¿Desea continuar?");
        if (ok && typeof onConfirm === "function") {
            onConfirm();
        }
        return;
    }

    Swal.fire({
        icon: "warning",
        title: config.title || "Confirmación",
        text: config.text || "¿Desea continuar?",
        showCancelButton: true,
        confirmButtonText: config.confirmText || "Aceptar",
        cancelButtonText: "Cancelar",
        confirmButtonColor: config.confirmColor || "#dc3545",
        cancelButtonColor: "#94a3b8",
        reverseButtons: true,
        focusCancel: true,
        customClass: {
            popup: "shadow-lg rounded-4"
        }
    }).then(function (result) {
        if (result.isConfirmed && typeof onConfirm === "function") {
            onConfirm();
        }
    });
}

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

    prepararModalCuenta({
        titulo: cuentaEditandoId ? "Editar servicio" : "Nuevo servicio",
        id: cuentaEditandoId,
        nombre: nombre,
        accion: "EditarAliasCuenta"
    });

    return false;
}

function abrirModalCuentaClienteNueva() {
    prepararModalCuenta({
        titulo: "Nueva cuenta",
        id: "",
        nombre: "",
        accion: "CrearCuentaCliente"
    });
    return false;
}

function prepararModalCuenta(config) {
    const titulo = document.getElementById("modalCuentaTitulo");
    const idEditar = document.getElementById("idCuentaModalEditar");
    const accionInput = document.getElementById("accionCuentaModal");
    const input = document.getElementById("txtCuentaNombre");
    const err = document.getElementById("cuentaError");
    const modalEl = document.getElementById("modalCuentaCliente");

    if (!modalEl) {
        return;
    }

    if (titulo) titulo.innerText = config.titulo || "Cuenta";
    if (idEditar) idEditar.value = config.id || "";
    if (accionInput) accionInput.value = config.accion || "EditarAliasCuenta";
    if (input) input.value = config.nombre || "";
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
}

function guardarCuentaDirecto(btn) {
    const nombre = (document.getElementById("txtCuentaNombre").value || "").trim();
    const idCuenta = document.getElementById("idCuentaModalEditar").value;
    const accion = document.getElementById("accionCuentaModal").value || "EditarAliasCuenta";
    const error = document.getElementById("cuentaError");

    if (nombre.length < 2) {
        error.classList.remove("d-none");
        return;
    }

    error.classList.add("d-none");

    const modalEl = document.getElementById("modalCuentaCliente");
    const modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) modal.hide();

    if (accion === "CrearCuentaCliente") {
        EjecutarAccion("CrearCuentaCliente", nombre, btn);
        return;
    }

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
        configurarDetalleCaja();
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

function agregarProductoDesdeCard(btn) {
    const item = btn.closest(".producto-item");
    if (!item) return false;

    const input = item.querySelector(".js-cantidad");
    const cantidad = input ? Math.max(1, parseInt(input.value || "1", 10) || 1) : 1;
    const id = btn.getAttribute("data-id") || item.dataset.productoId || "";

    if (!id) {
        return false;
    }

    EjecutarAccion("AgregarProducto", BuildArgs({
        ID: id,
        CANTIDAD: cantidad
    }), btn);
    return false;
}

function configurarDetalleCaja() {
    const contenedor = document.querySelector(".lista-productos") || document;

    function normalizar(input) {
        let v = parseInt(input.value, 10);
        if (isNaN(v) || v < 1) v = 1;
        input.value = v;
        return v;
    }

    contenedor.addEventListener("click", function (e) {
        const restar = e.target.closest(".js-detalle-restar");
        const sumar = e.target.closest(".js-detalle-sumar");
        if (!restar && !sumar) return;

        const card = e.target.closest(".producto-item-detalle");
        if (!card) return;

        const input = card.querySelector(".js-detalle-cantidad");
        if (!input) return;

        let cantidad = normalizar(input);
        cantidad = restar ? Math.max(1, cantidad - 1) : cantidad + 1;
        input.value = cantidad;
    });

    contenedor.addEventListener("input", function (e) {
        const input = e.target.closest(".js-detalle-cantidad");
        if (!input) return;
        input.value = input.value.replace(/[^\d]/g, "");
        normalizar(input);
    });
}

function guardarCantidadDetalle(btn) {
    const card = btn.closest(".producto-item-detalle");
    if (!card) return false;

    const input = card.querySelector(".js-detalle-cantidad");
    const cantidad = input ? Math.max(1, parseInt(input.value || "1", 10) || 1) : 1;
    const id = card.dataset.detalleId || "";

    if (!id) return false;

    EjecutarAccion("ActualizarCantidadDetalle", BuildArgs({
        ID: id,
        CANTIDAD: cantidad
    }), btn);
    return false;
}

function confirmarEliminarDetalle(btn) {
    const card = btn.closest(".producto-item-detalle");
    if (!card) return false;

    const id = card.dataset.detalleId || "";
    if (!id) return false;

    mostrarConfirmacion({
        title: "Eliminar producto",
        text: "¿Seguro que desea eliminar este producto del servicio activo?",
        confirmText: "Sí, eliminar",
        confirmColor: "#dc3545"
    }, function () {
        EjecutarAccion("EliminarDetalle", BuildArgs({
            ID: id,
            NOTA: ""
        }), btn);
    });

    return false;
}

function mostrarFuncionPendiente(nombre) {
    if (window.Swal && typeof window.Swal.fire === "function") {
        Swal.fire({
            icon: "info",
            title: nombre || "Función pendiente",
            text: "Esta opción visual ya fue restaurada. En la siguiente fase conectamos su lógica completa igual que en Menu.aspx.",
            confirmButtonText: "Entendido",
            confirmButtonColor: "#2563eb"
        });
    } else {
        alert((nombre || "Función pendiente") + ": esta opción se conectará en la siguiente fase.");
    }

    return false;
}

function obtenerCardDetalle(el) {
    return el ? el.closest('.producto-item-detalle') : null;
}

function editarNotaDetalle(btn) {
    const card = obtenerCardDetalle(btn);
    if (!card) return false;
    const id = card.dataset.detalleId || '';
    const notaActual = card.dataset.detalleNota && card.dataset.detalleNota !== '--' ? card.dataset.detalleNota : '';

    if (!window.Swal) {
        const nota = prompt('Escriba la nota del detalle', notaActual);
        if (nota === null) return false;
        EjecutarAccion('GuardarNotaDetalle', BuildArgs({ ID: id, NOTA: nota }), btn);
        return false;
    }

    Swal.fire({
        title: 'Notas del detalle',
        input: 'text',
        inputValue: notaActual,
        inputLabel: 'Nota o adición',
        inputPlaceholder: 'Escriba la nota',
        showCancelButton: true,
        confirmButtonText: 'Guardar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#2563eb'
    }).then(function (result) {
        if (result.isConfirmed) {
            EjecutarAccion('GuardarNotaDetalle', BuildArgs({ ID: id, NOTA: result.value || '' }), btn);
        }
    });
    return false;
}

function dividirDetalle(btn) {
    const card = obtenerCardDetalle(btn);
    if (!card) return false;
    const id = card.dataset.detalleId || '';
    const actual = parseInt(card.dataset.detalleCantidad || '0', 10) || 0;

    if (actual <= 1) {
        mostrarFuncionPendiente('No se puede dividir un detalle con cantidad menor o igual a 1');
        return false;
    }

    if (!window.Swal) {
        const cantidad = prompt('Cantidad a dividir', '1');
        if (cantidad === null) return false;
        EjecutarAccion('DividirDetalle', BuildArgs({ ID: id, ACTUAL: actual, DIVIDIR: cantidad }), btn);
        return false;
    }

    Swal.fire({
        title: 'Dividir detalle',
        input: 'number',
        inputValue: 1,
        inputAttributes: { min: 1, max: actual - 1, step: 1 },
        inputLabel: 'Cantidad a dividir',
        showCancelButton: true,
        confirmButtonText: 'Dividir',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#2563eb',
        preConfirm: function (value) {
            const numero = parseInt(value, 10);
            if (!numero || numero <= 0 || numero >= actual) {
                Swal.showValidationMessage('La cantidad debe ser mayor a 0 y menor a la cantidad actual.');
                return false;
            }
            return numero;
        }
    }).then(function (result) {
        if (result.isConfirmed) {
            EjecutarAccion('DividirDetalle', BuildArgs({ ID: id, ACTUAL: actual, DIVIDIR: result.value }), btn);
        }
    });
    return false;
}

function anclarDetalleCuenta(btn) {
    const card = obtenerCardDetalle(btn);
    if (!card) return false;
    const id = card.dataset.detalleId || '';
    const cuentas = Array.from(document.querySelectorAll('.cuentas-lista .cuenta-item'));
    const opciones = cuentas
        .map(function (item) {
            const onclick = item.getAttribute('onclick') || '';
            const match = onclick.match(/ID=(\d+)/);
            const cuentaId = match ? match[1] : '';
            const nombre = (item.childNodes[0] && item.childNodes[0].textContent ? item.childNodes[0].textContent : item.textContent || '').trim();
            return cuentaId && nombre ? { id: cuentaId, nombre: nombre } : null;
        })
        .filter(function (x) { return x && x.id !== '0'; });

    if (!opciones.length) {
        mostrarFuncionPendiente('No hay cuentas cliente disponibles para anclar este detalle');
        return false;
    }

    const inputOptions = {};
    opciones.forEach(function (op) { inputOptions[op.id] = op.nombre; });

    if (!window.Swal) {
        const elegido = prompt('ID de la cuenta destino: ' + opciones.map(function (o) { return o.id + ' - ' + o.nombre; }).join(', '), opciones[0].id);
        if (!elegido) return false;
        EjecutarAccion('AnclarDetalleCuenta', BuildArgs({ ID: id, CUENTA: elegido }), btn);
        return false;
    }

    Swal.fire({
        title: 'Anclar detalle a cuenta',
        input: 'select',
        inputOptions: inputOptions,
        inputPlaceholder: 'Seleccione una cuenta',
        showCancelButton: true,
        confirmButtonText: 'Anclar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#2563eb',
        preConfirm: function (value) {
            if (!value) {
                Swal.showValidationMessage('Debe seleccionar una cuenta.');
                return false;
            }
            return value;
        }
    }).then(function (result) {
        if (result.isConfirmed) {
            EjecutarAccion('AnclarDetalleCuenta', BuildArgs({ ID: id, CUENTA: result.value }), btn);
        }
    });
    return false;
}

function editarValorDetalle(btn) {
    const card = obtenerCardDetalle(btn);
    if (!card) return false;
    const id = card.dataset.detalleId || '';
    const valorActual = card.dataset.detallePrecio || '0';

    if (!window.Swal) {
        const valor = prompt('Nuevo valor del detalle', valorActual);
        if (valor === null) return false;
        EjecutarAccion('EditarValorDetalle', BuildArgs({ ID: id, VALOR: valor }), btn);
        return false;
    }

    Swal.fire({
        title: 'Editar valor',
        input: 'number',
        inputValue: valorActual,
        inputAttributes: { min: 1, step: '0.01' },
        inputLabel: 'Nuevo valor unitario',
        showCancelButton: true,
        confirmButtonText: 'Guardar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#2563eb',
        preConfirm: function (value) {
            const numero = parseFloat(value);
            if (!numero || numero <= 0) {
                Swal.showValidationMessage('Ingrese un valor mayor a cero.');
                return false;
            }
            return value;
        }
    }).then(function (result) {
        if (result.isConfirmed) {
            EjecutarAccion('EditarValorDetalle', BuildArgs({ ID: id, VALOR: result.value }), btn);
        }
    });
    return false;
}

function editarNombreDetalle(btn) {
    const card = obtenerCardDetalle(btn);
    if (!card) return false;
    const id = card.dataset.detalleId || '';
    const nombreActual = card.dataset.detalleNombre || '';

    if (!window.Swal) {
        const nombre = prompt('Nueva descripción del producto', nombreActual);
        if (nombre === null) return false;
        EjecutarAccion('EditarNombreDetalle', BuildArgs({ ID: id, NOMBRE: nombre }), btn);
        return false;
    }

    Swal.fire({
        title: 'Editar descripción',
        input: 'text',
        inputValue: nombreActual,
        inputLabel: 'Descripción visible del detalle',
        showCancelButton: true,
        confirmButtonText: 'Guardar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#2563eb',
        preConfirm: function (value) {
            if (!value || !value.trim()) {
                Swal.showValidationMessage('Ingrese una descripción válida.');
                return false;
            }
            return value.trim();
        }
    }).then(function (result) {
        if (result.isConfirmed) {
            EjecutarAccion('EditarNombreDetalle', BuildArgs({ ID: id, NOMBRE: result.value }), btn);
        }
    });
    return false;
}
