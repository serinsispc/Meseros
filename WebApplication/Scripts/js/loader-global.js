window.LoaderGlobal = (function () {
    function getLoader() {
        return document.getElementById('globalLoader');
    }

    function getTexto() {
        return document.getElementById('globalLoaderText');
    }

    function mostrar(mensaje) {
        var loader = getLoader();
        var texto = getTexto();

        if (!loader) return;

        if (texto) {
            texto.textContent = mensaje || 'Procesando...';
        }

        loader.classList.remove('global-loader--hidden');
    }

    function ocultar() {
        var loader = getLoader();
        var texto = getTexto();

        if (!loader) return;

        loader.classList.add('global-loader--hidden');

        if (texto) {
            texto.textContent = 'Procesando...';
        }
    }

    window.addEventListener('load', function () {
        ocultar();
    });

    if (typeof Sys !== 'undefined' &&
        Sys.WebForms &&
        Sys.WebForms.PageRequestManager) {

        var prm = Sys.WebForms.PageRequestManager.getInstance();

        prm.add_beginRequest(function () {
            mostrar('Procesando...');
        });

        prm.add_endRequest(function () {
            ocultar();
        });
    }

    return {
        mostrar: mostrar,
        ocultar: ocultar
    };
})();