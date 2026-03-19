using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WebApplication.ViewModels;

namespace WebApplication.Helpers
{
    public class WhatsAppMetaAperturaHelper
    {
        private const string TemplateName = "notificacin_de_apertura_de_caja";
        private const string TemplateLanguageCode = "es";
        private static readonly CultureInfo MoneyCulture = new CultureInfo("es-CO");

        public async Task<WhatsAppMetaSendResult> EnviarAperturaCajaAsync(string db, MenuViewModels models, AperturaCajaWhatsAppData data)
        {
            var result = new WhatsAppMetaSendResult();

            try
            {
                var config = await ObtenerConfiguracionAsync(db, models);
                if (config == null || string.IsNullOrWhiteSpace(config.accessToken) || string.IsNullOrWhiteSpace(config.phoneNumberId))
                {
                    result.Message = "No existe configuración activa de WhatsApp Meta.";
                    return result;
                }

                var destinatarios = await ObtenerDestinatariosAsync(db);
                if (destinatarios.Count == 0)
                {
                    result.Message = "No hay números activos en ListaWhatsApp.";
                    return result;
                }

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                ServicePointManager.Expect100Continue = false;

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (config.accessToken ?? string.Empty).Trim());

                    foreach (var numero in destinatarios)
                    {
                        try
                        {
                            var payload = BuildTemplatePayload(numero, models, data);
                            var json = JsonConvert.SerializeObject(payload);
                            var response = await client.PostAsync(
                                $"https://graph.facebook.com/v22.0/{(config.phoneNumberId ?? string.Empty).Trim()}/messages",
                                new StringContent(json, Encoding.UTF8, "application/json"));

                            var responseBody = await response.Content.ReadAsStringAsync();
                            if (response.IsSuccessStatusCode)
                            {
                                result.Sent++;
                            }
                            else
                            {
                                result.Errors.Add($"{numero}: {responseBody}");
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"{numero}: {ex.Message}");
                        }
                    }
                }

                result.Success = result.Sent > 0 && result.Errors.Count == 0;
                if (result.Sent > 0 && result.Errors.Count == 0)
                {
                    result.Message = $"WhatsApp enviado a {result.Sent} destinatario(s).";
                }
                else if (result.Sent > 0)
                {
                    result.Message = $"WhatsApp enviado a {result.Sent} destinatario(s). Algunos envios fallaron: {result.Errors.FirstOrDefault()}";
                }
                else
                {
                    result.Message = result.Errors.FirstOrDefault() ?? "No fue posible enviar el WhatsApp.";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        private async Task<ConfigWhatsAppMetaItem> ObtenerConfiguracionAsync(string db, MenuViewModels models)
        {
            using (var cn = new Conection_SQL(db))
            {
                var sql = "select top 1 id, accessToken, phoneNumberId, businessAccountId, nombreConfiguracion, numeroRemitente from Config_WhatsApp_Meta";
                if (int.TryParse(models?.Sede?.idWhatsAppMeta, out var idConfig) && idConfig > 0)
                {
                    sql = "select top 1 id, accessToken, phoneNumberId, businessAccountId, nombreConfiguracion, numeroRemitente from Config_WhatsApp_Meta where id = " + idConfig;
                }
                sql += " order by id desc";

                var json = await cn.EjecutarConsulta(sql, true);
                var list = string.IsNullOrWhiteSpace(json)
                    ? new List<ConfigWhatsAppMetaItem>()
                    : JsonConvert.DeserializeObject<List<ConfigWhatsAppMetaItem>>(json) ?? new List<ConfigWhatsAppMetaItem>();

                return list.FirstOrDefault();
            }
        }

        private async Task<List<string>> ObtenerDestinatariosAsync(string db)
        {
            using (var cn = new Conection_SQL(db))
            {
                var json = await cn.EjecutarConsulta("select numeroWhatsApp from ListaWhatsApp where estadoWhatsApp = 1 order by id", true);
                var list = string.IsNullOrWhiteSpace(json)
                    ? new List<ListaWhatsAppItem>()
                    : JsonConvert.DeserializeObject<List<ListaWhatsAppItem>>(json) ?? new List<ListaWhatsAppItem>();

                return list.Select(x => NormalizarNumero(x.numeroWhatsApp)).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
            }
        }

        private object BuildTemplatePayload(string numero, MenuViewModels models, AperturaCajaWhatsAppData data)
        {
            var parametros = new List<object>
            {
                TextParam("nombre_cajero", data.NombreCajero),
                TextParam("nombre_cliente", string.IsNullOrWhiteSpace(data.NombreCliente) ? (models?.Sede?.nombreSede ?? "Cliente") : data.NombreCliente),
                TextParam("fecha_apertura", data.FechaApertura),
                TextParam("hora_apertura", data.HoraApertura),
                TextParam("valor_base", FormatearMonto(data.ValorBase))
            };

            return new
            {
                messaging_product = "whatsapp",
                to = numero,
                type = "template",
                template = new
                {
                    name = TemplateName,
                    language = new { code = TemplateLanguageCode },
                    components = new[]
                    {
                        new
                        {
                            type = "body",
                            parameters = parametros.ToArray()
                        }
                    }
                }
            };
        }

        private object TextParam(string parameterName, string value)
        {
            return new
            {
                type = "text",
                parameter_name = parameterName,
                text = string.IsNullOrWhiteSpace(value) ? "-" : value
            };
        }

        private string FormatearMonto(decimal valor)
        {
            return valor.ToString("N0", MoneyCulture);
        }

        private string NormalizarNumero(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero)) return null;
            var digits = new string(numero.Where(char.IsDigit).ToArray());
            if (digits.Length == 10) return "57" + digits;
            if (digits.Length == 12 && digits.StartsWith("57")) return digits;
            return digits.Length >= 10 ? digits : null;
        }

        private class ConfigWhatsAppMetaItem
        {
            public int id { get; set; }
            public string accessToken { get; set; }
            public string phoneNumberId { get; set; }
            public string businessAccountId { get; set; }
            public string nombreConfiguracion { get; set; }
            public string numeroRemitente { get; set; }
        }

        private class ListaWhatsAppItem
        {
            public string numeroWhatsApp { get; set; }
        }
    }

    public class AperturaCajaWhatsAppData
    {
        public string NombreCajero { get; set; }
        public string NombreCliente { get; set; }
        public string FechaApertura { get; set; }
        public string HoraApertura { get; set; }
        public decimal ValorBase { get; set; }
    }
}
