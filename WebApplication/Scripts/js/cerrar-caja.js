// con esta funcion llamamos el boton oculto que hace puente con el codigo C#
function EjecutarAccion(accion, argumento) {
    document.getElementById('hidAccion').value = accion || '';
    document.getElementById('hidArgumento').value = argumento || '';
    document.getElementById('btnBridge').click();
}

// con esta funcion interpretamos los argumentos o parametros
function BuildArgs(obj) {
    return Object.keys(obj)
        .map(function (k) { return k + '=' + encodeURIComponent(obj[k] ?? ''); })
        .join('|');
}

function ccParseMoney(value) {
    if (value == null) return 0;
    var cleaned = value.toString().replace(/[^0-9,-]/g, '').replace(/\./g, '').replace(',', '.');
    var parsed = parseFloat(cleaned);
    return isNaN(parsed) ? 0 : parsed;
}

function ccFormatMoney(value) {
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
}

function ccActualizarDiferencia() {
    var input = document.getElementById('txtEfectivoFisico');
    var totalEl = document.getElementById('lblEfectivoMasBase');
    var diffEl = document.getElementById('lblDiferencia');
    var badgeEl = document.getElementById('lblDiferenciaBadge');

    if (!input || !totalEl || !diffEl || !badgeEl) {
        return;
    }

    var esperado = ccParseMoney(totalEl.textContent || totalEl.innerText || '0');
    var contado = ccParseMoney(input.value || '0');
    var diferencia = contado - esperado;
    var abs = Math.abs(diferencia);
    var icon = badgeEl.querySelector('i');

    diffEl.textContent = ccFormatMoney(abs);
    badgeEl.classList.remove('ok', 'warn', 'bad');

    if (icon) {
        icon.className = 'bi';
    }

    if (abs < 1) {
        badgeEl.classList.add('ok');
        if (icon) icon.classList.add('bi-check2-circle');
    } else if (diferencia < 0) {
        badgeEl.classList.add('bad');
        if (icon) icon.classList.add('bi-arrow-down-circle');
    } else {
        badgeEl.classList.add('warn');
        if (icon) icon.classList.add('bi-arrow-up-circle');
    }
}

function ccInitCerrarCaja() {
    var btn = document.getElementById('btnConfirmarCierre');
    var txtEfectivoFisico = document.getElementById('txtEfectivoFisico');

    if (txtEfectivoFisico && !txtEfectivoFisico.dataset.ccBound) {
        txtEfectivoFisico.addEventListener('input', ccActualizarDiferencia);
        txtEfectivoFisico.addEventListener('change', ccActualizarDiferencia);
        txtEfectivoFisico.addEventListener('keyup', ccActualizarDiferencia);
        txtEfectivoFisico.dataset.ccBound = '1';
    }

    if (btn && !btn.dataset.ccBound) {
        btn.addEventListener('click', function () {
            var obs = (document.getElementById('txtObsCierre') || {}).value || '';
            EjecutarAccion('ConfirmarCierre', BuildArgs({ OBS: obs }));
        });
        btn.dataset.ccBound = '1';
    }

    ccActualizarDiferencia();
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', ccInitCerrarCaja);
} else {
    ccInitCerrarCaja();
}
