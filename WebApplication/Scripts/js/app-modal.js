// ===============================
// 🪟 MODAL GLOBAL (Bootstrap 5)
// ===============================
window.AppModal = (function () {

    function open(idModal, options) {
        const el = document.getElementById(idModal);
        if (!el) {
            console.warn("Modal no encontrado:", idModal);
            return null;
        }

        const modal = new bootstrap.Modal(el, {
            backdrop: options?.backdrop ?? 'static',
            keyboard: options?.keyboard ?? false,
            focus: true
        });

        modal.show();
        return modal;
    }

    function close(idModal) {
        const el = document.getElementById(idModal);
        if (!el) return;

        const modal = bootstrap.Modal.getInstance(el);
        if (modal) modal.hide();
    }

    return {
        open,
        close
    };

})();