window.CajaConfig = window.CajaConfig || {};

(function () {
    const STORAGE_KEY = "caja_scroll_state:" + window.location.pathname;

    function getConfig() {
        window.CajaConfig = window.CajaConfig || {};
        return {
            autoFocusBusquedaDesktop: window.CajaConfig.autoFocusBusquedaDesktop !== false,
            desktopMinWidth: parseInt(window.CajaConfig.desktopMinWidth || "992", 10),
            preservarPosicionEnMobile: window.CajaConfig.preservarPosicionEnMobile !== false
        };
    }

    function esEscritorio() {
        const cfg = getConfig();
        return window.innerWidth >= cfg.desktopMinWidth;
    }

    function esMobile() {
        return !esEscritorio();
    }

    function obtenerScrollPagina() {
        return document.scrollingElement || document.documentElement || document.body;
    }

    function obtenerContenedoresScroll() {
        return Array.from(document.querySelectorAll(
            ".cuentas-box, .mesas-box, .zonas-box, .categorias-box, .productos-box, .h-lista, .ventas-tabla-responsive"
        ));
    }

    function asegurarKeyScroll(el, index) {
        if (!el) return "";
        if (!el.dataset.scrollKey) {
            const base =
                el.id ||
                el.getAttribute("data-scroll-id") ||
                (el.className || "").toString().replace(/\s+/g, "-").toLowerCase() ||
                ("scroll-" + index);

            el.dataset.scrollKey = base + "-" + index;
        }
        return el.dataset.scrollKey;
    }

    function guardarEstadoScroll() {
        const cfg = getConfig();
        if (!cfg.preservarPosicionEnMobile || !esMobile()) return;

        const pagina = obtenerScrollPagina();
        const estado = {
            pageTop: pagina ? pagina.scrollTop || 0 : 0,
            pageLeft: pagina ? pagina.scrollLeft || 0 : 0,
            containers: {}
        };

        obtenerContenedoresScroll().forEach(function (el, index) {
            const key = asegurarKeyScroll(el, index);
            estado.containers[key] = {
                top: el.scrollTop || 0,
                left: el.scrollLeft || 0
            };
        });

        try {
            sessionStorage.setItem(STORAGE_KEY, JSON.stringify(estado));
        } catch (e) {
        }
    }

    function restaurarEstadoScroll() {
        const cfg = getConfig();
        if (!cfg.preservarPosicionEnMobile || !esMobile()) return;

        let raw = null;
        try {
            raw = sessionStorage.getItem(STORAGE_KEY);
        } catch (e) {
            raw = null;
        }

        if (!raw) return;

        let estado = null;
        try {
            estado = JSON.parse(raw);
        } catch (e) {
            estado = null;
        }

        if (!estado) return;

        function aplicar() {
            const pagina = obtenerScrollPagina();

            if (pagina) {
                pagina.scrollTop = estado.pageTop || 0;
                pagina.scrollLeft = estado.pageLeft || 0;
            }

            window.scrollTo(estado.pageLeft || 0, estado.pageTop || 0);

            obtenerContenedoresScroll().forEach(function (el, index) {
                const key = asegurarKeyScroll(el, index);
                const item = estado.containers ? estado.containers[key] : null;
                if (!item) return;

                el.scrollTop = item.top || 0;
                el.scrollLeft = item.left || 0;
            });
        }

        requestAnimationFrame(function () {
            aplicar();
            setTimeout(aplicar, 80);
            setTimeout(aplicar, 180);
            setTimeout(aplicar, 350);
            setTimeout(aplicar, 600);
        });
    }

    function activarFocusBuscadorSiAplica() {
        const cfg = getConfig();
        if (!cfg.autoFocusBusquedaDesktop || !esEscritorio()) return;

        const input = document.getElementById("btnbuscar");
        if (!input) return;

        function enfocar() {
            try {
                input.focus({ preventScroll: true });
            } catch (e) {
                try {
                    input.focus();
                } catch (err) {
                }
            }

            try {
                const len = (input.value || "").length;
                input.setSelectionRange(len, len);
            } catch (e) {
            }
        }

        requestAnimationFrame(function () {
            enfocar();
            setTimeout(enfocar, 120);
            setTimeout(enfocar, 300);
        });
    }

    window.CajaViewport = {
        esEscritorio: esEscritorio,
        esMobile: esMobile,
        guardarEstadoScroll: guardarEstadoScroll,
        restaurarEstadoScroll: restaurarEstadoScroll,
        activarFocusBuscadorSiAplica: activarFocusBuscadorSiAplica
    };

    document.addEventListener("DOMContentLoaded", function () {
        restaurarEstadoScroll();
    });

    window.addEventListener("load", function () {
        restaurarEstadoScroll();
        activarFocusBuscadorSiAplica();
    });

    window.addEventListener("pageshow", function () {
        restaurarEstadoScroll();
        activarFocusBuscadorSiAplica();
    });

    window.addEventListener("beforeunload", function () {
        guardarEstadoScroll();
    });
})();


(function () {
    let busy = false;

    window.EjecutarAccion = function (accion, argumento, btn) {
        if (busy) return;
        busy = true;

        if (window.CajaViewport && typeof window.CajaViewport.guardarEstadoScroll === "function") {
            window.CajaViewport.guardarEstadoScroll();
        }

        const hidAccion = document.getElementById("hidAccion");
        const hidArgumento = document.getElementById("hidArgumento");
        const btnBridge = document.getElementById("btnBridge");

        if (!hidAccion || !hidArgumento || !btnBridge) {
            busy = false;
            return;
        }

        hidAccion.value = accion || "";
        hidArgumento.value = argumento || "";

        const buscador = document.getElementById("btnbuscar");
        if (buscador) {
            buscador.value = "";

            if (window.CajaBuscador && typeof window.CajaBuscador.apply === "function") {
                window.CajaBuscador.apply("");
            }
        }

        const mostrarLoader = accion !== "SeleccionarCategoria";

        if (mostrarLoader && window.SerinsisLoading && typeof window.SerinsisLoading.show === "function") {
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
            text: "Se abrirá el modulo de cierre de caja para revisar el resumen y confirmar el arqueo. ¿Desea continuar?",
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
        configurarMenuFlotanteMovil();
    });
})();




