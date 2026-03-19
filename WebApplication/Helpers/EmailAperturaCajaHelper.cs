using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication.Helpers
{
    public class EmailAperturaCajaHelper
    {
        private static readonly CultureInfo Co = new CultureInfo("es-CO");

        public async Task<EmailSendResult> EnviarAperturaCajaAsync(string db, AperturaCajaEmailData data)
        {
            var result = new EmailSendResult();

            try
            {
                var smtp = await ObtenerConfiguracionAsync(db);
                if (smtp == null || string.IsNullOrWhiteSpace(smtp.host) || smtp.port <= 0 || string.IsNullOrWhiteSpace(smtp.from_address))
                {
                    result.Message = "No existe configuracion SMTP valida.";
                    return result;
                }

                var correos = await ObtenerCorreosAsync(db);
                if (correos.Count == 0)
                {
                    result.Message = "No hay correos en CorreosNotificaciones.";
                    return result;
                }

                using (var client = new SmtpClient(smtp.host, smtp.port))
                {
                    client.EnableSsl = RequiereSsl(smtp.encryption);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = string.IsNullOrWhiteSpace(smtp.username)
                        ? CredentialCache.DefaultNetworkCredentials
                        : new NetworkCredential(smtp.username, smtp.password ?? string.Empty);
                    client.Timeout = 30000;

                    foreach (var correo in correos)
                    {
                        try
                        {
                            using (var mail = new MailMessage())
                            {
                                mail.From = new MailAddress(smtp.from_address.Trim(), string.IsNullOrWhiteSpace(smtp.from_name) ? "Sistema" : smtp.from_name.Trim());
                                mail.To.Add(correo);
                                mail.Subject = $"Apertura de caja | Turno {data.IdTurno} | {data.NombreCliente}";
                                mail.SubjectEncoding = Encoding.UTF8;
                                mail.BodyEncoding = Encoding.UTF8;
                                mail.IsBodyHtml = true;
                                mail.Body = ConstruirHtml(data);
                                await client.SendMailAsync(mail);
                                result.Sent++;
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(correo + ": " + ex.Message);
                        }
                    }
                }

                result.Success = result.Sent > 0 && result.Errors.Count == 0;
                if (result.Sent > 0 && result.Errors.Count == 0)
                {
                    result.Message = $"Correo enviado a {result.Sent} destinatario(s).";
                }
                else if (result.Sent > 0)
                {
                    result.Message = $"Correo enviado a {result.Sent} destinatario(s). Algunos envios fallaron: {result.Errors.FirstOrDefault()}";
                }
                else
                {
                    result.Message = result.Errors.FirstOrDefault() ?? "No fue posible enviar el correo.";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        private async Task<ConfiguracionSmtpItem> ObtenerConfiguracionAsync(string db)
        {
            using (var cn = new Conection_SQL(db))
            {
                var sql = "select top 1 id, host, port, username, password, encryption, from_address, from_name from ConfiguracionSMTP order by id desc";
                var json = await cn.EjecutarConsulta(sql, true);
                var list = string.IsNullOrWhiteSpace(json)
                    ? new List<ConfiguracionSmtpItem>()
                    : JsonConvert.DeserializeObject<List<ConfiguracionSmtpItem>>(json) ?? new List<ConfiguracionSmtpItem>();
                return list.FirstOrDefault();
            }
        }

        private async Task<List<string>> ObtenerCorreosAsync(string db)
        {
            using (var cn = new Conection_SQL(db))
            {
                var json = await cn.EjecutarConsulta("select email from CorreosNotificaciones order by id", true);
                var list = string.IsNullOrWhiteSpace(json)
                    ? new List<CorreoNotificacionItem>()
                    : JsonConvert.DeserializeObject<List<CorreoNotificacionItem>>(json) ?? new List<CorreoNotificacionItem>();

                return list.Select(x => (x.email ?? string.Empty).Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }
        }

        private bool RequiereSsl(string encryption)
        {
            var value = (encryption ?? string.Empty).Trim().ToLowerInvariant();
            return value == "ssl" || value == "tls" || value == "starttls";
        }

        private string ConstruirHtml(AperturaCajaEmailData data)
        {
            return $@"<!doctype html>
<html>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<title>Apertura de caja</title>
</head>
<body style='margin:0;background:#f4f7fb;font-family:Segoe UI,Arial,sans-serif;color:#10233f;'>
<div style='max-width:760px;margin:0 auto;padding:24px 16px;'>
    <div style='background:linear-gradient(135deg,#2563eb,#0f766e);border-radius:22px;padding:28px 30px;color:#fff;box-shadow:0 18px 50px rgba(37,99,235,.18);'>
        <div style='font-size:13px;letter-spacing:.08em;text-transform:uppercase;opacity:.85;'>Notificacion automatica</div>
        <div style='font-size:32px;font-weight:800;margin-top:8px;'>Apertura de Caja</div>
        <div style='font-size:15px;opacity:.92;margin-top:8px;'>{Html(data.NombreCliente)} · Turno #{data.IdTurno}</div>
        <div style='display:inline-block;margin-top:16px;padding:8px 14px;border-radius:999px;background:rgba(255,255,255,.14);font-weight:700;'>Estado: ACTIVA</div>
    </div>

    <div style='display:grid;grid-template-columns:repeat(3,1fr);gap:14px;margin-top:18px;'>
        {Card("Cajero", data.NombreCajero, "#eef6ff")}
        {Card("Fecha", data.FechaApertura, "#ecfdf5")}
        {Card("Hora", data.HoraApertura, "#fff7ed")}
    </div>

    <div style='margin-top:18px;background:#fff;border-radius:20px;padding:22px;box-shadow:0 10px 30px rgba(16,35,63,.08);'>
        <div style='font-size:20px;font-weight:800;margin-bottom:16px;'>Resumen de apertura</div>
        <table style='width:100%;border-collapse:collapse;'>
            {Row("Cliente", data.NombreCliente)}
            {Row("Cajero", data.NombreCajero)}
            {Row("Fecha de apertura", data.FechaApertura)}
            {Row("Hora de apertura", data.HoraApertura)}
            {MoneyRow("Valor base", data.ValorBase)}
        </table>
    </div>

    <div style='margin-top:18px;background:#0f172a;color:#dbe7ff;border-radius:20px;padding:18px 22px;'>
        <div style='font-size:12px;text-transform:uppercase;letter-spacing:.08em;opacity:.75;'>Generado por el sistema</div>
        <div style='margin-top:8px;font-size:14px;'>Fecha de envio: {Html(data.FechaGeneracion)}</div>
    </div>
</div>
</body>
</html>";
        }

        private string Card(string label, string value, string bg)
        {
            return $"<div style='background:{bg};border-radius:18px;padding:18px 16px;'><div style='font-size:13px;color:#475569;font-weight:700;'>{Html(label)}</div><div style='margin-top:8px;font-size:24px;font-weight:800;color:#0f172a;'>{Html(value)}</div></div>";
        }

        private string Row(string label, string value)
        {
            return $"<tr><td style='padding:12px 0;border-bottom:1px solid #eef2f7;color:#475569;font-weight:700;width:36%;'>{Html(label)}</td><td style='padding:12px 0;border-bottom:1px solid #eef2f7;'>{Html(value)}</td></tr>";
        }

        private string MoneyRow(string label, decimal value)
        {
            return $"<tr><td style='padding:12px 0;border-bottom:1px solid #eef2f7;color:#475569;font-weight:700;'>{Html(label)}</td><td style='padding:12px 0;border-bottom:1px solid #eef2f7;font-weight:800;text-align:right;'>{Money(value)}</td></tr>";
        }

        private string Money(decimal value)
        {
            return value.ToString("C0", Co);
        }

        private string Html(string value)
        {
            return System.Web.HttpUtility.HtmlEncode(value ?? string.Empty);
        }

        private class ConfiguracionSmtpItem
        {
            public int id { get; set; }
            public string host { get; set; }
            public int port { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string encryption { get; set; }
            public string from_address { get; set; }
            public string from_name { get; set; }
        }

        private class CorreoNotificacionItem
        {
            public string email { get; set; }
        }
    }

    public class AperturaCajaEmailData
    {
        public int IdTurno { get; set; }
        public string NombreCajero { get; set; }
        public string NombreCliente { get; set; }
        public string FechaApertura { get; set; }
        public string HoraApertura { get; set; }
        public decimal ValorBase { get; set; }
        public string FechaGeneracion { get; set; }
    }
}
