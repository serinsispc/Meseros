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
        var btnGuardarBase = document.getElementById('btnGuardarBase');
        var btnEditarBase = document.getElementById('btnEditarBase');
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

        ccActualizarDiferencia();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', ccInitCerrarCaja);
    } else {
        ccInitCerrarCaja();
    }
})();
