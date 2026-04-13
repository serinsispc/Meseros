using DAL.Controler;
using DAL.Funciones;
using DAL.Model;
using Newtonsoft.Json;
using RFacturacionElectronicaDIAN.Entities.Request;
using RFacturacionElectronicaDIAN.Factories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web;
using WebApplication.Helpers;
using WebApplication.ViewModels;
using System.Text.RegularExpressions;
using Acquirer_Response = RFacturacionElectronicaDIAN.Entities.Response.Acquirer_Response;

namespace WebApplication
{
    public partial class HVentas : Page
    {
        public class Valores
        {
            public decimal total { get; set; }
            public decimal propina { get; set; }
            public int facturas { get; set; }
            public int anuladas { get; set; }
            public int ventasCreditoCantidad { get; set; }
            public decimal ventasCreditoValor { get; set; }
        }
        protected Valores valores { get; set; } = new Valores();
        protected V_TablaVentas venta { get; set; } = new V_TablaVentas();
        protected List<V_DetalleCaja> detalleCaja { get; set; } = new List<V_DetalleCaja>();
        protected List<V_Resoluciones> listaResoluciones { get; set; } = new List<V_Resoluciones>();
        protected List<Clientes> listaClientes { get; set; } = new List<Clientes>();

        protected bool _mdlVenta = false;
        protected bool _mdlResolucionVenta = false;
        protected bool _mdlClienteVenta = false;

        private readonly JavaScriptSerializer _json = new JavaScriptSerializer();
        private readonly CultureInfo _co = new CultureInfo("es-CO");


        #region Ciclo de vida

        protected async void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    await InicializarPagina();
                }

