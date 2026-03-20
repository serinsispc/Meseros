using DAL.Controler;
using DAL.Funciones;
using DAL.Model;
using Newtonsoft.Json;
using RFacturacionElectronicaDIAN.Entities.Request;
using RFacturacionElectronicaDIAN.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication.ViewModels;
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

            valores = new Valores
            {
                total = listaventas?.Sum(x => x.total_A_Pagar) ?? 0,
                propina = listaventas?.Sum(x => x.propina) ?? 0,
                facturas = listaventas?.Count(x => x.estadoVenta != "ANULADA") ?? 0,
                anuladas = listaventas?.Count(x => x.estadoVenta == "ANULADA") ?? 0
            };

            rpVentas.DataSource = listaventas;
            rpVentas.DataBind();
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
        private async Task btnDescargarPDF(string cufe)
        {
            if (string.IsNullOrWhiteSpace(cufe) || cufe == "--")
            {
                await MostrarMensaje(TipoMensaje.Error, "Error", "No hay PDF disponible.");
                return;
            }

            string link = $"https://catalogo-vpfe.dian.gov.co/User/SearchDocument?DocumentKey={cufe}";
            string script = $"window.open('{link}', '_blank');";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "AbrirPDF", script, true);
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

        #endregion
    }
}