function configurarMenuFlotanteMovil() {
    const nav = document.getElementById("mobileQuickNav");
    const toggle = document.getElementById("mobileQuickNavToggle");
    const panel = document.getElementById("mobileQuickNavPanel");
    const links = Array.from(document.querySelectorAll(".mobile-quick-nav__link[data-target]"));

    if (!nav || !toggle || !panel || !links.length) {
        return;
    }

    function contenidoSuperaPantalla() {
        const altoDocumento = Math.max(
            document.body ? document.body.scrollHeight : 0,
            document.documentElement ? document.documentElement.scrollHeight : 0
        );

        return altoDocumento > (window.innerHeight + 40);
    }

    function debeMostrarMenuLateral() {
        return window.innerWidth < 1200 || contenidoSuperaPantalla();
    }

    function esMenuLateralActivo() {
        return nav.classList.contains("is-enabled");
    }

    function actualizarEstadoMenu() {
        const visible = debeMostrarMenuLateral();
        nav.classList.toggle("is-enabled", visible);

        if (!visible) {
            cerrar();
        }
    }

    function abrir() {
        nav.classList.add("is-open");
        toggle.setAttribute("aria-expanded", "true");
    }

    function cerrar() {
        nav.classList.remove("is-open");
        toggle.setAttribute("aria-expanded", "false");
    }

    toggle.addEventListener("click", function () {
        if (!esMenuLateralActivo()) return;

        if (nav.classList.contains("is-open")) {
            cerrar();
        } else {
            abrir();
        }
    });

    links.forEach(function (btn) {
        btn.addEventListener("click", function () {
            const targetId = btn.getAttribute("data-target");
            if (!targetId) return;

            const destino = document.getElementById(targetId);
            if (!destino) return;

            const top = destino.getBoundingClientRect().top + window.pageYOffset - 12;

            window.scrollTo({
                top: top,
                behavior: "smooth"
            });

            setTimeout(function () {
                cerrar();
            }, 180);
        });
    });

    document.addEventListener("click", function (e) {
        if (!nav.classList.contains("is-open")) return;
        if (nav.contains(e.target)) return;
        cerrar();
    });

    window.addEventListener("resize", function () {
        actualizarEstadoMenu();
    });

    window.addEventListener("load", actualizarEstadoMenu);
    window.addEventListener("pageshow", actualizarEstadoMenu);
    document.addEventListener("DOMContentLoaded", actualizarEstadoMenu);
    setTimeout(actualizarEstadoMenu, 200);
    setTimeout(actualizarEstadoMenu, 800);

    actualizarEstadoMenu();
}



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

