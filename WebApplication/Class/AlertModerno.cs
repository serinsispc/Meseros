using System;
using System.Web;
using System.Web.UI;

namespace WebApplication.Class
{
    /// <summary>
    /// Helper para mostrar alertas (SweetAlert2) y redirigir en ASP.NET WebForms.
    /// Requiere la función JS: AlertModerno._toast(tipo, titulo, mensaje, ms, posicion, esToast)
    /// </summary>
    public static class AlertModerno
    {
        // =====================================================
        // ALERTAS BÁSICAS
        // =====================================================

        public static void Success(Page page, string titulo, string mensaje,
                                   bool esToast = false, int ms = 3000, string posicion = "center")
            => Show(page, "success", titulo, mensaje, ms, posicion, esToast);

        public static void Error(Page page, string titulo, string mensaje,
                                 bool esToast = false, int ms = 3000, string posicion = "center")
            => Show(page, "error", titulo, mensaje, ms, posicion, esToast);

        public static void Warning(Page page, string titulo, string mensaje,
                                   bool esToast = false, int ms = 3000, string posicion = "center")
            => Show(page, "warning", titulo, mensaje, ms, posicion, esToast);

        public static void Info(Page page, string titulo, string mensaje,
                                bool esToast = false, int ms = 3000, string posicion = "center")
            => Show(page, "info", titulo, mensaje, ms, posicion, esToast);

        // =====================================================
        // CONFIRMACIÓN (SweetAlert2)
        // =====================================================
        public static void Confirm(Page page, string titulo, string mensaje, string jsCallback,
                                   string posicion = "center",
                                   string textoAceptar = "Sí, continuar",
                                   string textoCancelar = "Cancelar",
                                   string icono = "warning")
        {
            string script = $@"
Swal.fire({{
  title: '{Escape(titulo)}',
  text: '{Escape(mensaje)}',
  icon: '{Escape(icono)}',
  showCancelButton: true,
  confirmButtonText: '{Escape(textoAceptar)}',
  cancelButtonText: '{Escape(textoCancelar)}',
  confirmButtonColor: '#3085d6',
  cancelButtonColor: '#d33',
  position: '{Escape(posicion)}'
}}).then((result) => {{
  if (result.isConfirmed) {{
    {jsCallback}
  }}
}});";
            Register(page, script);
        }



        public static void ConfirmDual(
    Page page,
    string titulo,
    string mensaje,
    string jsOnConfirm,           // JS a ejecutar si elige confirmar
    string jsOnDeny,              // JS a ejecutar si elige “negar”
    string textoConfirm = "Nuevo servicio",
    string textoDeny = "Amarrar a existente",
    string textoCancel = "Cancelar",
    string icono = "question",
    string posicion = "center")
        {
            string script = $@"
Swal.fire({{
  title: '{Escape(titulo)}',
  text: '{Escape(mensaje)}',
  icon: '{Escape(icono)}',
  position: '{Escape(posicion)}',
  showCancelButton: true,
  showDenyButton: true,
  confirmButtonText: '{Escape(textoConfirm)}',
  denyButtonText: '{Escape(textoDeny)}',
  cancelButtonText: '{Escape(textoCancel)}'
}}).then(function(r) {{
  if (r.isConfirmed) {{
    {jsOnConfirm}
  }} else if (r.isDenied) {{
    {jsOnDeny}
  }}
}});";
            ScriptManager.RegisterStartupScript(page, page.GetType(), Guid.NewGuid().ToString("N"), script, true);
        }


        // =====================================================
        // ALERTA + REDIRECCIÓN
        // =====================================================

        /// <summary>Muestra un toast SweetAlert y redirige al terminar el tiempo.</summary>
        public static void SuccessGoTo(Page page, string titulo, string mensaje, string url,
                                       bool esToast = false, int ms = 1200, string posicion = "center", int extraDelayMs = 50)
            => ShowAndRedirect(page, "success", titulo, mensaje, url, esToast, ms, posicion, extraDelayMs);

        public static void ErrorGoTo(Page page, string titulo, string mensaje, string url,
                                     bool esToast = false, int ms = 1500, string posicion = "center", int extraDelayMs = 50)
            => ShowAndRedirect(page, "error", titulo, mensaje, url, esToast, ms, posicion, extraDelayMs);

        public static void WarningGoTo(Page page, string titulo, string mensaje, string url,
                                       bool esToast = false, int ms = 1500, string posicion = "center", int extraDelayMs = 50)
            => ShowAndRedirect(page, "warning", titulo, mensaje, url, esToast, ms, posicion, extraDelayMs);

        public static void InfoGoTo(Page page, string titulo, string mensaje, string url,
                                    bool esToast = false, int ms = 1200, string posicion = "center", int extraDelayMs = 50)
            => ShowAndRedirect(page, "info", titulo, mensaje, url, esToast, ms, posicion, extraDelayMs);

        /// <summary>Solo redirige con un retardo (sin alerta).</summary>
        public static void GoTo(Page page, string url, int delayMs = 0)
        {
            string href = page.ResolveUrl(url ?? "~/");
            string script = delayMs > 0
                ? $"setTimeout(function(){{ window.location.href = '{href}'; }}, {delayMs});"
                : $"window.location.href = '{href}';";
            Register(page, script);
        }

        // =====================================================
        // MÉTODOS BASE
        // =====================================================

        /// <summary>Método base: invoca AlertModerno._toast(...)</summary>
        private static void Show(Page page, string tipo, string titulo, string mensaje,
                                 int ms, string posicion, bool esToast)
        {
            string script =
                $"AlertModerno._toast('{Escape(tipo)}','{Escape(titulo)}','{Escape(mensaje)}',{ms},'{Escape(posicion)}',{esToast.ToString().ToLower()});";
            Register(page, script);
        }

        /// <summary>Muestra toast y redirige pasada la duración.</summary>
        private static void ShowAndRedirect(Page page, string tipo, string titulo, string mensaje, string url,
                                            bool esToast, int ms, string posicion, int extraDelayMs)
        {
            Show(page, tipo, titulo, mensaje, ms, posicion, esToast);

            string href = page.ResolveUrl(url ?? "~/");
            int totalDelay = Math.Max(0, ms + Math.Max(0, extraDelayMs)); // seguridad
            string script = $"setTimeout(function(){{ window.location.href = '{href}'; }}, {totalDelay});";
            Register(page, script);
        }

        /// <summary>Registro encapsulado para evitar claves duplicadas.</summary>
        private static void Register(Page page, string script)
        {
            ScriptManager.RegisterStartupScript(
                page,
                page.GetType(),
                Guid.NewGuid().ToString("N"),
                script,
                true
            );
        }

        // =====================================================
        // UTILIDADES
        // =====================================================

        /// <summary>Escapa texto para insertarlo en un string JS entre comillas simples.</summary>
        private static string Escape(string s)
        {
            // JavaScriptStringEncode escapa \, " , caracteres de control, etc.
            // Luego reemplazamos ' por \'
            string enc = HttpUtility.JavaScriptStringEncode(s ?? string.Empty);
            return enc.Replace("'", "\\'");
        }
    }
}
