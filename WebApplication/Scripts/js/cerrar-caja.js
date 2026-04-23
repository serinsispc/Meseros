(function () {
    var busy = false;

    function ccEnsureLoadingApi() {
        if (window.SerinsisLoading && typeof window.SerinsisLoading.show === 'function') {
            return;
        }

        var overlay = document.getElementById('appLoading');
        window.SerinsisLoading = {
            show: function () {
                if (overlay) {
                    overlay.style.display = 'flex';
                }
            },
            hide: function () {
                if (overlay) {
                    overlay.style.display = 'none';
                }
            }
        };
    }

    function ccPrepareTicketWindow() {
        if (window.ccMostrarCierreCaja !== true) {
            return;
        }

        try {
            sessionStorage.setItem('cc_print_after_close', '1');
        } catch (e) {
        }

        var win = window.open('', 'cierreCajaTicket', 'width=380,height=760');
        if (win && !win.closed) {
            try {
                win.document.open();
                win.document.write('<!doctype html><html><head><title>Preparando impresion</title></head><body style="font-family:Consolas,monospace;padding:16px;">Preparando ticket...</body></html>');
                win.document.close();
            } catch (e) {
            }
        }
    }

    window.EjecutarAccion = function (accion, argumento, btn) {
        if (busy) return;
        busy = true;

        var hidAccion = document.getElementById('hidAccion');
        var hidArgumento = document.getElementById('hidArgumento');
        var btnBridge = document.getElementById('btnBridge');

        if (!hidAccion || !hidArgumento || !btnBridge) {
            busy = false;
            return;
        }

        hidAccion.value = accion || '';
        hidArgumento.value = argumento || '';

        ccEnsureLoadingApi();
        if (window.SerinsisLoading) {
            window.SerinsisLoading.show();
        }

        if (btn) {
            try {
                btn.disabled = true;
                btn.classList.add('disabled');
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
            .map(function (k) { return k + '=' + encodeURIComponent(obj[k] ?? ''); })
            .join('|');
    };

    window.ccParseMoney = function (value) {
        if (value == null) return 0;
        var cleaned = value.toString().replace(/[^0-9,-]/g, '').replace(/\./g, '').replace(',', '.');
        var parsed = parseFloat(cleaned);
        return isNaN(parsed) ? 0 : parsed;
    };

    window.ccFormatMoney = function (value) {
        var number = Number(value || 0);
        try {
            return number.toLocaleString('es-CO', {
                style: 'currency',
                currency: 'COP',
                maximumFractionDigits: 0
            });
        } catch (e) {
            return '$ ' + Math.round(number);
        }
    };

    function ccGetProductosTurno() {
        var field = document.getElementById('hidProductosVendidosJson');
        if (!field || !field.value) {
            return [];
        }

        try {
            var data = JSON.parse(field.value);
            return Array.isArray(data) ? data : [];
        } catch (e) {
            return [];
        }
    }

    function ccFormatCantidad(value) {
        var number = Number(value || 0);
        if (!isFinite(number)) return '0';
        if (Math.floor(number) === number) return number.toLocaleString('es-CO');
        return number.toLocaleString('es-CO', { minimumFractionDigits: 0, maximumFractionDigits: 2 });
    }

    function ccRenderProductosTurno() {
        var resumen = document.getElementById('ccResumenProductosTurno');
        var detalle = document.getElementById('ccDetalleProductosTurno');
        if (!resumen || !detalle) {
            return;
        }

        var items = ccGetProductosTurno();
        if (!items.length) {
            resumen.innerHTML = '';
            detalle.innerHTML = '<div class="cc-note"><i class="bi bi-info-circle me-2"></i>No hay productos vendidos registrados para este turno.</div>';
            return;
        }

        var categorias = {};
        items.forEach(function (item) {
            var nombreCategoria = (item.nombreCategoria || 'Sin categoría').trim() || 'Sin categoría';
            if (!categorias[nombreCategoria]) {
                categorias[nombreCategoria] = {
                    nombre: nombreCategoria,
                    cantidad: 0,
                    valor: 0,
                    items: []
                };
            }

            categorias[nombreCategoria].cantidad += Number(item.cantidad || 0);
            categorias[nombreCategoria].valor += Number(item.valor || 0);
            categorias[nombreCategoria].items.push(item);
        });

        var listaCategorias = Object.keys(categorias).sort().map(function (key) { return categorias[key]; });
        var totalCantidad = items.reduce(function (acc, item) { return acc + Number(item.cantidad || 0); }, 0);
        var totalValor = items.reduce(function (acc, item) { return acc + Number(item.valor || 0); }, 0);

        resumen.innerHTML = [
            '<div class="col-12 col-md-4"><div class="cc-kpi"><div class="top"><div class="label">Categorías</div><div class="ico"><i class="bi bi-tags"></i></div></div><div class="value">' + listaCategorias.length + '</div><div class="text-muted small fw-semibold">grupos del turno</div></div></div>',
            '<div class="col-12 col-md-4"><div class="cc-kpi success"><div class="top"><div class="label">Cantidad vendida</div><div class="ico"><i class="bi bi-123"></i></div></div><div class="value">' + ccFormatCantidad(totalCantidad) + '</div><div class="text-muted small fw-semibold">unidades del turno</div></div></div>',
            '<div class="col-12 col-md-4"><div class="cc-kpi warning"><div class="top"><div class="label">Valor vendido</div><div class="ico"><i class="bi bi-currency-dollar"></i></div></div><div class="value">' + ccFormatMoney(totalValor) + '</div><div class="text-muted small fw-semibold">total productos</div></div></div>'
        ].join('');

        detalle.innerHTML = listaCategorias.map(function (categoria) {
            var filas = categoria.items.map(function (item) {
                return '<tr>' +
                    '<td>' + (item.codigoProducto || 'ITEM') + '</td>' +
                    '<td>' + (item.nombreProducto || 'Producto') + '</td>' +
                    '<td class="money">' + ccFormatCantidad(item.cantidad || 0) + '</td>' +
                    '<td class="money">' + ccFormatMoney(item.valor || 0) + '</td>' +
                    '</tr>';
            }).join('');

            return '<div class="cc-card mb-3">' +
                '<div class="cc-card-h">' +
                '<h4 class="cc-card-t"><i class="bi bi-tag"></i>' + categoria.nombre + '</h4>' +
                '<span class="cc-pill"><i class="bi bi-bar-chart"></i>' + ccFormatCantidad(categoria.cantidad) + ' und · ' + ccFormatMoney(categoria.valor) + '</span>' +
                '</div>' +
                '<div class="cc-card-b">' +
                '<div class="cc-report-wrap">' +
                '<table class="cc-report-table">' +
                '<thead><tr><th>Código</th><th>Producto</th><th>Cantidad</th><th>Valor</th></tr></thead>' +
                '<tbody>' + filas + '</tbody>' +
                '</table>' +
                '</div>' +
                '</div>' +
                '</div>';
        }).join('');
    }

    function ccAbrirProductosTurno() {
        ccRenderProductosTurno();
        var modalEl = document.getElementById('mdlProductosTurno');
        if (!modalEl || !window.bootstrap || !bootstrap.Modal) {
            return;
        }

        var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modal.show();
    }

    window.ccActualizarDiferencia = function () {
        var input = document.getElementById('txtEfectivoFisico');
        var totalEl = document.getElementById('lblEfectivoMasBase');
        var diffEl = document.getElementById('lblDiferencia');
        var badgeEl = document.getElementById('lblDiferenciaBadge');

        if (!input || !totalEl || !diffEl || !badgeEl) {
            return;
        }

        var esperado = ccParseMoney(totalEl.textContent || totalEl.innerText || '0');
        var contado = ccParseMoney(input.value || '0');
        var diferencia = esperado - contado;
        var icon = badgeEl.querySelector('i');
        var diferenciaTexto = ccFormatMoney(Math.abs(diferencia));

        if (diferencia < 0) {
            diferenciaTexto = '- ' + diferenciaTexto;
        }

        diffEl.textContent = diferenciaTexto;
        badgeEl.classList.remove('ok', 'warn', 'bad');

        if (icon) {
            icon.className = 'bi';
        }

        if (Math.abs(diferencia) < 1) {
            badgeEl.classList.add('ok');
            if (icon) icon.classList.add('bi-check2-circle');
        } else if (diferencia > 0) {
            badgeEl.classList.add('bad');
            if (icon) icon.classList.add('bi-arrow-down-circle');
        } else {
            badgeEl.classList.add('warn');
            if (icon) icon.classList.add('bi-arrow-up-circle');
        }
    };

    function ccInitCerrarCaja() {
        var btn = document.getElementById('btnConfirmarCierre');
        var btnAperturarCajon = document.getElementById('btnAperturarCajon');
        var btnGuardarBase = document.getElementById('btnGuardarBase');
        var btnEditarBase = document.getElementById('btnEditarBase');
        var btnVerProductosTurno = document.getElementById('btnVerProductosTurno');
        var txtEfectivoFisico = document.getElementById('txtEfectivoFisico');
        var txtValorBaseEditar = document.getElementById('txtValorBaseEditar');

        ccEnsureLoadingApi();
        if (window.SerinsisLoading) {
            window.SerinsisLoading.hide();
        }

        if (txtEfectivoFisico && !txtEfectivoFisico.dataset.ccBound) {
            txtEfectivoFisico.addEventListener('input', ccActualizarDiferencia);
            txtEfectivoFisico.addEventListener('change', ccActualizarDiferencia);
            txtEfectivoFisico.addEventListener('keyup', ccActualizarDiferencia);
            txtEfectivoFisico.dataset.ccBound = '1';
        }

        if (btn && !btn.dataset.ccBound) {
            btn.addEventListener('click', function () {
                var obs = (document.getElementById('txtObsCierre') || {}).value || '';
                ccPrepareTicketWindow();
                EjecutarAccion('ConfirmarCierre', BuildArgs({ OBS: obs }));
            });
            btn.dataset.ccBound = '1';
        }

        if (btnEditarBase && !btnEditarBase.dataset.ccBound) {
            btnEditarBase.addEventListener('click', function () {
                if (!txtValorBaseEditar) {
                    return;
                }

                var valorActual = ccParseMoney((document.getElementById('lblValorBase') || {}).textContent || '0');
                txtValorBaseEditar.value = valorActual > 0 ? Math.round(valorActual).toString() : '';
                txtValorBaseEditar.focus();
                txtValorBaseEditar.select();
            });
            btnEditarBase.dataset.ccBound = '1';
        }

        if (btnGuardarBase && !btnGuardarBase.dataset.ccBound) {
            btnGuardarBase.addEventListener('click', function () {
                var valorBase = (txtValorBaseEditar || {}).value || '';
                EjecutarAccion('ActualizarBase', BuildArgs({ VALOR_BASE: valorBase }));
            });
            btnGuardarBase.dataset.ccBound = '1';
        }

        if (btnAperturarCajon && !btnAperturarCajon.dataset.ccBound) {
            btnAperturarCajon.addEventListener('click', function () {
                EjecutarAccion('AperturarCajon', '', btnAperturarCajon);
            });
            btnAperturarCajon.dataset.ccBound = '1';
        }

        if (btnVerProductosTurno && !btnVerProductosTurno.dataset.ccBound) {
            btnVerProductosTurno.addEventListener('click', function () {
                ccAbrirProductosTurno();
            });
            btnVerProductosTurno.dataset.ccBound = '1';
        }

        ccActualizarDiferencia();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', ccInitCerrarCaja);
    } else {
        ccInitCerrarCaja();
    }
})();