function escapeHtml(value) {
    return (value || '').toString()
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function normalizarListaNotas(texto) {
    return (texto || '')
        .split(';')
        .map(function (item) { return item.trim(); })
        .filter(function (item) { return item.length > 0; });
}

function obtenerAdicionesPredeterminadas(idCategoria) {
    const categoriaId = parseInt(idCategoria || '0', 10) || 0;
    const catalogo = Array.isArray(window.CajaAdicionesCatalogo) ? window.CajaAdicionesCatalogo : [];

    return catalogo
        .filter(function (item) {
            return (parseInt(item.idCategoria || '0', 10) || 0) === categoriaId;
        })
        .map(function (item) {
            return (item.nombreAdicion || '').toString().trim();
        })
        .filter(function (item, index, arr) {
            return item.length > 0 && arr.indexOf(item) === index;
        });
}

function construirHtmlAdicionesPredeterminadas(adiciones, notaActual) {
    if (!adiciones.length) {
        return '<div class="text-muted small mb-2">Este producto no tiene adiciones predeterminadas configuradas.</div>';
    }

    const seleccionadas = new Set(normalizarListaNotas(notaActual).map(function (item) {
        return item.toLowerCase();
    }));

    return [
        '<div class="mb-2 text-start fw-semibold">Adiciones predeterminadas</div>',
        '<div id="swalAdicionesPredeterminadas" class="d-flex flex-wrap gap-2 mb-3">',
        adiciones.map(function (adicion) {
            const activa = seleccionadas.has(adicion.toLowerCase());
            return '<button type="button" class="btn btn-sm ' + (activa ? 'btn-primary' : 'btn-outline-primary') + ' js-adicion-preset" data-adicion="' + escapeHtml(adicion) + '">' + escapeHtml(adicion) + '</button>';
        }).join(''),
        '</div>'
    ].join('');
}

function editarNotaDetalle(btn) {
    const card = obtenerCardDetalle(btn);
    if (!card) return false;
    const id = card.dataset.detalleId || '';
    const notaActual = card.dataset.detalleNota && card.dataset.detalleNota !== '--' ? card.dataset.detalleNota : '';
    const idCategoria = card.dataset.detalleCategoriaId || '';
    const adicionesPredeterminadas = obtenerAdicionesPredeterminadas(idCategoria);

    if (!window.Swal) {
        const nota = prompt('Escriba la nota del detalle', notaActual);
        if (nota === null) return false;
        EjecutarAccion('GuardarNotaDetalle', BuildArgs({ ID: id, NOTA: nota }), btn);
        return false;
    }

    Swal.fire({
        title: 'Notas del detalle',
        html: [
            construirHtmlAdicionesPredeterminadas(adicionesPredeterminadas, notaActual),
            '<label for="swalNotaDetalle" class="form-label d-block text-start">Nota o adicion</label>',
            '<textarea id="swalNotaDetalle" class="swal2-textarea m-0" style="display:block;width:100%;max-width:100%;min-width:100%;min-height:140px;box-sizing:border-box;" placeholder="Escriba la nota">' + escapeHtml(notaActual) + '</textarea>',
            '<div class="d-flex justify-content-end mt-2"><button type="button" id="swalNotaDetalleLimpiar" class="btn btn-outline-danger btn-sm">Borrar</button></div>'
        ].join(''),
        showCancelButton: true,
        confirmButtonText: 'Guardar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#2563eb',
        focusConfirm: false,
        didOpen: function () {
            const textarea = document.getElementById('swalNotaDetalle');
            const presetButtons = Array.from(document.querySelectorAll('.js-adicion-preset'));
            const clearButton = document.getElementById('swalNotaDetalleLimpiar');

            function getNotasActuales() {
                return normalizarListaNotas(textarea ? textarea.value : '');
            }

            function setNotasActuales(items) {
                if (!textarea) return;
                textarea.value = items.join('; ');
            }

            if (clearButton) {
                clearButton.addEventListener('click', function () {
                    if (textarea) {
                        textarea.value = '';
                        textarea.focus();
                    }

                    presetButtons.forEach(function (presetBtn) {
                        presetBtn.classList.remove('btn-primary');
                        presetBtn.classList.add('btn-outline-primary');
                    });
                });
            }

            presetButtons.forEach(function (presetBtn) {
                presetBtn.addEventListener('click', function () {
                    const adicion = (presetBtn.dataset.adicion || '').trim();
                    if (!adicion) return;

                    const actuales = getNotasActuales();
                    const indice = actuales.findIndex(function (item) {
                        return item.toLowerCase() === adicion.toLowerCase();
                    });

                    if (indice >= 0) {
                        actuales.splice(indice, 1);
                        presetBtn.classList.remove('btn-primary');
                        presetBtn.classList.add('btn-outline-primary');
                    } else {
                        actuales.push(adicion);
                        presetBtn.classList.remove('btn-outline-primary');
                        presetBtn.classList.add('btn-primary');
                    }

                    setNotasActuales(actuales);
                    if (textarea) textarea.focus();
                });
            });

            if (textarea) {
                textarea.focus();
                textarea.setSelectionRange(textarea.value.length, textarea.value.length);
            }
        },
        preConfirm: function () {
            const textarea = document.getElementById('swalNotaDetalle');
            return textarea ? textarea.value || '' : '';
        }
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

(function () {
    let modalPropina;
    let subtotal = 0;
    let porcentaje = 0;
    let propina = 0;
    let idventa = 0;
    let idcuenta = 0;
    let lastEdited = null;
    const MAX_PERCENT = 15;
    const nfCOP = new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 });

    function byId(id) { return document.getElementById(id); }
    function formatCOP(n) { return nfCOP.format(isFinite(n) ? n : 0); }
    function clamp(n, min, max) { return Math.min(Math.max(n, min), max); }
    function roundTo100(n) { return Math.round(n / 100) * 100; }
    function parseNumber(str) {
        if (typeof str === 'number') return str;
        if (!str) return 0;
        str = String(str).trim().replace(/[^\d.,-]/g, '');
        if (str.includes('.') && str.includes(',')) str = str.replace(/\./g, '').replace(',', '.');
        else if (str.includes(',')) str = str.replace(/\./g, '').replace(',', '.');
        else str = str.replace(/\./g, '');
        const n = parseFloat(str);
        return Number.isFinite(n) ? n : 0;
    }
    function renderHelp() {
        const ayuda = byId('ayudaPropina');
        if (ayuda) ayuda.textContent = 'Esto equivale a ' + porcentaje + '% sobre ' + formatCOP(subtotal) + '.';
    }
    function syncFromPercent() {
        const txtPorcentaje = byId('txtPorcentajePropina');
        const txtPropina = byId('txtValorPropina');
        porcentaje = Math.round(clamp(parseNumber(txtPorcentaje ? txtPorcentaje.value : 0), 0, MAX_PERCENT));
        const calc = (subtotal * porcentaje) / 100;
        propina = Math.min(roundTo100(calc), subtotal);
        if (txtPorcentaje) txtPorcentaje.value = porcentaje;
        if (txtPropina) txtPropina.value = formatCOP(propina);
        lastEdited = 'percent';
        renderHelp();
    }
    function syncFromValue() {
        const txtPorcentaje = byId('txtPorcentajePropina');
        const txtPropina = byId('txtValorPropina');
        let raw = Math.max(0, parseNumber(txtPropina ? txtPropina.value : 0));
        propina = Math.min(roundTo100(raw), subtotal);
        porcentaje = subtotal > 0 ? Math.round((propina / subtotal) * 100) : 0;
        porcentaje = clamp(porcentaje, 0, MAX_PERCENT);
        if (txtPorcentaje) txtPorcentaje.value = porcentaje;
        if (txtPropina) txtPropina.value = formatCOP(propina);
        lastEdited = 'value';
        renderHelp();
    }
    function ensureModalPropina() {
        const modalEl = byId('modalPropina');
        if (!modalEl || !window.bootstrap || !window.bootstrap.Modal) return null;
        if (!modalPropina) modalPropina = bootstrap.Modal.getOrCreateInstance(modalEl);
        return modalPropina;
    }

    window.abrirModalPropina = function (btn) {
        const txtSubtotal = byId('txtSubtotalPropina');
        const txtPorcentaje = byId('txtPorcentajePropina');
        const txtPropina = byId('txtValorPropina');
        if (!btn) return false;

        subtotal = parseNumber(btn.dataset.subtotal);
        porcentaje = Math.round(clamp(parseNumber(btn.dataset.porcentaje), 0, MAX_PERCENT));
        propina = Math.max(0, parseNumber(btn.dataset.propina));
        idventa = parseInt(btn.dataset.idventa || '0', 10) || 0;
        idcuenta = parseInt(btn.dataset.idcuenta || '0', 10) || 0;

        if (propina > 0) {
            porcentaje = subtotal > 0 ? Math.round(clamp((propina / subtotal) * 100, 0, MAX_PERCENT)) : 0;
        } else {
            const calc = (subtotal * porcentaje) / 100;
            propina = Math.min(roundTo100(calc), subtotal);
        }

        if (txtSubtotal) txtSubtotal.value = formatCOP(subtotal);
        if (txtPorcentaje) txtPorcentaje.value = porcentaje;
        if (txtPropina) txtPropina.value = formatCOP(propina);
        lastEdited = null;
        renderHelp();
        ensureModalPropina()?.show();
        return false;
    };

    window.eliminarPropinaResumen = function (btn) {
        if (!btn) return false;

        const idVentaEliminar = parseInt(btn.dataset.idventa || '0', 10) || 0;
        const idCuentaEliminar = parseInt(btn.dataset.idcuenta || '0', 10) || 0;

        EjecutarAccion('EditarPropina', JSON.stringify({
            porcentaje: 0,
            propina: 0,
            idventa: idVentaEliminar,
            idcuenta: idCuentaEliminar
        }), btn);

        return false;
    };

    document.addEventListener('DOMContentLoaded', function () {
        const txtPorcentaje = byId('txtPorcentajePropina');
        const txtPropina = byId('txtValorPropina');
        const btnGuardar = byId('btnGuardarPropina');
        const btnQuitar = byId('btnQuitarPropina');

        txtPorcentaje?.addEventListener('input', function () {
            porcentaje = Math.round(clamp(parseNumber(txtPorcentaje.value), 0, MAX_PERCENT));
            const calc = (subtotal * porcentaje) / 100;
            propina = Math.min(roundTo100(calc), subtotal);
            if (txtPropina) txtPropina.value = formatCOP(propina);
            lastEdited = 'percent';
            renderHelp();
        });
        txtPropina?.addEventListener('input', function () {
            let raw = Math.max(0, parseNumber(txtPropina.value));
            let pTmp = subtotal > 0 ? Math.round((raw / subtotal) * 100) : 0;
            pTmp = clamp(pTmp, 0, MAX_PERCENT);
            if (txtPorcentaje) txtPorcentaje.value = pTmp;
            lastEdited = 'value';
            renderHelp();
        });
        txtPorcentaje?.addEventListener('blur', syncFromPercent);
        txtPropina?.addEventListener('blur', syncFromValue);
        document.querySelectorAll('.quick-tip').forEach(function (btn) {
            btn.addEventListener('click', function () {
                if (txtPorcentaje) txtPorcentaje.value = parseNumber(btn.dataset.tip);
                syncFromPercent();
            });
        });
        btnQuitar?.addEventListener('click', function () {
            if (txtPorcentaje) txtPorcentaje.value = '0';
            if (txtPropina) txtPropina.value = formatCOP(0);
            syncFromPercent();
        });
        btnGuardar?.addEventListener('click', function () {
            if (lastEdited === 'value') syncFromValue(); else syncFromPercent();
            EjecutarAccion('EditarPropina', JSON.stringify({ porcentaje: porcentaje, propina: propina, idventa: idventa, idcuenta: idcuenta }), btnGuardar);
        });
    });

    window.ConfirmarCerrarSesion = function (btn) {
        mostrarConfirmacion({
            title: "Cerrar sesion",
            text: "La sesion actual se cerrara, pero la caja seguira activa. ¿Desea continuar?",
            confirmText: "Si, salir",
            confirmColor: "#dc2626"
        }, function () {
            EjecutarAccion("CerrarSesion", "", btn);
        });
        return false;
    };
})();

window.btnDomicilioCaja = function (btn) {
    const idMesa = btn.getAttribute("data-idmesa") || "0";
    const idServicio = btn.getAttribute("data-idservicio") || "0";
    EjecutarAccion("Domicilio", idMesa + "|" + idServicio, btn);
    return false;
};

window.cargarTablaDomicilios = function (filtro) {
    const lista = window.ListaClientesDomicilio || [];
    const tbody = document.querySelector('#tblDomicilios tbody');
    if (!tbody) return 0;

    tbody.innerHTML = "";
    const valor = (filtro || "").trim();
    const busqueda = valor.toUpperCase();
    const filtrados = valor
        ? lista.filter(function (item) {
            const tel = (item.celularCliente || "").toString();
            const nom = (item.nombreCliente || "").toString().toUpperCase();
            return tel.indexOf(valor) === 0 || nom.indexOf(busqueda) !== -1;
        })
        : lista;

    filtrados.forEach(function (item) {
        const tr = document.createElement('tr');
        tr.innerHTML = '<td>' + (item.celularCliente || '') + '</td><td>' + (item.nombreCliente || '') + '</td><td>' + (item.direccionCliente || '') + '</td>';
        tr.addEventListener('click', function () {
            document.getElementById('txtTelefono').value = item.celularCliente || '';
            document.getElementById('txtNombreCliente').value = item.nombreCliente || '';
            document.getElementById('txtDireccion').value = item.direccionCliente || '';
            const hd = document.getElementById('hdIdClienteDomicilio');
            if (hd) hd.value = item.id || '';
        });
        tbody.appendChild(tr);
    });

    return filtrados.length;
};

(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const txtBuscar = document.getElementById('txtBuscarCelular');
        const txtTelefono = document.getElementById('txtTelefono');
        const hdIdCliente = document.getElementById('hdIdClienteDomicilio');
        const btnCrear = document.getElementById('btnCrearDomicilio');
        const btnSeleccionar = document.getElementById('btnSeleccionarDomicilio');

        if (txtBuscar) {
            txtBuscar.addEventListener('input', function () {
                this.value = this.value.replace(/\D/g, '').substring(0, 10);
                cargarTablaDomicilios(this.value);
            });

            txtBuscar.addEventListener('keydown', function (e) {
                if (e.key !== 'Enter') return;
                e.preventDefault();
                const filtro = (this.value || '').trim();
                if (filtro.length !== 10) {
                    if (window.Swal) {
                        Swal.fire({ icon: 'warning', title: 'Numero invalido', text: 'El numero debe tener exactamente 10 digitos.' });
                    }
                    return;
                }
                const cantidad = cargarTablaDomicilios(filtro);
                if (cantidad === 0) {
                    if (txtTelefono) txtTelefono.value = filtro;
                    if (hdIdCliente) hdIdCliente.value = '';
                    const txtNombre = document.getElementById('txtNombreCliente');
                    if (txtNombre) txtNombre.focus();
                }
            });
        }

        if (txtTelefono) {
            txtTelefono.addEventListener('input', function () {
                this.value = this.value.replace(/\D/g, '').substring(0, 10);
            });
        }

        if (btnCrear) {
            btnCrear.addEventListener('click', function (e) {
                e.preventDefault();
                const id = hdIdCliente ? (hdIdCliente.value || '').trim() : '';
                const tel = (document.getElementById('txtTelefono').value || '').trim();
                const nom = (document.getElementById('txtNombreCliente').value || '').trim();
                const dir = (document.getElementById('txtDireccion').value || '').trim();
                if (!tel || !nom || !dir) {
                    if (window.Swal) {
                        Swal.fire({ icon: 'warning', title: 'Campos incompletos', text: 'Debe llenar Telefono, Nombre y Direccion.' });
                    }
                    return;
                }
                EjecutarAccion('CrearActualizarClienteDomicilio', [id, tel, nom, dir].join('|'), btnCrear);
            });
        }

        if (btnSeleccionar) {
            btnSeleccionar.addEventListener('click', function (e) {
                e.preventDefault();
                const id = hdIdCliente ? (hdIdCliente.value || '').trim() : '';
                const tel = (document.getElementById('txtTelefono').value || '').trim();
                const nom = (document.getElementById('txtNombreCliente').value || '').trim();
                const dir = (document.getElementById('txtDireccion').value || '').trim();
                if (!id) {
                    if (window.Swal) {
                        Swal.fire({ icon: 'warning', title: 'Seleccione un cliente', text: 'Debe seleccionar un cliente de la lista o crearlo antes.' });
                    }
                    return;
                }
                EjecutarAccion('SeleccionarClienteDomicilio', [id, tel, nom, dir].join('|'), btnSeleccionar);
            });
        }
    });
})();

