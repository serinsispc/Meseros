using DAL.Controler;
using DAL.Model;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;

namespace WebApplication.Helpers
{
    public static class AdminControlAccessHelper
    {
        public static readonly TimeSpan ReminderInterval = TimeSpan.FromHours(2);

        public static async Task<bool> BloquearIngresoSiSuspendidoAsync(Page page, string db)
        {
            var adminControl = await AdminControlControler.ConsultarAsync(db);
            if (adminControl == null)
            {
                return false;
            }

            if (adminControl.tipo_admincontrol != 2)
            {
                if (adminControl.tipo_admincontrol == 0)
                {
                    LimpiarRecordatorio(page?.Session);
                }

                return false;
            }

            LimpiarRecordatorio(page?.Session);
            RegistrarBloqueo(page, adminControl, redirigirASalir: false);
            return true;
        }

        public static async Task<bool> MostrarRecordatorioIngresoSiCorrespondeAsync(Page page, string db, string redirectUrl = null, string onCloseScript = null)
        {
            var adminControl = await AdminControlControler.ConsultarAsync(db);
            if (adminControl == null)
            {
                return false;
            }

            if (adminControl.tipo_admincontrol == 0)
            {
                LimpiarRecordatorio(page?.Session);
                return false;
            }

            if (adminControl.tipo_admincontrol != 1)
            {
                return false;
            }

            MarcarRecordatorio(page?.Session);

            var script = ConstruirScriptAlerta(
                "warning",
                "Recordatorio de pago",
                adminControl.mensaje_admincontrol,
                esBloqueante: false,
                redirigirASalir: false,
                redirectUrl: redirectUrl,
                onCloseScript: onCloseScript);

            RegistrarScript(page, "admincontrol_recordatorio_ingreso", script);
            return true;
        }

        public static async Task AplicarControlAsync(Page page)
        {
            if (page?.Session == null)
            {
                return;
            }

            var db = Convert.ToString(page.Session[SessionContextHelper.DbKey]);
            if (string.IsNullOrWhiteSpace(db))
            {
                return;
            }

            var adminControl = await AdminControlControler.ConsultarAsync(db);
            if (adminControl == null)
            {
                return;
            }

            switch (adminControl.tipo_admincontrol)
            {
                case 0:
                    LimpiarRecordatorio(page.Session);
                    break;
                case 1:
                    RegistrarRecordatorioSiCorresponde(page, adminControl);
                    break;
                case 2:
                    LimpiarRecordatorio(page.Session);
                    RegistrarBloqueo(page, adminControl, redirigirASalir: true);
                    break;
            }
        }

        private static void RegistrarRecordatorioSiCorresponde(Page page, AdminControl adminControl)
        {
            var session = page.Session;
            var ahoraUtc = DateTime.UtcNow;
            var ultimoRecordatorio = session[SessionContextHelper.AdminControlReminderAtKey] as string;

            if (DateTime.TryParse(ultimoRecordatorio, null, System.Globalization.DateTimeStyles.RoundtripKind, out var ultimaFechaUtc))
            {
                var transcurrido = ahoraUtc - ultimaFechaUtc;
                if (transcurrido < ReminderInterval)
                {
                    return;
                }
            }

            MarcarRecordatorio(session);
            RegistrarScript(
                page,
                "admincontrol_recordatorio",
                ConstruirScriptAlerta(
                    "warning",
                    "Recordatorio de pago",
                    adminControl.mensaje_admincontrol,
                    esBloqueante: false,
                    redirigirASalir: false));
        }

        private static void RegistrarBloqueo(Page page, AdminControl adminControl, bool redirigirASalir)
        {
            var script = ConstruirScriptAlerta(
                "error",
                "Servicio suspendido",
                adminControl.mensaje_admincontrol,
                esBloqueante: true,
                redirigirASalir);

            RegistrarScript(page, "admincontrol_bloqueo", script);
        }

        private static string ConstruirScriptAlerta(string icono, string titulo, string mensaje, bool esBloqueante, bool redirigirASalir, string redirectUrl = null, string onCloseScript = null)
        {
            var tituloSeguro = HttpUtility.JavaScriptStringEncode(titulo ?? string.Empty).Replace("'", "\\'");
            var mensajeSeguro = HttpUtility.JavaScriptStringEncode((mensaje ?? string.Empty).Replace("\r\n", "\n")).Replace("'", "\\'");
            var destino = string.Empty;
            if (redirigirASalir)
            {
                destino = $"window.location.href = '{PageSafeUrl("~/Salir.aspx")}';";
            }
            else if (!string.IsNullOrWhiteSpace(redirectUrl))
            {
                destino = $"window.location.href = '{PageSafeUrl(redirectUrl)}';";
            }

            var accionPosterior = string.IsNullOrWhiteSpace(onCloseScript)
                ? destino
                : onCloseScript + Environment.NewLine + destino;
            var permitirCerrar = esBloqueante ? "false" : "true";
            var escape = esBloqueante ? "false" : "true";

            return $@"
Swal.fire({{
  icon: '{icono}',
  title: '{tituloSeguro}',
  html: '<div style=""white-space:pre-line;text-align:left;"">{mensajeSeguro}</div>',
  confirmButtonText: 'Entendido',
  allowOutsideClick: {permitirCerrar},
  allowEscapeKey: {escape}
}}).then(function() {{
  {accionPosterior}
}});";
        }

        private static void RegistrarScript(Page page, string key, string script)
        {
            if (page == null)
            {
                return;
            }

            if (ScriptManager.GetCurrent(page) != null)
            {
                ScriptManager.RegisterStartupScript(page, page.GetType(), key, script, true);
                return;
            }

            page.ClientScript.RegisterStartupScript(page.GetType(), key, script, true);
        }

        private static string PageSafeUrl(string url)
        {
            return VirtualPathUtility.ToAbsolute(url ?? "~/");
        }

        private static void MarcarRecordatorio(HttpSessionState session)
        {
            if (session == null)
            {
                return;
            }

            session[SessionContextHelper.AdminControlReminderAtKey] = DateTime.UtcNow.ToString("o");
        }

        private static void LimpiarRecordatorio(HttpSessionState session)
        {
            if (session == null)
            {
                return;
            }

            session.Remove(SessionContextHelper.AdminControlReminderAtKey);
        }
    }
}
