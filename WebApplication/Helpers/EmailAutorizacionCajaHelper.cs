using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication.Helpers
{
    public sealed class EmailAutorizacionCajaHelper
    {
        public async Task<EmailSendResult> EnviarAsync(
            string db,
            string codigo,
            string accion,
            string producto,
            string cajero,
            string empresa,
            string tipoNotificacion)
        {
            var result = new EmailSendResult();
            try
            {
                var smtp = await ObtenerConfiguracionAsync(db);
                if (smtp == null || string.IsNullOrWhiteSpace(smtp.host) || smtp.port <= 0 || string.IsNullOrWhiteSpace(smtp.username))
                {
                    result.Message = "No existe una configuración SMTP válida.";
                    return result;
                }

                var correos = await ObtenerCorreosAsync(db);
                if (!correos.Any())
                {
                    result.Message = "No hay destinatarios en CorreosNotificaciones.";
                    return result;
                }

                using (var client = new SmtpClient(smtp.host, smtp.port))
                {
                    // frmCajaTouch siempre usa SSL y autentica con el usuario SMTP.
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(smtp.username.Trim(), smtp.password ?? string.Empty);
                    client.Timeout = 30000;

                    try
                    {
                        using (var mail = new MailMessage())
                        {
                            // El POS de escritorio usa la misma cuenta autenticada como
                            // remitente. Usar from_address distinto puede hacer que Gmail
                            // acepte SMTP pero descarte o reescriba posteriormente el correo.
                            mail.From = new MailAddress(
                                smtp.username.Trim(),
                                string.IsNullOrWhiteSpace(empresa) ? "SERINSIS POS" : empresa.Trim(),
                                Encoding.UTF8);
                            foreach (var correo in correos)
                            {
                                mail.To.Add(correo);
                            }

                            mail.Subject = accion.IndexOf("eliminar", StringComparison.OrdinalIgnoreCase) >= 0
                                ? $"Eliminar producto - {cajero}"
                                : $"Editar precio - {cajero}";
                            mail.SubjectEncoding = Encoding.UTF8;
                            mail.BodyEncoding = Encoding.UTF8;
                            mail.IsBodyHtml = true;
                            mail.Priority = MailPriority.High;
                            mail.Body = ConstruirHtml(codigo, accion, producto, cajero);
                            await client.SendMailAsync(mail);
                            result.Sent = correos.Count;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(ex.Message);
                    }
                }

                result.Success = result.Sent > 0;
                result.Message = result.Success
                    ? $"Código enviado a {result.Sent} destinatario(s)."
                    : result.Errors.FirstOrDefault() ?? "No fue posible enviar el código.";

                if (result.Success)
                {
                    var html = ConstruirHtml(codigo, accion, producto, cajero);
                    var registro = await RegistrarSolicitudesMovilesAsync(
                        db,
                        empresa,
                        tipoNotificacion,
                        html);

                    if (!registro.Success)
                    {
                        result.Success = false;
                        result.Message = $"El correo fue enviado, pero no se registró la solicitud en DBNotificacionesMovil: {registro.Message}";
                        return result;
                    }

                    result.Message += $" Solicitud registrada para {registro.Inserted} supervisor(es) en DBNotificacionesMovil.";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        private async Task<NotificationRegistrationResult> RegistrarSolicitudesMovilesAsync(
            string db,
            string empresa,
            string tipoNotificacion,
            string mensaje)
        {
            var result = new NotificationRegistrationResult();
            try
            {
                var numeros = await ObtenerNumerosWhatsAppAsync(db);
                if (!numeros.Any())
                {
                    result.Message = "No hay supervisores activos en V_ListaWhatsApp.";
                    return result;
                }

                foreach (var numero in numeros)
                {
                    await InsertarNotificacionMovilAsync(
                        numero,
                        db,
                        empresa,
                        tipoNotificacion,
                        mensaje);
                    result.Inserted++;
                }

                result.Success = result.Inserted == numeros.Count;
                result.Message = result.Success
                    ? $"Se registraron {result.Inserted} solicitud(es)."
                    : "No se registraron todas las solicitudes.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        private async Task InsertarNotificacionMovilAsync(
            string celular,
            string db,
            string empresa,
            string tipoNotificacion,
            string mensaje)
        {
            const string sql = @"
insert into dbo.NotificacionesMovil
    (fecha, celular, db, representateLegal, establecimiento, tipo, mensaje, estado)
values
    (@fecha, @celular, @db, @representateLegal, @establecimiento, @tipo, @mensaje, @estado);";

            using (var cn = new SqlConnection(RuntimeSettings.BuildSqlConnectionString("DBNotificacionesMovil")))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@fecha", SqlDbType.DateTime).Value = DateTime.Now;
                cmd.Parameters.Add("@celular", SqlDbType.VarChar, 50).Value = celular;
                cmd.Parameters.Add("@db", SqlDbType.VarChar, 255).Value = db ?? string.Empty;
                cmd.Parameters.Add("@representateLegal", SqlDbType.NVarChar, 500).Value = empresa ?? string.Empty;
                cmd.Parameters.Add("@establecimiento", SqlDbType.NVarChar, 500).Value = empresa ?? string.Empty;
                cmd.Parameters.Add("@tipo", SqlDbType.NVarChar, 500).Value = tipoNotificacion ?? string.Empty;
                cmd.Parameters.Add("@mensaje", SqlDbType.NVarChar, -1).Value = mensaje ?? string.Empty;
                cmd.Parameters.Add("@estado", SqlDbType.Bit).Value = true;

                await cn.OpenAsync();
                var filas = await cmd.ExecuteNonQueryAsync();
                if (filas != 1)
                {
                    throw new InvalidOperationException("La base de notificaciones no confirmó la inserción.");
                }
            }
        }

        private async Task<List<string>> ObtenerNumerosWhatsAppAsync(string db)
        {
            using (var cn = new Conection_SQL(db))
            {
                var json = await cn.EjecutarConsulta(
                    "select numeroWhatsApp from V_ListaWhatsApp where estadoWhatsApp = 1",
                    true);
                var lista = string.IsNullOrWhiteSpace(json)
                    ? new List<WhatsAppItem>()
                    : JsonConvert.DeserializeObject<List<WhatsAppItem>>(json) ?? new List<WhatsAppItem>();
                return lista.Select(x => (x.numeroWhatsApp ?? string.Empty).Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }

        private async Task<ConfiguracionSmtpItem> ObtenerConfiguracionAsync(string db)
        {
            using (var cn = new Conection_SQL(db))
            {
                var json = await cn.EjecutarConsulta("select top 1 id, host, port, username, password, encryption, from_address, from_name from ConfiguracionSMTP order by id desc", true);
                return string.IsNullOrWhiteSpace(json)
                    ? null
                    : (JsonConvert.DeserializeObject<List<ConfiguracionSmtpItem>>(json) ?? new List<ConfiguracionSmtpItem>()).FirstOrDefault();
            }
        }

        private async Task<List<string>> ObtenerCorreosAsync(string db)
        {
            using (var cn = new Conection_SQL(db))
            {
                var json = await cn.EjecutarConsulta("select email from CorreosNotificaciones order by id", true);
                var lista = string.IsNullOrWhiteSpace(json)
                    ? new List<CorreoItem>()
                    : JsonConvert.DeserializeObject<List<CorreoItem>>(json) ?? new List<CorreoItem>();
                return lista.Select(x => (x.email ?? string.Empty).Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }
        }

        private static bool RequiereSsl(string encryption)
        {
            var value = (encryption ?? string.Empty).Trim().ToLowerInvariant();
            return value == "ssl" || value == "tls" || value == "starttls";
        }

        private static string ConstruirHtml(string codigo, string accion, string producto, string cajero)
        {
            var usuarioSeguro = Web(cajero);
            var accionSegura = Web(accion);
            var codigoSeguro = Web(codigo);
            var codigoUrl = System.Web.HttpUtility.UrlEncode(codigo ?? string.Empty);

            return $@"<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Código de Verificación</title>
    <style>
        body {{ margin:0;padding:0;background-color:#e8f4fc;font-family:Arial,sans-serif; }}
        .container {{ max-width:600px;margin:20px auto;background-color:#fff;padding:0;border-radius:8px;box-shadow:0 4px 8px rgba(0,0,0,.1);text-align:center; }}
        .header {{ background-color:#007bb5;color:#fff;padding:15px;border-radius:8px 8px 0 0; }}
        .header h1 {{ margin:0;font-size:20px; }}
        .content {{ padding:25px;color:#333; }}
        .content p {{ font-size:16px;margin:12px 0; }}
        .accion {{ font-weight:bold;color:#007bb5; }}
        .codigo {{ margin:20px auto;padding:15px;font-size:28px;font-weight:bold;letter-spacing:6px;color:#007bb5;background-color:#f1f8fd;border:2px dashed #007bb5;width:fit-content;border-radius:6px; }}
        .nota {{ font-size:14px;color:#666;margin-top:20px; }}
        .footer {{ margin:20px;font-size:12px;color:#888; }}
        .btn-aprobar {{ display:inline-block;margin:10px auto 0;padding:12px 18px;background:#16a34a;color:#fff !important;text-decoration:none;border-radius:8px;font-weight:700;font-size:16px; }}
        .btn-aprobar:hover {{ filter:brightness(.95); }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'><h1>🔐 Código de Verificación</h1></div>
        <div class='content'>
            <p>Hola</p>
            <p>El cajero <strong>{usuarioSeguro}</strong>,</p>
            <p>Estás intentando realizar la siguiente acción:</p>
            <p class='accion'>{accionSegura}</p>
            <p>Utiliza el siguiente código para continuar:</p>
            <div class='codigo'>{codigoSeguro}</div>
            <a class='btn-aprobar' href='https://www.serinsispc.com/ApiPOS/api/NotificacionesMovil/AprobarLink?codigo={codigoUrl}' target='_blank' rel='noopener'>✅ Aprobar</a>
            <p class='nota'>Este código es confidencial y tiene una vigencia limitada.<br>No lo compartas con nadie.</p>
        </div>
        <div class='footer'><p>&copy; 2025 SERINSIS PC S.A.S. Todos los derechos reservados.</p></div>
    </div>
</body>
</html>";
        }

        private static string Web(string value) => System.Web.HttpUtility.HtmlEncode(value ?? string.Empty);

        private sealed class ConfiguracionSmtpItem
        {
            public string host { get; set; }
            public int port { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string encryption { get; set; }
            public string from_address { get; set; }
            public string from_name { get; set; }
        }

        private sealed class CorreoItem { public string email { get; set; } }
        private sealed class WhatsAppItem { public string numeroWhatsApp { get; set; } }
        private sealed class NotificationRegistrationResult
        {
            public bool Success { get; set; }
            public int Inserted { get; set; }
            public string Message { get; set; }
        }
    }
}