(function () {
    function getRelDomicilio() {
        const hf = document.getElementById('hfRelMediosInternosDomicilio');
        if (!hf || !hf.value) return [];
        try { return JSON.parse(hf.value); } catch { return []; }
    }

    function actualizarPlaceholderDomicilio(nombreMedio) {
        const sel = document.getElementById('selMetodoPagoDomicilio');
        if (!sel || !sel.options || sel.options.length === 0) return;
        sel.options[0].text = nombreMedio && String(nombreMedio).trim()
            ? String(nombreMedio).trim()
            : 'Selecciona un medio';
    }

    function obtenerNombreBaseDomicilio(idMedioBase) {
        const sel = document.getElementById('selMetodoPagoDomicilio');
        if (!sel || !sel.options) return '';
        const id = parseInt(idMedioBase || '0', 10) || 0;
        if (!id) return '';
        const opt = Array.from(sel.options).find(x => parseInt(x.value || '0', 10) === id);
        return opt ? String(opt.text || '').trim() : '';
    }

    function obtenerNombreInternoDomicilio(idMedioBase, idMedioInterno) {
        const rel = getRelDomicilio();
        const idBase = parseInt(idMedioBase || '0', 10) || 0;
        const idInterno = Math.abs(parseInt(idMedioInterno || '0', 10) || 0);
        if (!idBase || !idInterno) return '';
        const item = rel.find(x =>
            parseInt(x.idMedioDePago, 10) === idBase &&
            parseInt(x.idMediosDePagoInternos, 10) === idInterno);
        return item ? String(item.nombreRMPI || '').trim() : '';
    }

    function actualizarResumenCobroDomicilio() {
        const lbl = document.getElementById('lblResumenMedioDomicilio');
        const sel = document.getElementById('selMetodoPagoDomicilio');
        const hfMedioBase = document.getElementById('hfMedioBaseDomicilio');
        const hfMedioInterno = document.getElementById('hfMedioInternoDomicilio');
        if (!lbl) return;

        const idBase = parseInt((sel && sel.value) || (hfMedioBase && hfMedioBase.value) || '0', 10) || 0;
        const idInterno = parseInt((hfMedioInterno && hfMedioInterno.value) || '0', 10) || 0;

        let texto = 'Sin definir';
        if (idBase > 0) {
            const nombreBase = obtenerNombreBaseDomicilio(idBase);
            texto = nombreBase || texto;

            if (idInterno < 0) {
                const nombreInterno = obtenerNombreInternoDomicilio(idBase, idInterno);
                if (nombreInterno) {
                    texto = nombreBase ? (nombreBase + ' / ' + nombreInterno) : nombreInterno;
                }
            }
        }

        lbl.textContent = texto;
    }

    function esMetodoEfectivo(selectEl) {
        if (!selectEl) return false;
        const option = selectEl.options[selectEl.selectedIndex];
        const texto = option ? (option.text || '') : '';
        return String(selectEl.value || '') === '10' || texto.toLowerCase().indexOf('efect') >= 0;
    }

    function actualizarCobroDomicilioUI() {
        const sel = document.getElementById('selMetodoPagoDomicilio');
        const box = document.getElementById('boxMontoBilleteDomicilio');
        const txt = document.getElementById('txtMontoBilleteDomicilio');
        if (!sel || !box) return;

        const mostrar = esMetodoEfectivo(sel);
        box.style.display = mostrar ? '' : 'none';
        if (!mostrar && txt) {
            txt.value = '';
        }
    }

    function abrirModalInternosDomicilio(idMedioPago, nombreMedioPago) {
        const modalEl = document.getElementById('mdlMediosInternosDomicilio');
        const cont = document.getElementById('listMediosInternosDomicilio');
        const lbl = document.getElementById('lblMedioSeleccionadoDomicilio');
        const sel = document.getElementById('selMetodoPagoDomicilio');
        const hfMedioBase = document.getElementById('hfMedioBaseDomicilio');
        const hfMedioInterno = document.getElementById('hfMedioInternoDomicilio');
        const rel = getRelDomicilio();
        if (!modalEl || !cont) return;
        const id = parseInt(idMedioPago, 10);
        const filtrados = rel.filter(x => parseInt(x.idMedioDePago, 10) === id);
        if (!filtrados.length) return;

        if (lbl) lbl.textContent = `Medio seleccionado: ${nombreMedioPago} (ID ${id})`;
        cont.innerHTML = '';

        filtrados.forEach(item => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'list-group-item list-group-item-action d-flex justify-content-between align-items-center';
            btn.innerHTML = `
                <div>
                    <div class="fw-bold">${item.nombreRMPI || 'Sin nombre'}</div>
                    <div class="text-muted" style="font-size:12px;">${item.reporteRDIAN || ''}</div>
                </div>
                <span class="badge text-bg-secondary">ID ${item.idMediosDePagoInternos}</span>
            `;

            btn.addEventListener('click', () => {
                if (hfMedioBase) {
                    hfMedioBase.value = String(id);
                }
                if (hfMedioInterno) {
                    hfMedioInterno.value = String(parseInt(item.idMediosDePagoInternos || '0', 10) * -1);
                }
                if (sel) {
                    actualizarPlaceholderDomicilio(nombreMedioPago);
                    sel.value = '';
                }
                actualizarCobroDomicilioUI();
                actualizarResumenCobroDomicilio();

                if (window.bootstrap) {
                    bootstrap.Modal.getOrCreateInstance(modalEl).hide();
                }
            });

            cont.appendChild(btn);
        });

        if (window.bootstrap) {
            bootstrap.Modal.getOrCreateInstance(modalEl, { backdrop: 'static' }).show();
        }
    }

    window.guardarCobroDomicilio = function (btn) {
        const sel = document.getElementById('selMetodoPagoDomicilio');
        const txt = document.getElementById('txtMontoBilleteDomicilio');
        const hfMedioBase = document.getElementById('hfMedioBaseDomicilio');
        const hfMedioInterno = document.getElementById('hfMedioInternoDomicilio');
        const idMetodoBase = parseInt((sel && sel.value) || (hfMedioBase && hfMedioBase.value) || '0', 10) || 0;
        if (!sel || !idMetodoBase) {
            if (window.Swal) {
                Swal.fire({ icon: 'warning', title: 'Medio de pago', text: 'Selecciona cómo va a pagar el cliente.' });
            }
            return false;
        }

        const rel = getRelDomicilio();
        const tieneInternos = rel.some(x => parseInt(x.idMedioDePago, 10) === idMetodoBase);
        const idMetodoFinal = (hfMedioInterno && hfMedioInterno.value) ? parseInt(hfMedioInterno.value || '0', 10) : idMetodoBase;

        if (tieneInternos && (!idMetodoFinal || idMetodoFinal >= 0)) {
            const nombre = sel.value
                ? (sel.options[sel.selectedIndex] ? (sel.options[sel.selectedIndex].text || '') : '')
                : ((sel.options && Array.from(sel.options).find(x => parseInt(x.value || '0', 10) === idMetodoBase)?.text) || '');
            abrirModalInternosDomicilio(idMetodoBase, nombre);
            return false;
        }

        const nombreBase = ((sel.options && Array.from(sel.options).find(x => parseInt(x.value || '0', 10) === idMetodoBase)?.text) || '');
        const esEfectivo = idMetodoBase === 10 || nombreBase.toLowerCase().indexOf('efect') >= 0;
        const monto = txt ? String(txt.value || '').replace(/\D/g, '') : '';
        if (esEfectivo && !monto) {
            if (window.Swal) {
                Swal.fire({ icon: 'warning', title: 'Billete del cliente', text: 'Indica con qué billete va a pagar para preparar las vueltas.' });
            }
            return false;
        }

        EjecutarAccion('GuardarCobroDomicilio', BuildArgs({
            IDMEDIO: idMetodoFinal,
            MONTO: esEfectivo ? monto : ''
        }), btn);
        return false;
    };

    document.addEventListener('DOMContentLoaded', function () {
        const sel = document.getElementById('selMetodoPagoDomicilio');
        const txt = document.getElementById('txtMontoBilleteDomicilio');
        const hfMedioBase = document.getElementById('hfMedioBaseDomicilio');
        const hfMedioInterno = document.getElementById('hfMedioInternoDomicilio');
        if (sel) {
            sel.addEventListener('change', function () {
                const idMetodoBase = parseInt(sel.value || '0', 10) || 0;
                if (hfMedioBase) {
                    hfMedioBase.value = idMetodoBase > 0 ? String(idMetodoBase) : '';
                }
                if (hfMedioInterno) {
                    hfMedioInterno.value = '';
                }

                const rel = getRelDomicilio();
                const tieneInternos = rel.some(x => parseInt(x.idMedioDePago, 10) === idMetodoBase);
                const nombre = sel.options[sel.selectedIndex] ? (sel.options[sel.selectedIndex].text || '') : '';

                actualizarPlaceholderDomicilio();
                actualizarCobroDomicilioUI();
                actualizarResumenCobroDomicilio();

                if (tieneInternos && idMetodoBase > 0) {
                    abrirModalInternosDomicilio(idMetodoBase, nombre);
                }
            });

            const nombreBaseActual = ((sel.options && Array.from(sel.options).find(x => parseInt(x.value || '0', 10) === parseInt((hfMedioBase && hfMedioBase.value) || '0', 10))?.text) || '');
            if (hfMedioInterno && hfMedioInterno.value && nombreBaseActual) {
                actualizarPlaceholderDomicilio(nombreBaseActual);
                sel.value = '';
            } else {
                actualizarPlaceholderDomicilio();
            }
            actualizarCobroDomicilioUI();
            actualizarResumenCobroDomicilio();
        }
        if (txt) {
            txt.addEventListener('input', function () {
                this.value = this.value.replace(/\D/g, '');
            });
        }
    });
})();