                await ProcesarPostBack();
                await RefrescarVista();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Page_Load HVentas: " + ex.Message);
            }
        }

        #endregion

        #region Inicialización

        private async Task InicializarPagina()
        {
            await CargarCombosIniciales();
            await CargarDatosInicialesCliente();
            //cargamos todos los ddl
            await CargarDDL();
        }

        private async Task CargarCombosIniciales()
        {
            await Task.CompletedTask;
        }

        private async Task CargarDatosInicialesCliente()
        {
            await Task.CompletedTask;
        }

        #endregion

        #region Router PostBack
        public dynamic ExtraerValores(string eventArgument)
        {
            return JsonConvert.DeserializeObject<dynamic>(eventArgument);
        }
        private async Task ProcesarPostBack()
        {
            var eventTarget = Request["__EVENTTARGET"];
            var eventArgument = Request["__EVENTARGUMENT"];

            if (string.IsNullOrWhiteSpace(eventTarget))
                return;

            switch (eventTarget)
            {
                case "btnAbrirVenta":
                    await btnAbrirVenta(eventArgument);
                    break;

                case "btnEditarResolucion":
                    await btnEditarResolucion(eventArgument);
                    break;

                case "btnEditarCliente":
                    await btnEditarCliente(eventArgument);
                    break;

                case "btnBuscarNIT":
                    await btnBuscarNIT(eventArgument);
                    break;

                case "btnGuardarCliente":
                    await btnGuardarCliente(eventArgument);
                    break;


                case "btnImprimirVenta":
                    await btnImprimirVenta(eventArgument);
                    break;

                case "btnGuardarResolucion":
                    await btnGuardarResolucion(eventArgument);
                    break;

                case "btnSeleccionarCliente":
                    await btnSeleccionarCliente(eventArgument);
                    break;

                case "btnEnviarDIAN":
                    await btnEnviarDIAN(eventArgument);
                    break;

                case "btnDescargarPDF":
                    await btnDescargarPDF(eventArgument);
                    break;
            }
        }

        #endregion

        #region Refresco de vista

        private async Task RefrescarVista()
        {
            await CargarVentas();
        }

        private async Task CargarVentas()
        {
            var model = JsonConvert.DeserializeObject<MenuViewModels>(Session["ModelsJson"].ToString());
            var listaventas = await V_TablaVentasControler.Lista(Session["db"].ToString(), model.BaseCaja.id);
            var ventasValidas = listaventas?.Where(x => EsVentaValida(x)).ToList() ?? new List<V_TablaVentas>();
            var ventasContadoValidas = ventasValidas.Where(x => x.idFormaDePago != 2).ToList();

            valores = new Valores
            {
                total = ventasContadoValidas.Sum(x => x.total_A_Pagar),
                propina = ventasValidas.Sum(x => x.propina),
                facturas = ventasContadoValidas.Count,
                anuladas = listaventas?.Count(x => EsVentaAnulada(x)) ?? 0,
                ventasCreditoCantidad = ventasValidas.Count(x => x.idFormaDePago == 2),
                ventasCreditoValor = ventasValidas.Where(x => x.idFormaDePago == 2).Sum(x => x.total_A_Pagar)
            };

            rpVentas.DataSource = listaventas;
            rpVentas.DataBind();
        }

        protected bool EsVentaAnulada(V_TablaVentas item)
        {
            return string.Equals(item?.estadoVenta, "ANULADA", StringComparison.OrdinalIgnoreCase);
        }

        protected bool EsVentaValida(V_TablaVentas item)
        {
            return string.Equals(item?.estadoVenta, "CANCELADO", StringComparison.OrdinalIgnoreCase);
        }

        protected string ObtenerEstadoBadge(string cufe, string tipoFactura)
        {
            if (cufe != "--" && tipoFactura == "FACTURA ELECTRÓNICA DE VENTA")
                return "<span class='hv-badge success'><i class='bi bi-qr-code-scan'></i>Emitida</span>";

            return "<span class='hv-badge danger'><i class='bi bi-x-circle'></i>Rechazada</span>";
        }

        #endregion


        #region Eventos

        private async Task btnAbrirVenta(string idventa)
        {
            int id = Convert.ToInt32(idventa);

            var v_tabla = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), id);
            venta = new V_TablaVentas();
            venta = v_tabla ?? new V_TablaVentas();

            var detalle = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), id, 0);
            detalleCaja = detalle ?? new List<V_DetalleCaja>();

            rpDetalleCaja.DataSource = detalleCaja;
            rpDetalleCaja.DataBind();

            _mdlVenta = true;
        }
        private async Task btnEditarResolucion(string idventa)
        {
            int id = Convert.ToInt32(idventa);

            var v_tabla = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), id);
            venta = new V_TablaVentas();
            venta = v_tabla ?? new V_TablaVentas();
            //en esta parte guardamos la venta en json session
            string jsonventa = JsonConvert.SerializeObject(venta);
            Session["venta"] = jsonventa;

            if (venta.cufe != "--")
            {
                await MostrarMensaje(TipoMensaje.Warning,"Factura electrónica","La factura es electrónica y ya fue aceptada por la DIAN.");
                return;
            }

            //cargamos las resoluciones
            var resoluciones = await V_Resoluciones_Controler.Lista(Session["db"].ToString());
            if (resoluciones.Count > 0)
            {
                listaResoluciones = new List<V_Resoluciones>();
                listaResoluciones = resoluciones;
                rpResoluciones.DataSource = resoluciones;
                rpResoluciones.DataBind();
            }

            _mdlResolucionVenta = true;
        }
        public class GuardarResolucionDto
        {
            public int resolucionId { get; set; }
            public int ventaId { get; set; }
        }

        public class FacturaPdfDetalleDto
        {
            public string codigo { get; set; }
            public string descripcion { get; set; }
            public string cantidad { get; set; }
            public decimal unitario { get; set; }
            public decimal impuesto { get; set; }
            public decimal total { get; set; }
        }

        public class FacturaPdfDto
        {
            public string archivo { get; set; }
            public string empresaNombre { get; set; }
            public string empresaNit { get; set; }
            public string empresaDireccion { get; set; }
            public string empresaTelefono { get; set; }
            public string empresaEmail { get; set; }
            public string tipoFactura { get; set; }
            public string numeroFactura { get; set; }
            public string fechaGeneracion { get; set; }
            public string fechaExpedicion { get; set; }
            public string fechaVencimiento { get; set; }
            public string clienteNombre { get; set; }
            public string clienteDocumento { get; set; }
            public string clienteDireccion { get; set; }
            public string clienteTelefono { get; set; }
            public string clienteCiudad { get; set; }
            public string formaPago { get; set; }
            public string observacion { get; set; }
            public string resolucionNumero { get; set; }
            public string resolucionFecha { get; set; }
            public string resolucionRango { get; set; }
            public string cufe { get; set; }
            public string nombreImpuesto { get; set; }
            public string nombreRecargo { get; set; }
            public decimal porcentajeRecargo { get; set; }
            public int totalItems { get; set; }
            public decimal totalBruto { get; set; }
            public decimal descuento { get; set; }
            public decimal iva { get; set; }
            public decimal recargo { get; set; }
            public decimal totalPagar { get; set; }
            public string valorLetras { get; set; }
            public string logoBase64 { get; set; }
            public string qrBase64 { get; set; }
            public List<FacturaPdfDetalleDto> detalles { get; set; }
        }
        private async Task btnGuardarResolucion(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new Exception("Payload vacío");

            var obj = JsonConvert.DeserializeObject<GuardarResolucionDto>(data);

            if (obj == null)
                throw new Exception("No se pudo deserializar");

            if (obj.resolucionId <= 0 || obj.ventaId <= 0)
                throw new Exception("Datos inválidos");

            var db = Session["db"]?.ToString();
            if (string.IsNullOrWhiteSpace(db))
                throw new Exception("La sesión de base de datos no está disponible");

            var idResolucion = obj.resolucionId;
            var idVenta = obj.ventaId;

            var venta = await TablaVentasControler.ConsultarIdVenta(db, idVenta);
            if (venta == null)
                throw new Exception("No se encontró la venta");

            venta.idResolucion = idResolucion;

            var resp = await TablaVentasControler.CRUD(db, venta, 1);
            if (resp == null || !resp.estado)
                throw new Exception("No fue posible editar la resolución");

            await MostrarMensaje(TipoMensaje.Success, "Ok", "Resolución editada correctamente.");
        }

        private async Task btnImprimirVenta(string idventa)
        {

            var imprimir = new ImprimirFactura() { id = 0, idventa = Convert.ToInt32(idventa) };
            var resp = await ImprimirFacturaControler.CRUD(Session["db"].ToString(), imprimir, 0);
            if (!resp.estado)
            {
                await MostrarMensaje(TipoMensaje.Error, "Error", "No fue posible enviar la factura.");
                await Task.CompletedTask;
                return;
            }

            await MostrarMensaje(TipoMensaje.Success, "Ok", "Factura enviada.");
            await Task.CompletedTask;
        }

        private async Task btnBuscarNIT(string nit)
        {
            try
            {
                nit = (nit ?? "").Trim();

                if (string.IsNullOrWhiteSpace(nit))
                {
                    await MostrarMensaje(TipoMensaje.Warning, "Atención", "Debes ingresar un NIT.");
                    return;
                }

                var clientesJson = Session["clientes"]?.ToString();
                var clientes = string.IsNullOrWhiteSpace(clientesJson)
                    ? new List<Clientes>()
                    : JsonConvert.DeserializeObject<List<Clientes>>(clientesJson) ?? new List<Clientes>();

                var cliente = clientes.FirstOrDefault(x => x.identificationNumber == nit);

                if (cliente == null)
                {
                    if (!int.TryParse(nit, out var nitNumero))
                    {
                        await MostrarMensaje(TipoMensaje.Warning, "Atención", "El NIT no tiene un formato válido.");
                        return;
                    }

                    cliente = new Clientes();
                    var acquirer_Response = await Consultar_NIT_DIAN(nitNumero);

                    if (acquirer_Response == null || string.IsNullOrWhiteSpace(acquirer_Response.name))
                    {
                        var limpiarPayload = new
                        {
                            typeDocId = "",
                            nit = "",
                            orgId = "",
                            municipioId = "",
                            regimenId = "",
                            responsabilidadId = "",
                            impuestoId = "",
                            nombre = "",
                            comercio = "",
                            telefono = "",
                            direccion = "",
                            correo = "",
                            matricula = "",
                            actionLabel = "Guardar"
                        };

                        var limpiarJson = JsonConvert.SerializeObject(limpiarPayload);
                        var limpiarScript = $"Swal.fire({{icon:'error',title:'¡Error!',text:'El documento no se encuentra registrado ni en la base de datos ni en la DIAN. Debes crearlo manualmente.',confirmButtonColor:'#2563eb'}}).then(function(){{if(window.setClienteData){{window.setClienteData({limpiarJson});}} var modalEl=document.getElementById('mdlCliente'); if(modalEl){{bootstrap.Modal.getOrCreateInstance(modalEl).show();}}}});";

                        await EjecutarScript(limpiarScript);
                        _mdlClienteVenta = true;
                        return;
                    }

                    cliente.typeDocumentIdentification_id = 6;
                    cliente.identificationNumber = nit;
                    cliente.typeOrganization_id = 2;
                    cliente.municipality_id = 605;
                    cliente.typeRegime_id = 2;
                    cliente.typeLiability_id = 29;
                    cliente.typeTaxDetail_id = 5;
                    cliente.nameCliente = acquirer_Response.name;
                    cliente.tradeName = "-";
                    cliente.phone = "0";
                    cliente.adress = "-";
                    cliente.email = acquirer_Response.email;
                    cliente.merchantRegistration = "0";
                }

                var payload = new
                {
                    typeDocId = cliente.typeDocumentIdentification_id,
                    nit = cliente.identificationNumber,
                    orgId = cliente.typeOrganization_id,
                    municipioId = cliente.municipality_id,
                    regimenId = cliente.typeRegime_id,
                    responsabilidadId = cliente.typeLiability_id,
                    impuestoId = cliente.typeTaxDetail_id,
                    nombre = cliente.nameCliente,
                    comercio = cliente.tradeName,
                    telefono = cliente.phone,
                    direccion = cliente.adress,
                    correo = cliente.email,
                    matricula = cliente.merchantRegistration
                };

                var json = JsonConvert.SerializeObject(payload);
                var script = $"Swal.close(); if(window.setClienteData){{window.setClienteData({json});}} var modalEl=document.getElementById('mdlCliente'); if(modalEl){{bootstrap.Modal.getOrCreateInstance(modalEl).show();}}";

                await EjecutarScript(script);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error btnBuscarNIT: " + ex.Message);
                await MostrarMensaje(TipoMensaje.Error, "Error", "No fue posible consultar el NIT.");
            }

            _mdlClienteVenta = true;
        }

        private async Task btnGuardarCliente(string raw)
        {
            try
            {
                int accion = 0;
                int idcliente = 0;
                if (string.IsNullOrWhiteSpace(raw))
                    return;

                // Aquí luego parseas JSON real
                var data = JsonConvert.DeserializeObject<dynamic>(raw);

                var cliente = await ClientesControler.Consultar_nit(Session["db"].ToString(),(string)data.identificacion);
                if (cliente == null) 
                {
                    cliente = new Clientes();
                }
                else
                {
                    idcliente = cliente.id;
                    accion = 1;
                }

                cliente.id = idcliente;
                cliente.typeDocumentIdentification_id = data.tipoDocumento;
                cliente.typeOrganization_id = data.tipoOrganizacion;
                cliente.municipality_id = data.municipio;
                cliente.typeRegime_id = data.regimen;
                cliente.typeLiability_id = data.responsabilidad;
                cliente.typeTaxDetail_id = data.impuesto;
                cliente.nameCliente = data.nombre;
                cliente.tradeName = data.comercio;
                cliente.phone = data.telefono;
                cliente.adress = data.direccion;
                cliente.email = data.correo;
                cliente.merchantRegistration = data.matricula;
                cliente.identificationNumber = data.identificacion;

                if ((bool)data.esProveedor) cliente.idTipoTercero = data.esProveedor;
                if ((bool)data.esCliente) cliente.idTipoTercero = data.esCliente;

                var resp = await ClientesControler.CRUD(Session["db"].ToString(),cliente,accion);
                if (!resp.estado)
                {
                    await MostrarMensaje(TipoMensaje.Error,"Error","No fue pocible gestionar el cliente.");
                    return;
                }

                await MostrarMensaje(TipoMensaje.Success, "OK", "Cliente guardado correctamente.");

                await CargarClientes();

                _mdlClienteVenta = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error btnGuardarCliente: " + ex.Message);
            }

            await Task.CompletedTask;
        }

        private async Task btnEditarCliente(string arg)
        {
            var v_tabla = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), Convert.ToInt32(arg));
            venta = new V_TablaVentas();
            venta = v_tabla ?? new V_TablaVentas();
            if (venta.cufe != "--")
            {
                await MostrarMensaje(TipoMensaje.Warning, "Factura electrónica", "La factura es electrónica y ya fue aceptada por la DIAN.");
                return;
            }

            Session["idventa"]=arg;
            await CargarClientes();

            // Solo abre modal por ahora
            _mdlClienteVenta = true;
            await Task.CompletedTask;
        }
        private async Task CargarClientes()
        {
            //cargamos el listado de los clientes
            var clientes = await ClientesControler.ListaClientes(Session["db"].ToString());
            if (clientes == null) return;

            listaClientes = new List<Clientes>();
            listaClientes = clientes;
            //guardamos la lista en json session
            Session["clientes"] = JsonConvert.SerializeObject(listaClientes);
            rpClientes.DataSource = clientes;
            rpClientes.DataBind();
        }

        public class ClienteSeleccionado
        {
             public int clienteId {  get; set; }
        }

        private async Task btnSeleccionarCliente(string objeto)
        {
            var data = JsonConvert.DeserializeObject<ClienteSeleccionado>(objeto);
            var idCliente = data.clienteId;

            var idventa = Convert.ToInt32(Session["idventa"].ToString());

            int accion = 0;
            int idR = 0;

            var rvc = await R_VentaCliente_Controler.ConsultarRelacion(Session["db"].ToString(),idventa);
            if (rvc == null)
            {
                rvc = new R_VentaCliente();
            }
            else
            {
                idR = rvc.id;
                accion = 1;
            }

            rvc.id= idR;
            rvc.idVenta = idventa;
            rvc.idCliente= idCliente;

            var resp = await R_VentaCliente_Controler.CRUD(Session["db"].ToString(),rvc,accion);
            if (!resp)
            {
                await MostrarMensaje(TipoMensaje.Error,"Error","No se pudo editar el cliente.");
                return;
            }

            await MostrarMensaje(TipoMensaje.Success,"Ok",$"Cliente editado con éxito.");
        }

        private async Task CargarDDL()
        {
            ddlTipoDocumentoHv.DataSource = await type_document_identificationsControler.ListaTiposDocumento(Session["db"].ToString());
            ddlTipoDocumentoHv.DataTextField = "name";
            ddlTipoDocumentoHv.DataValueField = "id";
            ddlTipoDocumentoHv.DataBind();
            ddlTipoDocumentoHv.Items.Insert(0, new ListItem("Seleccionar", ""));

            ddlTipoOrganizacionHv.DataSource = await type_organizationsControler.ListaTiposOrganizacion(Session["db"].ToString());
            ddlTipoOrganizacionHv.DataTextField = "name";
            ddlTipoOrganizacionHv.DataValueField = "id";
            ddlTipoOrganizacionHv.DataBind();
            ddlTipoOrganizacionHv.Items.Insert(0, new ListItem("Seleccionar", ""));

            ddlMunicipioHv.DataSource = await V_MunicipiosControler.ListaMunicipios(Session["db"].ToString());
            ddlMunicipioHv.DataTextField = "name";
            ddlMunicipioHv.DataValueField = "id";
            ddlMunicipioHv.DataBind();
            ddlMunicipioHv.Items.Insert(0, new ListItem("Seleccionar", ""));

            ddlTipoRegimenHv.DataSource = await type_regimesControler.ListaTiposRegimen(Session["db"].ToString());
            ddlTipoRegimenHv.DataTextField = "name";
            ddlTipoRegimenHv.DataValueField = "id";
            ddlTipoRegimenHv.DataBind();
            ddlTipoRegimenHv.Items.Insert(0, new ListItem("Seleccionar", ""));

            ddlTipoResponsabilidadHv.DataSource = await type_liabilitiesControler.ListaTiposResponsabilidad(Session["db"].ToString());
            ddlTipoResponsabilidadHv.DataTextField = "name";
            ddlTipoResponsabilidadHv.DataValueField = "id";
            ddlTipoResponsabilidadHv.DataBind();
            ddlTipoResponsabilidadHv.Items.Insert(0, new ListItem("Seleccionar", ""));

            ddlDetalleImpuestoHv.DataSource = await tax_detailsControler.ListaDetallesImpuesto(Session["db"].ToString());
            ddlDetalleImpuestoHv.DataTextField = "name";
            ddlDetalleImpuestoHv.DataValueField = "id";
            ddlDetalleImpuestoHv.DataBind();
            ddlDetalleImpuestoHv.Items.Insert(0, new ListItem("Seleccionar", ""));
        }

        private async Task btnEnviarDIAN(string id)
        {
            var idVenta=Convert.ToInt32(id);
            var tablaventa = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), idVenta);
            if (tablaventa == null)
            {
                await MostrarMensaje(TipoMensaje.Warning,"DIAN","No fue posible enviar la factura.");
                return;
            }

            if (tablaventa.cufe != "--")
            {
                await MostrarMensaje(TipoMensaje.Warning, "DIAN", "La factura ya fue acectada por la DIAN.");
                return;
            }

            var detalleventa = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), idVenta, 0);
            if (detalleventa == null)
            {
                await MostrarMensaje(TipoMensaje.Warning, "DIAN", "No fue posible enviar la factura.");
                return;
            }

            var model = JsonConvert.DeserializeObject<MenuViewModels>(Session["ModelsJson"].ToString());

            var respDIAN =await ClassFE.FacturaElectronica(Session["db"].ToString(),tablaventa, detalleventa, Convert.ToString(model.TokenEmpresa),false);
            if (!respDIAN) 
            {
                await MostrarMensaje(TipoMensaje.Warning, "DIAN", "No fue posible enviar la factura.");
                return;
            }

            await MostrarMensaje(TipoMensaje.Success, "DIAN", "Factura enviada con éxito.");
        }
        private async Task btnDescargarPDF(string idVentaRaw)
        {
            if (!int.TryParse(idVentaRaw, out var idVenta) || idVenta <= 0)
            {
                await MostrarMensaje(TipoMensaje.Error, "Error", "No se recibió una venta válida para descargar el PDF.");
                return;
            }

            var payload = await ConstruirFacturaPdf(idVenta);
            if (payload == null)
            {
                await MostrarMensaje(TipoMensaje.Error, "Error", "No fue posible construir el PDF de la factura.");
                return;
            }

            var json = HttpUtility.JavaScriptStringEncode(JsonConvert.SerializeObject(payload));
            var script = $"window.hvBuildInvoicePdf(JSON.parse('{json}'));";
            await EjecutarScript(script);
        }

        #endregion

        #region Helpers PRO

        public enum TipoMensaje
        {
            Success,
            Error,
            Warning,
            Info
        }
        private async Task MostrarMensaje(TipoMensaje tipo, string titulo, string texto)
        {
            var icon = ObtenerIcono(tipo);

            var script = $@"
        if(window.Swal){{
            Swal.fire({{
                icon: '{icon}',
                title: '{titulo}',
                text: '{texto}',
                confirmButtonColor: '#2563eb'
            }});
        }}
    ";

            await EjecutarScript(script);
        }
        private string ObtenerIcono(TipoMensaje tipo)
        {
            switch (tipo)
            {
                case TipoMensaje.Success:
                    return "success";

                case TipoMensaje.Error:
                    return "error";

                case TipoMensaje.Warning:
                    return "warning";

                case TipoMensaje.Info:
                default:
                    return "info";
            }
        }

        private async Task EjecutarScript(string script)
        {
            if (string.IsNullOrWhiteSpace(script)) return;

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                Guid.NewGuid().ToString("N"),
                script,
                true
            );
        }

        private async Task<Acquirer_Response> Consultar_NIT_DIAN(int nit)
        {
            FacturacionElectronicaDIANFactory facturacionElectronica = new FacturacionElectronicaDIANFactory();

            Acquirer_Request acquirer_Request = new Acquirer_Request();

            acquirer_Request.environment = new Acquirer_Request.Environment();
            acquirer_Request.environment.type_environment_id = 1;

            acquirer_Request.type_document_identification_id = 6;
            acquirer_Request.identification_number = nit;

            string TokenFE = await controlador_tokenEmpresa.ConsultarTokenSerinsisPC();

            Acquirer_Response response = await facturacionElectronica.ConsultarAcquirer(acquirer_Request, TokenFE);

            return response;
        }

        private async Task<FacturaPdfDto> ConstruirFacturaPdf(int idVenta)
        {
            var db = Convert.ToString(Session["db"]);
            if (string.IsNullOrWhiteSpace(db))
            {
                return null;
            }

            var ventaFactura = await V_TablaVentasControler.Consultar_Id(db, idVenta);
            if (ventaFactura == null || string.IsNullOrWhiteSpace(ventaFactura.cufe) || ventaFactura.cufe == "--")
            {
                return null;
            }

            var detalleVenta = await V_DetalleCajaControler.Lista_IdVenta(db, idVenta, 0) ?? new List<V_DetalleCaja>();
            var model = SessionContextHelper.LoadModels(Session) ?? new MenuViewModels { db = db };
            var sede = model.Sede ?? await SedeControler.Consultar(db);
            var cliente = ventaFactura.idCliente > 0 ? await ClientesControler.Consultar_id(db, ventaFactura.idCliente) : null;
            var resolucion = ventaFactura.idResolucion > 0 ? await V_Resoluciones_Controler.ConsultarID(db, ventaFactura.idResolucion) : null;
            var facturaElectronica = await FacturaElectronicaControler.ConsultarIdVenta(db, idVenta);
            var porcentajePropina = Math.Round(ventaFactura.por_propina * 100m, 2);

            return new FacturaPdfDto
            {
                archivo = $"{(ventaFactura.prefijo ?? "FACT")}-{ventaFactura.numeroVenta}",
                empresaNombre = sede?.nombreSede ?? Convert.ToString(Session["NombreEmpresa"] ?? "Mi empresa"),
                empresaNit = sede?.nit ?? string.Empty,
                empresaDireccion = sede?.direccion ?? string.Empty,
                empresaTelefono = !string.IsNullOrWhiteSpace(sede?.telefono) ? sede.telefono : (sede?.celular ?? string.Empty),
                empresaEmail = sede?.correoAdmin1 ?? string.Empty,
                tipoFactura = string.IsNullOrWhiteSpace(ventaFactura.tipoFactura) ? "FACTURA DE VENTA" : ventaFactura.tipoFactura,
                numeroFactura = $"N. {ventaFactura.prefijo} {ventaFactura.numeroVenta}",
                fechaGeneracion = FormatearFechaPdf(ventaFactura.fechaVenta),
                fechaExpedicion = FormatearFechaPdf(ventaFactura.fechaVenta),
                fechaVencimiento = FormatearFechaPdf(ventaFactura.fechaVencimiento),
                clienteNombre = !string.IsNullOrWhiteSpace(cliente?.nameCliente) ? cliente.nameCliente : ventaFactura.nombreCliente,
                clienteDocumento = !string.IsNullOrWhiteSpace(cliente?.identificationNumber) ? cliente.identificationNumber : ventaFactura.nit,
                clienteDireccion = cliente?.adress ?? string.Empty,
                clienteTelefono = cliente?.phone ?? string.Empty,
                clienteCiudad = string.Empty,
                formaPago = !string.IsNullOrWhiteSpace(ventaFactura.medioDePago) ? ventaFactura.medioDePago : ventaFactura.formaDePago,
                observacion = string.IsNullOrWhiteSpace(ventaFactura.observacionVenta) ? "--" : ventaFactura.observacionVenta,
                resolucionNumero = resolucion?.numeroResolucion ?? string.Empty,
                resolucionFecha = resolucion?.fechaAvilitacion ?? string.Empty,
                resolucionRango = resolucion == null
                    ? string.Empty
                    : $"Prefijo {resolucion.prefijo} desde el número {resolucion.desde} hasta el número {resolucion.hasta}",
                cufe = ventaFactura.cufe,
                nombreImpuesto = "IVA",
                nombreRecargo = porcentajePropina > 0 ? $"Propina ({porcentajePropina.ToString("0.##", _co)}%)" : "Propina",
                porcentajeRecargo = porcentajePropina,
                totalItems = detalleVenta.Count,
                totalBruto = ventaFactura.subtotalVenta,
                descuento = ventaFactura.descuentoVenta,
                iva = ventaFactura.ivaVenta,
                recargo = ventaFactura.propina,
                totalPagar = ventaFactura.total_A_Pagar,
                valorLetras = ConvertirMonedaALetras(ventaFactura.total_A_Pagar),
                logoBase64 = ObtenerLogoBase64(db),
                qrBase64 = ObtenerQrBase64(ventaFactura, facturaElectronica),
                detalles = detalleVenta.Select(x => new FacturaPdfDetalleDto
                {
                    codigo = string.IsNullOrWhiteSpace(x.codigoProducto) ? "ITEM" : x.codigoProducto,
                    descripcion = ConstruirDescripcionDetalle(x),
                    cantidad = x.unidad.ToString("N1", _co),
                    unitario = x.precioVenta,
                    impuesto = x.valorImpuesto,
                    total = x.totalDetalle
                }).ToList()
            };
        }

        private string ConstruirDescripcionDetalle(V_DetalleCaja detalle)
        {
            var descripcion = detalle?.nombreProducto ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(detalle?.adiciones))
            {
                descripcion += " - " + detalle.adiciones;
            }

            return descripcion.Trim();
        }

        private string FormatearFechaPdf(DateTime fecha)
        {
            return fecha == DateTime.MinValue
                ? string.Empty
                : fecha.ToString("dd/MM/yyyy h:mm:ss tt", _co);
        }

        private string ObtenerLogoBase64(string db)
        {
            var ruta = Server.MapPath($"~/Recursos/Imagenes/logo/{db}.png");
            if (!File.Exists(ruta))
            {
                return string.Empty;
            }

            return "data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(ruta));
        }

        private string ObtenerQrBase64(V_TablaVentas ventaFactura, FacturaElectronica facturaElectronica)
        {
            var qr = NormalizarImagenBase64(ventaFactura?.imagenQR);
            if (!string.IsNullOrWhiteSpace(qr))
            {
                return qr;
            }

            qr = NormalizarImagenBase64(facturaElectronica?.imagenQR);
            if (!string.IsNullOrWhiteSpace(qr))
            {
                return qr;
            }

            if (!string.IsNullOrWhiteSpace(facturaElectronica?.dataQR) && facturaElectronica.dataQR != "--")
            {
                try
                {
                    var contenidoQr = ExtraerContenidoQr(facturaElectronica.dataQR);
                    var qrGenerado = ClassFE.General_qr(contenidoQr, ventaFactura?.id ?? 0)
                        .GetAwaiter()
                        .GetResult();

                    return NormalizarImagenBase64(qrGenerado);
                }
                catch
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }

        private string NormalizarImagenBase64(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64) || base64 == "--")
            {
                return string.Empty;
            }

            base64 = base64.Trim();
            if (base64.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            {
                return base64;
            }

            return "data:image/png;base64," + base64;
        }

        private string ConvertirMonedaALetras(decimal valor)
        {
            valor = Math.Round(valor, 2, MidpointRounding.AwayFromZero);
            var entero = (long)Math.Truncate(valor);
            var decimales = (int)((valor - entero) * 100m);

            return $"{NumeroALetras(entero)} PESOS {decimales:00}/100";
        }

        private string NumeroALetras(long numero)
        {
            if (numero == 0) return "CERO";
            if (numero < 0) return "MENOS " + NumeroALetras(Math.Abs(numero));

            if (numero >= 1000000)
            {
                var millones = numero / 1000000;
                var resto = numero % 1000000;
                var textoMillones = millones == 1 ? "UN MILLON" : NumeroALetras(millones) + " MILLONES";
                return resto > 0 ? textoMillones + " " + NumeroALetras(resto) : textoMillones;
            }

            if (numero >= 1000)
            {
                var miles = numero / 1000;
                var resto = numero % 1000;
                var textoMiles = miles == 1 ? "MIL" : NumeroALetras(miles) + " MIL";
                return resto > 0 ? textoMiles + " " + NumeroALetras(resto) : textoMiles;
            }

            if (numero >= 100)
            {
                if (numero == 100) return "CIEN";

                string[] centenas = { "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS" };
                var centena = centenas[numero / 100];
                var resto = numero % 100;
                return resto > 0 ? centena + " " + NumeroALetras(resto) : centena;
            }

            if (numero >= 30)
            {
                string[] decenas = { "", "", "", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };
                var decena = decenas[numero / 10];
                var unidad = numero % 10;
                return unidad > 0 ? decena + " Y " + NumeroALetras(unidad) : decena;
            }

            if (numero >= 20)
            {
                if (numero == 20) return "VEINTE";
                return "VEINTI" + NumeroALetras(numero - 20).ToLowerInvariant();
            }

            string[] especiales =
            {
                "", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE",
                "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISEIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE",
                "VEINTE", "VEINTIUN", "VEINTIDOS", "VEINTITRES", "VEINTICUATRO", "VEINTICINCO", "VEINTISEIS", "VEINTISIETE", "VEINTIOCHO", "VEINTINUEVE"
            };

            return especiales[numero];
        }

        private string ExtraerContenidoQr(string dataQr)
        {
            if (string.IsNullOrWhiteSpace(dataQr))
            {
                return string.Empty;
            }

            var match = Regex.Match(
                dataQr,
                @"QRCode:\s*(https?://\S+)",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);

            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return dataQr.Trim();
        }

        #endregion
    }
}
