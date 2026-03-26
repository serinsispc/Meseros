(function () {
    'use strict';

    function postback(eventName, data) {
        if (typeof window.__doPostBack !== 'function') {
            console.error('__doPostBack no disponible');
            return;
        }

        var payload = '';

        if (typeof data === 'string') {
            payload = data;
        } else if (data !== undefined && data !== null) {
            try {
                payload = JSON.stringify(data);
            } catch (e) {
                console.error('Error serializando payload', e);
                payload = '';
            }
        }

        window.__doPostBack(eventName, payload);
    }

    window.hvAction = {
        abrirVenta: function (idVenta) {
            postback('btnAbrirVenta', String(idVenta || 0));
        },

        imprimirVenta: function (idVenta) {
            postback('btnImprimirVenta', String(idVenta || 0));
        },

        editarResolucion: function (idVenta) {
            postback('btnEditarResolucion', String(idVenta || 0));
        },

        guardarResolucion: function (payload) {
            postback('btnGuardarResolucion', payload);
        },

        editarCliente: function (idVenta) {
            postback('btnEditarCliente', String(idVenta || 0));
        },

        buscarNIT: function (nit) {
            postback('btnBuscarNIT', (nit || '').trim());
        },

        seleccionarCliente: function (payload) {
            postback('btnSeleccionarCliente', payload);
        },

        guardarCliente: function (payload) {
            postback('btnGuardarCliente', payload);
        },

        enviarDIAN: function (idVenta) {
            postback('btnEnviarDIAN', String(idVenta || 0));
        },

        descargarPDF: function (idVenta) {
            postback('btnDescargarPDF', String(idVenta || 0));
        }
    };
})();
