using DAL;
using DAL.Model;
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
    public class EmailCierreCajaHelper
    {
        private static readonly CultureInfo Co = new CultureInfo("es-CO");

        public async Task<EmailSendResult> EnviarCierreCajaAsync(string db, CierreCajaEmailData data)
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
                                mail.Subject = ConstruirAsunto(data);
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

                return list
                    .Select(x => (x.email ?? string.Empty).Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }

        private bool RequiereSsl(string encryption)
        {
            var value = (encryption ?? string.Empty).Trim().ToLowerInvariant();
            return value == "ssl" || value == "tls" || value == "starttls";
        }

        private string ConstruirAsunto(CierreCajaEmailData data)
        {
            return $"Cierre de caja {data.EstadoBase} | Turno {data.IdTurno} | {data.NombreCliente}";
        }

        private string ConstruirHtml(CierreCajaEmailData data)
        {
            var pagosInternosHtml = (data.PagosInternos != null && data.PagosInternos.Count > 0)
                ? string.Join(string.Empty, data.PagosInternos.Select(x => $@"<tr><td style='padding:10px 12px;border:1px solid #dbe7f3;background:#ffffff;'>{Html(x.nombreMPI)}</td><td style='padding:10px 12px;border:1px solid #dbe7f3;background:#ffffff;text-align:right;font-weight:700;'>{Money(x.total)}</td></tr>"))
                : "<tr><td colspan='2' style='padding:12px;border:1px solid #dbe7f3;color:#6b7280;background:#ffffff;'>Sin pagos internos reportados.</td></tr>";

            var pagosInternosResumen = (data.PagosInternos != null && data.PagosInternos.Count > 0)
                ? string.Join("", data.PagosInternos.Select(x => $@"<div style='padding:6px 0;border-bottom:1px dashed #dbe7f3;'><span style='font-weight:700;color:#334155;'>{Html(x.nombreMPI)}</span><span style='float:right;font-weight:800;color:#0f172a;'>{Money(x.total)}</span></div>"))
                : "<div style='padding:8px 0;color:#6b7280;'>Sin pagos internos reportados.</div>";

            return $@"
<!doctype html>
<html>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<title>Cierre de caja</title>
</head>
<body style='margin:0;background:#f4f7fb;font-family:Segoe UI,Arial,sans-serif;color:#10233f;'>
    <div style='max-width:920px;margin:0 auto;padding:24px 16px;'>
        <div style='background:linear-gradient(135deg,#0f766e,#2563eb);border-radius:22px;padding:28px 30px;color:#fff;box-shadow:0 18px 50px rgba(37,99,235,.18);'>
            <div style='font-size:13px;letter-spacing:.08em;text-transform:uppercase;opacity:.85;'>Reporte automatico</div>
            <div style='font-size:32px;font-weight:800;margin-top:8px;'>Cierre de Caja</div>
            <div style='font-size:15px;opacity:.92;margin-top:8px;'>{Html(data.NombreCliente)} · Turno #{data.IdTurno}</div>
            <div style='display:inline-block;margin-top:16px;padding:8px 14px;border-radius:999px;background:rgba(255,255,255,.14);font-weight:700;'>Estado: {Html(data.EstadoBase)}</div>
        </div>

        <div style='display:grid;grid-template-columns:repeat(4,1fr);gap:14px;margin-top:18px;'>
            {Card("Base inicial", Money(data.ValorBase), "#eff6ff")}
            {Card("Total ingresos", Money(data.TotalIngresos), "#ecfdf5")}
            {Card("Total egresos", Money(data.TotalEgresos), "#fff1f2")}
            {Card("Producido", Money(data.Producido), "#fff7ed")}
        </div>

        <div style='margin-top:18px;background:#fff;border-radius:20px;padding:22px;box-shadow:0 10px 30px rgba(16,35,63,.08);'>
            <div style='font-size:20px;font-weight:800;margin-bottom:14px;'>Datos del turno</div>
            <table style='width:100%;border-collapse:collapse;'>
                {Row("Cajero", data.NombreCajero, "Apertura", data.FechaApertura)}
                {Row("Cierre", data.FechaCierre, "Observacion", string.IsNullOrWhiteSpace(data.Observacion) ? "Sin observacion" : data.Observacion)}
            </table>
        </div>

        <div style='display:grid;grid-template-columns:1fr 1fr;gap:18px;margin-top:18px;'>
            <div style='background:#fff;border-radius:20px;padding:22px;box-shadow:0 10px 30px rgba(16,35,63,.08);'>
                <div style='font-size:20px;font-weight:800;margin-bottom:14px;'>Ingresos</div>
                <table style='width:100%;border-collapse:collapse;'>
                    {MoneyRow("Ventas efectivo", data.VentasEfectivo)}
                    {MoneyRow("Ventas tarjeta", data.VentasTarjeta)}
                    {MoneyRow("Ventas credito", data.VentasCredito)}
                    {MoneyRow("Total efectivo", data.TotalEfectivo)}
                    {MoneyRow("Efectivo + base", data.EfectivoMasBase)}
                    {MoneyRow("Total ingresos", data.TotalIngresos, true)}
                </table>
            </div>
            <div style='background:#fff;border-radius:20px;padding:22px;box-shadow:0 10px 30px rgba(16,35,63,.08);'>
                <div style='font-size:20px;font-weight:800;margin-bottom:14px;'>Egresos</div>
                <table style='width:100%;border-collapse:collapse;'>
                    {MoneyRow("Pago CxC efectivo", data.PagoCcEfectivo)}
                    {MoneyRow("Pago CxC tarjeta", data.PagoCcTarjeta)}
                    {MoneyRow("Pago CxP efectivo", data.PagoCpEfectivo)}
                    {MoneyRow("Gastos en efectivo", data.GastosEfectivo)}
                    {MoneyRow("Total egresos", data.TotalEgresos, true)}
                </table>
            </div>
        </div>

        <div style='margin-top:18px;background:#fff;border-radius:20px;padding:22px;box-shadow:0 10px 30px rgba(16,35,63,.08);'>
            <div style='font-size:20px;font-weight:800;margin-bottom:8px;'>Pagos internos por turno</div>
            <div style='font-size:14px;color:#475569;font-weight:700;margin-bottom:14px;'>Total: {Money(data.TotalPagosInternos)}</div>
            <div style='background:#f8fbff;border:1px solid #dbe7f3;border-radius:14px;padding:12px 14px;margin-bottom:14px;'>
                {pagosInternosResumen}
                <div style='clear:both;'></div>
            </div>
            <table style='width:100%;border-collapse:collapse;border-spacing:0;' cellpadding='0' cellspacing='0' border='0'>
                <tr>
                    <th style='text-align:left;background:#eef6ff;padding:12px;border:1px solid #dbe7f3;'>Medio de pago</th>
                    <th style='text-align:right;background:#eef6ff;padding:12px;border:1px solid #dbe7f3;'>Total</th>
                </tr>
                {pagosInternosHtml}
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
            return $"<div style='background:{bg};border-radius:18px;padding:18px 16px;'><div style='font-size:13px;color:#475569;font-weight:700;'>{Html(label)}</div><div style='margin-top:8px;font-size:28px;font-weight:800;color:#0f172a;'>{Html(value)}</div></div>";
        }

        private string Row(string l1, string v1, string l2, string v2)
        {
            return $"<tr><td style='padding:10px 0;color:#475569;font-weight:700;width:18%;'>{Html(l1)}</td><td style='padding:10px 0;width:32%;'>{Html(v1)}</td><td style='padding:10px 0;color:#475569;font-weight:700;width:18%;'>{Html(l2)}</td><td style='padding:10px 0;width:32%;'>{Html(v2)}</td></tr>";
        }

        private string MoneyRow(string label, decimal value, bool strong = false)
        {
            var weight = strong ? "800" : "700";
            var color = strong ? "#0f172a" : "#1e293b";
            return $"<tr><td style='padding:11px 0;border-bottom:1px solid #eef2f7;color:#475569;font-weight:700;'>{Html(label)}</td><td style='padding:11px 0;border-bottom:1px solid #eef2f7;text-align:right;font-weight:{weight};color:{color};'>{Money(value)}</td></tr>";
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

    public class CierreCajaEmailData
    {
        public string NombreCliente { get; set; }
        public int IdTurno { get; set; }
        public string NombreCajero { get; set; }
        public string FechaApertura { get; set; }
        public string FechaCierre { get; set; }
        public string Observacion { get; set; }
        public string EstadoBase { get; set; }
        public string FechaGeneracion { get; set; }
        public decimal ValorBase { get; set; }
        public decimal VentasEfectivo { get; set; }
        public decimal VentasTarjeta { get; set; }
        public decimal VentasCredito { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal EfectivoMasBase { get; set; }
        public decimal PagoCcEfectivo { get; set; }
        public decimal PagoCcTarjeta { get; set; }
        public decimal PagoCpEfectivo { get; set; }
        public decimal GastosEfectivo { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal Producido { get; set; }
        public decimal TotalPagosInternos { get; set; }
        public List<InformePagoInternoTurnoItem> PagosInternos { get; set; } = new List<InformePagoInternoTurnoItem>();
    }

    public class EmailSendResult
    {
        public bool Success { get; set; }
        public int Sent { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}