(function () {
    function escapeHtml(text) {
        return String(text || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    window.imprimirDomicilioActivo = function () {
        const data = window.DomicilioActivoPrintData;
        if (!data) return false;

        const items = Array.isArray(data.items) ? data.items : [];
        const filas = items.length
            ? items.map(function (item) {
                return '<tr>'
                    + '<td style="padding:4px 0;">' + escapeHtml(item.cantidad) + 'x</td>'
                    + '<td style="padding:4px 8px;">' + escapeHtml(item.producto) + (item.nota ? '<div style="font-size:11px;color:#475569;">' + escapeHtml(item.nota) + '</div>' : '') + '</td>'
                    + '<td style="padding:4px 0;text-align:right;">' + escapeHtml(item.valor) + '</td>'
                    + '</tr>';
            }).join('')
            : '<tr><td colspan="3" style="padding:6px 0;color:#64748b;text-align:center;">Sin productos cargados</td></tr>';

        const html = '<!doctype html><html><head><meta charset="utf-8"><title>Domicilio ' + escapeHtml(data.cuenta) + '</title>'
            + '<style>'
            + 'body{font-family:Consolas,monospace;padding:18px;color:#0f172a;}'
            + '.t{font-size:18px;font-weight:800;text-align:center;margin-bottom:4px;}'
            + '.s{text-align:center;font-size:12px;margin-bottom:14px;}'
            + '.b{margin-bottom:10px;font-size:12px;}'
            + '.k{font-weight:800;}'
            + 'table{width:100%;border-collapse:collapse;font-size:12px;margin-top:8px;}'
            + '.f{margin-top:12px;border-top:1px dashed #94a3b8;padding-top:10px;font-size:12px;}'
            + '</style></head><body>'
            + '<div class="t">DOMICILIO</div>'
            + '<div class="s">Cuenta ' + escapeHtml(data.cuenta) + ' · Estado: ' + escapeHtml(data.estado) + '</div>'
            + '<div class="b"><span class="k">Cliente:</span> ' + escapeHtml(data.cliente) + '</div>'
            + '<div class="b"><span class="k">Teléfono:</span> ' + escapeHtml(data.telefono) + '</div>'
            + '<div class="b"><span class="k">Dirección:</span> ' + escapeHtml(data.direccion) + '</div>'
            + '<div class="b"><span class="k">Medio de pago:</span> ' + escapeHtml(data.medioPago) + '</div>'
            + (data.pagaCon ? '<div class="b"><span class="k">Paga con:</span> ' + escapeHtml(data.pagaCon) + '</div>' : '')
            + (data.vueltas ? '<div class="b"><span class="k">Vueltas:</span> ' + escapeHtml(data.vueltas) + '</div>' : '')
            + '<div class="b"><span class="k">Total:</span> ' + escapeHtml(data.total) + '</div>'
            + (data.observacion ? '<div class="b"><span class="k">Observación:</span> ' + escapeHtml(data.observacion) + '</div>' : '')
            + '<table><thead><tr><th style="text-align:left;">Cant</th><th style="text-align:left;">Producto</th><th style="text-align:right;">Valor</th></tr></thead><tbody>' + filas + '</tbody></table>'
            + '<div class="f">Ticket de despacho de domicilio</div>'
            + '</body></html>';

        const win = window.open('', '_blank', 'width=420,height=760');
        if (!win) return false;
        win.document.open();
        win.document.write(html);
        win.document.close();
        win.focus();
        setTimeout(function () { win.print(); }, 300);
        return false;
    };
})();

(function () {
    function escapeHtmlTicket(text) {
        return String(text || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    window.imprimirDomicilioActivo = function () {
        const data = window.DomicilioActivoPrintData;
        if (!data) return false;

        const widthMm = parseInt(data.printerWidth || 80, 10) <= 58 ? 58 : 80;
        const maxWidth = widthMm === 58 ? '54mm' : '76mm';
        const fontSize = widthMm === 58 ? '10px' : '12px';
        const titleSize = widthMm === 58 ? '15px' : '18px';
        const smallSize = widthMm === 58 ? '9px' : '10px';
        const popupWidth = widthMm === 58 ? 420 : 520;
        const fecha = new Date();
        const fechaTexto = fecha.toLocaleString('es-CO', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });

        const items = Array.isArray(data.items) ? data.items : [];
        const filas = items.length
            ? items.map(function (item) {
                return '<tr>'
                    + '<td style="padding:4px 0;">' + escapeHtmlTicket(item.cantidad) + 'x</td>'
                    + '<td style="padding:4px 8px;">' + escapeHtmlTicket(item.producto) + (item.nota ? '<div style="font-size:11px;color:#475569;">' + escapeHtmlTicket(item.nota) + '</div>' : '') + '</td>'
                    + '<td style="padding:4px 0;text-align:right;">' + escapeHtmlTicket(item.valor) + '</td>'
                    + '</tr>';
            }).join('')
            : '<tr><td colspan="3" style="padding:6px 0;color:#64748b;text-align:center;">Sin productos cargados</td></tr>';

        const cabeceraRestaurante = [
            data.restaurante,
            data.nitRestaurante ? 'NIT: ' + data.nitRestaurante : '',
            data.regimenRestaurante,
            data.direccionRestaurante,
            data.telefonoRestaurante ? 'Tel: ' + data.telefonoRestaurante : '',
            data.horarioRestaurante,
            data.puntoPago ? 'Punto: ' + data.puntoPago : '',
            data.leyendaRestaurante1,
            data.leyendaRestaurante2
        ].filter(function (linea) {
            return String(linea || '').trim() !== '';
        }).map(function (linea, index) {
            return '<div class="' + (index === 0 ? 'r1' : 'r2') + '">' + escapeHtmlTicket(linea) + '</div>';
        }).join('');

        const html = '<!doctype html><html><head><meta charset="utf-8"><title>Domicilio ' + escapeHtmlTicket(data.cuenta) + '</title>'
            + '<style>'
            + '@page{size:' + widthMm + 'mm auto;margin:4mm;}'
            + 'html,body{margin:0;padding:0;background:#fff;color:#0f172a;font-family:Consolas,monospace;font-size:' + fontSize + ';}'
            + 'body{padding:6px;}'
            + '.ticket{width:100%;max-width:' + maxWidth + ';margin:0 auto;}'
            + '.restaurant{border-bottom:1px dashed #94a3b8;padding-bottom:8px;margin-bottom:8px;text-align:center;}'
            + '.r1{font-size:' + titleSize + ';font-weight:800;line-height:1.15;}'
            + '.r2{font-size:' + fontSize + ';margin-top:2px;line-height:1.25;}'
            + '.t{font-size:' + titleSize + ';font-weight:800;text-align:center;margin:4px 0 2px;}'
            + '.s{text-align:center;font-size:' + fontSize + ';margin-bottom:10px;line-height:1.3;}'
            + '.b{margin-bottom:7px;font-size:' + fontSize + ';line-height:1.3;}'
            + '.k{font-weight:800;}'
            + '.sec{margin-top:8px;padding-top:8px;border-top:1px dashed #94a3b8;}'
            + 'table{width:100%;border-collapse:collapse;font-size:' + fontSize + ';margin-top:6px;}'
            + 'th,td{vertical-align:top;}'
            + 'th{padding-bottom:4px;border-bottom:1px solid #cbd5e1;}'
            + '.meta{display:flex;justify-content:space-between;gap:8px;font-size:' + smallSize + ';margin:2px 0 10px;color:#334155;}'
            + '.meta div:last-child{text-align:right;}'
            + '.f{margin-top:10px;border-top:1px dashed #94a3b8;padding-top:8px;font-size:' + smallSize + ';text-align:center;line-height:1.35;}'
            + '</style></head><body>'
            + '<div class="ticket">'
            + '<div class="restaurant">'
            + cabeceraRestaurante
            + '</div>'
            + '<div class="t">DOMICILIO</div>'
            + '<div class="s">Cuenta ' + escapeHtmlTicket(data.cuenta) + ' - Estado: ' + escapeHtmlTicket(data.estado) + '</div>'
            + '<div class="meta"><div>Fecha: ' + escapeHtmlTicket(fechaTexto) + '</div><div>Ancho: ' + escapeHtmlTicket(widthMm + 'mm') + '</div></div>'
            + '<div class="b"><span class="k">Cliente:</span> ' + escapeHtmlTicket(data.cliente) + '</div>'
            + '<div class="b"><span class="k">Telefono:</span> ' + escapeHtmlTicket(data.telefono) + '</div>'
            + '<div class="b"><span class="k">Direccion:</span> ' + escapeHtmlTicket(data.direccion) + '</div>'
            + '<div class="sec">'
            + '<div class="b"><span class="k">Medio de pago:</span> ' + escapeHtmlTicket(data.medioPago) + '</div>'
            + (data.pagaCon ? '<div class="b"><span class="k">Paga con:</span> ' + escapeHtmlTicket(data.pagaCon) + '</div>' : '')
            + (data.vueltas ? '<div class="b"><span class="k">Vueltas:</span> ' + escapeHtmlTicket(data.vueltas) + '</div>' : '')
            + '<div class="b"><span class="k">Total:</span> ' + escapeHtmlTicket(data.total) + '</div>'
            + '</div>'
            + (data.observacion ? '<div class="sec"><div class="b"><span class="k">Observacion:</span> ' + escapeHtmlTicket(data.observacion) + '</div></div>' : '')
            + '<table><thead><tr><th style="text-align:left;">Cant</th><th style="text-align:left;">Producto</th><th style="text-align:right;">Valor</th></tr></thead><tbody>' + filas + '</tbody></table>'
            + '<div class="f">Ticket de despacho de domicilio<br/>Gracias por preferirnos</div>'
            + '</div></body></html>';

        const win = window.open('', '_blank', 'width=' + popupWidth + ',height=760');
        if (!win) return false;
        win.document.open();
        win.document.write(html);
        win.document.close();
        win.focus();
        setTimeout(function () { win.print(); }, 300);
        return false;
    };
})();
