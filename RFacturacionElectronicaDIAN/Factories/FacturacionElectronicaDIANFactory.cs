using Newtonsoft.Json;
using RFacturacionElectronicaDIAN.Entities.Request;
using RFacturacionElectronicaDIAN.Entities.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RFacturacionElectronicaDIAN.Factories
{
    public class FacturacionElectronicaDIANFactory
    {
        public static string ErrorProsesoDian { get; private set; }
        public static string mensajeJsonText { get; set; }
        public static string TokenmensajeJsonText { get; set; }
        public static int EstadoMensaje { get; set; }

        public static string urlJSON = "";
        readonly string Api_Prod = urlJSON;
        //readonly string token;
        readonly string urlEndPoint;


        /// <summary>
        /// Factoria
        /// </summary>
        /// <param name="token"></param>
        /// <param name="fixed_hash"></param>
        /// <param name="usedTest"></param>
        public FacturacionElectronicaDIANFactory()
        {
            this.urlEndPoint = Api_Prod;
        }

        public Task<string> HttpWebRequestPost(string Url, string Json, HttpMethod httpMethod, [Optional] string token)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Task<string> task = Task.Run(() =>
            {
                string Response = null;

                try
                {
                    /// ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(urlEndPoint + Url);

                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Accept = "application/json";
                    httpWebRequest.Method = httpMethod.ToString();

                    if (!string.IsNullOrEmpty(token))
                    {
                        httpWebRequest.Headers.Add("Authorization", "Bearer " + token);

                    }

                    if ((httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put) && Json != null)
                    {
                        try
                        {
                            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                            {
                                
                                streamWriter.Write(Json);
                                streamWriter.Flush();
                                streamWriter.Close();
                            }
                        }
                        catch(Exception ex)
                        {
                            string error = ex.Message;
                            Response = error;
                        }

                    }
                        using (HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                        {
                            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                if (httpResponse.StatusCode == HttpStatusCode.OK || httpResponse.StatusCode == HttpStatusCode.Created)
                                {
                                    Response = streamReader.ReadToEnd();
                                }
                            }
                        }
                }
                catch (WebException ex)
                {
                    Response = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    ErrorProsesoDian = Response;
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    Response = "error";
                }

                return Response;
            });
            return task;
        }
        public async Task<FacturaNacionalResponse> FacturaNacional(FacturaNacionalRequest facturaNacional,int idVenta_frm)
        {
            try
            {
                string urlParameter = "invoice/" + facturaNacional.testSetID;

                string Json = JsonConvert.SerializeObject(facturaNacional);

                Json = Json.Replace("\"allowance_char" +
                    "ges\":null,", "");
                string Response=string.Empty;

                int maximoIntentos = 1; // ajusta a tu necesidad
                for (int intento = 1; intento <= maximoIntentos; intento++)
                {
                    Response = await HttpWebRequestPost(urlParameter, Json, HttpMethod.Post, facturaNacional.token);

                    if (Response?.Contains("Documento procesado anteriormente") == true)
                    {
                        // Preparar siguiente intento
                        facturaNacional.number = facturaNacional.number + 1;
                        urlParameter = $"invoice/{facturaNacional.testSetID}";
                        Json = JsonConvert.SerializeObject(facturaNacional)
                               .Replace("\"allowance_char" + "ges\":null,", "");
                        Response = string.Empty;
                        maximoIntentos++;
                        // Pausa de 10s antes del siguiente intento (si aún quedan intentos)
                        if (intento < maximoIntentos)
                            await Task.Delay(TimeSpan.FromSeconds(10));

                        continue; // siguiente vuelta
                    }

                    // Éxito u otro mensaje: salimos
                    break;
                }

                if (!String.IsNullOrEmpty(Response))
                {
                    FacturaNacionalResponse facturaNacionalResponse = JsonConvert.DeserializeObject<FacturaNacionalResponse>(Response);
                    if (facturaNacionalResponse.errors_messages!=null)
                    {
                        if (facturaNacionalResponse.errors_messages.Count > 0)
                        {

                        }
                    }

                    if (facturaNacionalResponse.zip_key != null)
                    {
                        facturaNacionalResponse = await FacturaValida(facturaNacional, facturaNacionalResponse.zip_key.ToString());
                    }

                    return facturaNacionalResponse;
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return null;
                //throw;
            }
        }
        public async Task<POSResponse> FacturaPOS(POSRequest facturaPOS, int idVenta_frm)
        {
            try
            {
                string urlParameter = "pos/" + facturaPOS.testSetID;

                string Json = JsonConvert.SerializeObject(facturaPOS);

                Json = Json.Replace("\"allowance_char" +
                    "ges\":null,", "");
                string Response=string.Empty;
                int maxIntentos = 1;
                for (int intento = 1; intento == maxIntentos; intento++)
                {
                    Response = await HttpWebRequestPost(urlParameter, Json, HttpMethod.Post, facturaPOS.token);
                    if(Response.Contains("Documento procesado anteriormente"))
                    {
                        maxIntentos++;
                        facturaPOS.number = facturaPOS.number++;
                        urlParameter = "pos/" + facturaPOS.testSetID;
                        Json = JsonConvert.SerializeObject(facturaPOS);
                        Json = Json.Replace("\"allowance_char" + "ges\":null,", "");
                        Response = string.Empty;
                    }
                }
                


                if (!String.IsNullOrEmpty(Response))
                {
                    POSResponse fposResponse = JsonConvert.DeserializeObject<POSResponse>(Response);
                    if (fposResponse.errors_messages != null)
                    {
                        if (fposResponse.errors_messages.Count > 0)
                        {

                        }
                    }

                    if (fposResponse.zip_key != null)
                    {
                        fposResponse = await POSValida(facturaPOS, fposResponse.zip_key.ToString());
                    }

                    return fposResponse;
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return null;
                //throw;
            }
        }
        public async Task<NotaCreditoResponse> NotaCredito(NotaCreditoRequest notaCredito)
        {
            try
            {
                string urlParameter = "credit-note/";

                string Json = JsonConvert.SerializeObject(notaCredito);

                Json = Json.Replace("\"allowance_charges\":null,", "");

                string Response = await HttpWebRequestPost(urlParameter, Json, HttpMethod.Post, notaCredito.token);

                if (!String.IsNullOrEmpty(Response))
                {
                    NotaCreditoResponse notaCreditoResponse = JsonConvert.DeserializeObject<NotaCreditoResponse>(Response);

                    return notaCreditoResponse;
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<NotaDebitoResponse> NotaDebito(NotaDebitoRequest notaDebito)
        {
            try
            {
                string urlParameter = "debit-note/" + notaDebito.testSetID;

                string Json = JsonConvert.SerializeObject(notaDebito);

                Json = Json.Replace("\"allowance_charges\":null,", "");

                string Response = await HttpWebRequestPost(urlParameter, Json, HttpMethod.Post, notaDebito.token);

                if (!String.IsNullOrEmpty(Response))
                {
                    NotaDebitoResponse notaDebitoResponse = JsonConvert.DeserializeObject<NotaDebitoResponse>(Response);

                    return notaDebitoResponse;
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<FacturaNacionalResponse> FacturaValida(FacturaNacionalRequest facturaNacional, string zip_key)
        {
            try
            {
                string urlParameter = "invoice/" + zip_key;

                string Json = JsonConvert.SerializeObject(facturaNacional);

                Json = Json.Replace("\"allowance_charges\":null,", "");

                string Response = await HttpWebRequestPost(urlParameter, Json, HttpMethod.Post, facturaNacional.token);

                if (!String.IsNullOrEmpty(Response))
                {
                    return JsonConvert.DeserializeObject<FacturaNacionalResponse>(Response);
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<POSResponse> POSValida(POSRequest facturaPOS, string zip_key)
        {
            try
            {
                string urlParameter = "invoice/" + zip_key;

                string Json = JsonConvert.SerializeObject(facturaPOS);

                Json = Json.Replace("\"allowance_charges\":null,", "");

                string Response = await HttpWebRequestPost(urlParameter, Json, HttpMethod.Post, facturaPOS.token);

                if (!String.IsNullOrEmpty(Response))
                {
                    return JsonConvert.DeserializeObject<POSResponse>(Response);
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<CorreoResponse> FacturaMail(CorreoRequest facturaNacional, string uuid)
        {
            try
            {
                string urlParameter = "mail/send/" + uuid;

                string Json = JsonConvert.SerializeObject(facturaNacional);

                Json = Json.Replace("\"allowance_charges\":null,", "");

                string Response = await HttpWebRequestPost(urlParameter, Json, HttpMethod.Post, facturaNacional.token);

                if (!String.IsNullOrEmpty(Response))
                {
                    return JsonConvert.DeserializeObject<CorreoResponse>(Response);
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<FacturaNacionalResponse> ConsultaEstadoDocumento(string token, string uuid)
        {
            try
            {
                string urlParameter = "status/document/" + uuid;

                string Json = string.Empty;

                Json = Json.Replace("\"allowance_charges\":null,", "");

                string Response = await HttpWebRequestPost(urlParameter, Json, HttpMethod.Post,token);

                if (!String.IsNullOrEmpty(Response))
                {
                    return JsonConvert.DeserializeObject<FacturaNacionalResponse>(Response);
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<string> ConsultarRUT(string nit, string token)
        {
            try
            {
                string urlParameter = $"status/nit/{nit}";
                string response = await HttpWebRequestPost(urlParameter, "", HttpMethod.Post, token);
                if (!String.IsNullOrEmpty(response))
                {
                    return response;
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<ConsultarEmail_Response> ConsultarEmail(ConsultarEmail_Request facturaNacional, string nit)
        {
            try
            {
                string urlParameter = "mail/reception/" + nit;

                string Json = JsonConvert.SerializeObject(facturaNacional);

                Json = Json.Replace("\"allowance_charges\":null,", "");

                string Response = await HttpWebRequestPost(urlParameter, Json, HttpMethod.Post, facturaNacional.token);

                if (!String.IsNullOrEmpty(Response))
                {
                    return JsonConvert.DeserializeObject<ConsultarEmail_Response>(Response);
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return null;
                //throw;
            }
        }


        public async Task<DocumentoSoporteResponse> DocumentoSoporte(DocumentoSoportelRequest documentoSoporte, int idVenta_frm)
        {
            try
            {
                string urlParameter = "document-support";

                string Json = JsonConvert.SerializeObject(documentoSoporte);

                Json = Json.Replace("\"allowance_char" +
                    "ges\":null,", "");

                string Response = await HttpWebRequestPost(urlParameter, Json, HttpMethod.Post, documentoSoporte.token);

                if (!String.IsNullOrEmpty(Response))
                {
                    return JsonConvert.DeserializeObject<DocumentoSoporteResponse>(Response);
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<NotaCreditoPOSResponse> NotaCreditoPOS(NotaCreditoPOSRequest notaCredito)
        {
            try
            {
                string urlParameter = "pos/credit-note/"+notaCredito.testSetID;

                string Json = JsonConvert.SerializeObject(notaCredito);

                Json = Json.Replace("\"allowance_charges\":null,", "");

                string Response = await HttpWebRequestPost(urlParameter, Json, HttpMethod.Post, notaCredito.token);

                if (!String.IsNullOrEmpty(Response))
                {
                    NotaCreditoPOSResponse notaCreditoResponse = JsonConvert.DeserializeObject<NotaCreditoPOSResponse>(Response);

                    return notaCreditoResponse;
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<CrearEmpresaResponse> CrearEmpresa(CrearEmpresaRequest crearEmpresaRequest,string nit,string token)
        {
            try
            {
                string urlParameter = $"config/{nit}";
                string Json=JsonConvert.SerializeObject (crearEmpresaRequest);
                Json = Json.Replace("\"allowance_charges\":null,", "");
                string response = await HttpWebRequestPost(urlParameter, Json, HttpMethod.Post, token);
                if (!String.IsNullOrEmpty(response))
                {
                    return JsonConvert.DeserializeObject<CrearEmpresaResponse>(response);
                }

                return null;
            }
            catch(Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<ConsultarEmpresaSoenacResponse> ConsultarEmpresa(string nit, string token)
        {
            try
            {
                string urlParameter = $"config/{nit}";
                string response = await HttpWebRequestPost(urlParameter, "", HttpMethod.Get, token);
                if (!String.IsNullOrEmpty(response))
                {
                    return JsonConvert.DeserializeObject<ConsultarEmpresaSoenacResponse>(response);
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<string> EliminarEmpresa(string nit, string token)
        {
            try
            {
                string urlParameter = $"config/{nit}";
                string response = await HttpWebRequestPost(urlParameter, "", HttpMethod.Delete, token);
                if (!String.IsNullOrEmpty(response))
                {
                    return response;
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<List<ListaResolucionesDIAN.Lista_Resoculiones>> ConsultarListaResoluciones(string token)
        {
            try
            {
                string urlParameter = $"config/resolutions";
                string response=await HttpWebRequestPost(urlParameter,"",HttpMethod.Get, token);
                if (!String.IsNullOrEmpty(response))
                {
                    return JsonConvert.DeserializeObject<List<ListaResolucionesDIAN.Lista_Resoculiones>>(response);
                }
                return null;
            }
            catch (Exception ex) 
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<SoftwareDIAN_Response> ActualizarSoftware(SoftwareDIAN_Reguest softwareDIAN_Reguest, string token)
        {
            try
            {
                string urlParameter = $"config/software";
                string Json = JsonConvert.SerializeObject(softwareDIAN_Reguest);
                Json = Json.Replace("\"allowance_charges\":null,", "");
                string response = await HttpWebRequestPost(urlParameter, Json, HttpMethod.Put, token);
                if (!String.IsNullOrEmpty(response))
                {
                    return JsonConvert.DeserializeObject<SoftwareDIAN_Response>(response);
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<SoftwareDIAN_Response> ConsultarSoftwareDIAN(string token)
        {
            try
            {
                string urlParameter = $"config/software";
                string response = await HttpWebRequestPost(urlParameter, "", HttpMethod.Get, token);
                if (!String.IsNullOrEmpty(response))
                {
                    return JsonConvert.DeserializeObject<SoftwareDIAN_Response>(response);
                }
                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<ActualizarAmbienteResponse> ActualizarAmbiente(ActualizarAmbienteRequest actualizarAmbienteRequest, string token)
        {
            try
            {
                string urlParameter = $"config/environment";
                string Json = JsonConvert.SerializeObject(actualizarAmbienteRequest);
                Json = Json.Replace("\"allowance_charges\":null,", "");
                string response = await HttpWebRequestPost(urlParameter, Json, HttpMethod.Put, token);
                if (!String.IsNullOrEmpty(response))
                {
                    return JsonConvert.DeserializeObject<ActualizarAmbienteResponse>(response);
                }

                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<Acquirer_Response> ConsultarAcquirer(Acquirer_Request acquirer_Request,string token)
        {
            try
            {
                string urlParameter = $"status/acquirer";
                string Json = JsonConvert.SerializeObject(acquirer_Request);
                Json = Json.Replace("\"allowance_charges\":null,", "");
                string response = await HttpWebRequestPost(urlParameter,Json, HttpMethod.Post, token);
                if (!String.IsNullOrEmpty(response))
                {
                    return JsonConvert.DeserializeObject<Acquirer_Response>(response);
                }
                return null;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                throw;
            }
        }
        public async Task<WhatsAppResponse> WhatsApp_(WhatsAppRequest whatsapp)
        {
            try
            {
                string urlParameter = "";

                string Json = JsonConvert.SerializeObject(whatsapp);

                Json = Json.Replace("\"allowance_charges\":null,", "");

                string Response = await HttpWebRequestPost(urlParameter, Json, HttpMethod.Post, whatsapp.token);
                return JsonConvert.DeserializeObject<WhatsAppResponse>(Response);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return null;
            }
        }

        public async Task<DeleteSMTP_Response> DeleteSMTP(string token)
        {
            string url = "config/smtp";
            string respuesta = await HttpWebRequestPost(url,"",HttpMethod.Delete,token);
            var deleteResponsse = JsonConvert.DeserializeObject<DeleteSMTP_Response>(respuesta);
            return deleteResponsse;
        }
        public async Task<SMTP_Response> ConfigurarSMTP(SMTP_Request sMTP_Request,string token)
        {
            string url = "config/smtp";
            string Json = JsonConvert.SerializeObject(sMTP_Request);

            Json = Json.Replace("\"allowance_char" +
                "ges\":null,", "");

            string respuesta = await HttpWebRequestPost(url, Json, HttpMethod.Put, token);
            var config = JsonConvert.DeserializeObject<SMTP_Response>(respuesta);
            return config;
        }
    }
}
