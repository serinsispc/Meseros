using DAL;
using DAL.Controler;
using DAL.Funciones;
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.Services;
using RFacturacionElectronicaDIAN.Entities.Request;
using RFacturacionElectronicaDIAN.Entities.Response;
using RFacturacionElectronicaDIAN.Factories;
using Acquirer_Response = RFacturacionElectronicaDIAN.Entities.Response.Acquirer_Response;
using WebApplication.Class;
using WebApplication.Helpers;
using WebApplication.ViewModels;

namespace WebApplication
{
    public partial class HVentas : System.Web.UI.Page
    {
        protected List<V_TablaVentas> Ventas = new List<V_TablaVentas>();
        protected decimal TotalVentas;
        protected int CantidadFacturas;
        protected decimal TotalPendiente;
        protected int CantidadAnuladas;
        protected MenuViewModels Models = new MenuViewModels();
        protected string VentasDetalleJson = "{}";
        protected List<V_Resoluciones> Resoluciones = new List<V_Resoluciones>();
        protected List<Clientes> ClientesDisponibles = new List<Clientes>();
        private List<type_document_identifications> _tiposDocumento = new List<type_document_identifications>();

        protected async void Page_Load(object sender, EventArgs e)
        {
            Models = SessionContextHelper.LoadModels(Session) ?? new MenuViewModels();
            var db = Convert.ToString(Session[SessionContextHelper.DbKey] ?? Models.db);
            var idBase = SessionContextHelper.ResolveBaseCajaId(Session, Models);

            if (string.IsNullOrWhiteSpace(db) || idBase <= 0)
            {
                Response.Redirect("~/Default.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                await CargarResoluciones(db);
                await CargarCombosCliente(db);
            }

            await CargarClientes(db, !IsPostBack);

            if (IsPostBack)
            {
                var eventTarget = Request["__EVENTTARGET"];
                var eventArgument = Request["__EVENTARGUMENT"];

                if (string.Equals(eventTarget, "ImprimirFacturaHV", StringComparison.OrdinalIgnoreCase))
                {
                    await EncolarImpresionFactura(db, eventArgument);
                }
                else if (string.Equals(eventTarget, "ReenviarFacturaDianHV", StringComparison.OrdinalIgnoreCase))
                {
                    await ReenviarFacturaDian(db, eventArgument);
                }
                else if (string.Equals(eventTarget, "BuscarNitClienteHV", StringComparison.OrdinalIgnoreCase))
                {
                    await BuscarNitCliente(db, eventArgument);
                }
                else if (string.Equals(eventTarget, "GuardarClienteFacturaHV", StringComparison.OrdinalIgnoreCase))
                {
                    await GuardarClienteCatalogo(db, eventArgument);
                }
                else if (string.Equals(eventTarget, "SeleccionarClienteFacturaHV", StringComparison.OrdinalIgnoreCase))
                {
                    await SeleccionarClienteFactura(db, eventArgument);
                }
            }

            await CargarVentas(db, idBase);
        }

        private async System.Threading.Tasks.Task CargarResoluciones(string db)
        {
            var dal = new SqlAutoDAL();
            Resoluciones = await dal.ConsultarLista<V_Resoluciones>(db, x => x.estado == 1) ?? new List<V_Resoluciones>();
            Resoluciones = Resoluciones.OrderBy(x => x.nombreRosolucion).ThenBy(x => x.prefijo).ToList();

            if (ddlResolucionEditar != null)
            {
                ddlResolucionEditar.Items.Clear();
                ddlResolucionEditar.Items.Add(new System.Web.UI.WebControls.ListItem("Selecciona...", ""));
                foreach (var resolucion in Resoluciones)
                {
                    var texto = string.Join(" | ", new[]
                    {
                        string.IsNullOrWhiteSpace(resolucion.nombreRosolucion) ? "Resolucion" : resolucion.nombreRosolucion,
                        string.IsNullOrWhiteSpace(resolucion.prefijo) ? null : ("Prefijo " + resolucion.prefijo),
                        string.IsNullOrWhiteSpace(resolucion.numeroResolucion) ? null : ("No. " + resolucion.numeroResolucion)
                    }.Where(x => !string.IsNullOrWhiteSpace(x)));

                    ddlResolucionEditar.Items.Add(new System.Web.UI.WebControls.ListItem(texto, resolucion.idResolucion.ToString()));
                }
            }
        }

        private async System.Threading.Tasks.Task CargarClientes(string db, bool cargarCombo)
        {
            ClientesDisponibles = await ClientesControler.ListaClientes(db) ?? new List<Clientes>();
            ClientesDisponibles = ClientesDisponibles
                .OrderBy(x => string.IsNullOrWhiteSpace(x.nameCliente) ? "ZZZ" : x.nameCliente)
                .ThenBy(x => x.identificationNumber)
                .ToList();

            if (cargarCombo && ddlClienteEditar != null)
            {
                ddlClienteEditar.Items.Clear();
                ddlClienteEditar.Items.Add(new System.Web.UI.WebControls.ListItem("Selecciona...", ""));
                foreach (var cliente in ClientesDisponibles)
                {
                    var nombre = string.IsNullOrWhiteSpace(cliente.nameCliente) ? "Cliente" : cliente.nameCliente.Trim();
                    var nit = string.IsNullOrWhiteSpace(cliente.identificationNumber) ? "Sin NIT" : cliente.identificationNumber.Trim();
                    ddlClienteEditar.Items.Add(new System.Web.UI.WebControls.ListItem(string.Format("{0} | {1}", nombre, nit), cliente.id.ToString()));
                }
            }
        }

        private async System.Threading.Tasks.Task CargarCombosCliente(string db)
        {
            await CargarTiposDocumento(db);
            await CargarTiposOrganizacion(db);
            await CargarMunicipios(db);
            await CargarTiposRegimen(db);
            await CargarTiposResponsabilidad(db);
            await CargarDetallesImpuesto(db);
        }

        private async System.Threading.Tasks.Task CargarTiposDocumento(string db)
        {
            _tiposDocumento = await type_document_identificationsControler.ListaTiposDocumento(db) ?? new List<type_document_identifications>();
            if (ddlTipoDocumentoHv != null)
            {
                ddlTipoDocumentoHv.Items.Clear();
                ddlTipoDocumentoHv.Items.Add(new System.Web.UI.WebControls.ListItem("Seleccionar tipo", ""));
                ddlTipoDocumentoHv.DataSource = _tiposDocumento;
                ddlTipoDocumentoHv.DataTextField = "name";
                ddlTipoDocumentoHv.DataValueField = "id";
                ddlTipoDocumentoHv.DataBind();
            }
        }

        private async System.Threading.Tasks.Task CargarTiposOrganizacion(string db)
        {
            var tipos = await type_organizationsControler.ListaTiposOrganizacion(db) ?? new List<type_organizations>();
            if (ddlTipoOrganizacionHv != null)
            {
                ddlTipoOrganizacionHv.Items.Clear();
                ddlTipoOrganizacionHv.Items.Add(new System.Web.UI.WebControls.ListItem("Seleccionar tipo", ""));
                ddlTipoOrganizacionHv.DataSource = tipos;
                ddlTipoOrganizacionHv.DataTextField = "name";
                ddlTipoOrganizacionHv.DataValueField = "id";
                ddlTipoOrganizacionHv.DataBind();
            }
        }

        private async System.Threading.Tasks.Task CargarMunicipios(string db)
        {
            var municipios = await V_MunicipiosControler.ListaMunicipios(db) ?? new List<V_Municipios>();
            if (ddlMunicipioHv != null)
            {
                ddlMunicipioHv.Items.Clear();
                ddlMunicipioHv.Items.Add(new System.Web.UI.WebControls.ListItem("Seleccionar municipio", ""));
                ddlMunicipioHv.DataSource = municipios;
                ddlMunicipioHv.DataTextField = "name";
                ddlMunicipioHv.DataValueField = "idMunicipio";
                ddlMunicipioHv.DataBind();
            }
        }

        private async System.Threading.Tasks.Task CargarTiposRegimen(string db)
        {
            var tipos = await type_regimesControler.ListaTiposRegimen(db) ?? new List<type_regimes>();
            if (ddlTipoRegimenHv != null)
            {
                ddlTipoRegimenHv.Items.Clear();
                ddlTipoRegimenHv.Items.Add(new System.Web.UI.WebControls.ListItem("Seleccionar régimen", ""));
                ddlTipoRegimenHv.DataSource = tipos;
                ddlTipoRegimenHv.DataTextField = "name";
                ddlTipoRegimenHv.DataValueField = "id";
                ddlTipoRegimenHv.DataBind();
            }
        }

        private async System.Threading.Tasks.Task CargarTiposResponsabilidad(string db)
        {
            var tipos = await type_liabilitiesControler.ListaTiposResponsabilidad(db) ?? new List<type_liabilities>();
            if (ddlTipoResponsabilidadHv != null)
            {
                ddlTipoResponsabilidadHv.Items.Clear();
                ddlTipoResponsabilidadHv.Items.Add(new System.Web.UI.WebControls.ListItem("Seleccionar responsabilidad", ""));
                ddlTipoResponsabilidadHv.DataSource = tipos;
                ddlTipoResponsabilidadHv.DataTextField = "name";
                ddlTipoResponsabilidadHv.DataValueField = "id";
                ddlTipoResponsabilidadHv.DataBind();
            }
        }

        private async System.Threading.Tasks.Task CargarDetallesImpuesto(string db)
        {
            var detalles = await tax_detailsControler.ListaDetallesImpuesto(db) ?? new List<tax_details>();
            if (ddlDetalleImpuestoHv != null)
            {
                ddlDetalleImpuestoHv.Items.Clear();
                ddlDetalleImpuestoHv.Items.Add(new System.Web.UI.WebControls.ListItem("Seleccionar impuesto", ""));
                ddlDetalleImpuestoHv.DataSource = detalles;
                ddlDetalleImpuestoHv.DataTextField = "name";
                ddlDetalleImpuestoHv.DataValueField = "id";
                ddlDetalleImpuestoHv.DataBind();
            }
        }

        protected string NombreTipoDocumentoCliente(int idTipoDocumento)
        {
            var item = (_tiposDocumento ?? new List<type_document_identifications>()).FirstOrDefault(x => x != null && x.id == idTipoDocumento);
            return item?.name ?? idTipoDocumento.ToString();
        }
        private async System.Threading.Tasks.Task CargarVentas(string db, int idBase)
        {
            var dal = new SqlAutoDAL();
            Ventas = await dal.ConsultarLista<V_TablaVentas>(db, x => x.idBaseCaja == idBase && x.eliminada == false) ?? new List<V_TablaVentas>();
            Ventas = Ventas.OrderByDescending(x => x.fechaVenta).ToList();

            TotalVentas = Ventas.Where(x => !EsVentaAnulada(x)).Sum(x => x.total_A_Pagar);
            CantidadFacturas = Ventas.Count(x => x.numeroVenta > 0);
            TotalPendiente = Ventas.Where(x => !EsVentaAnulada(x)).Sum(x => x.totalPendienteVenta);
            CantidadAnuladas = Ventas.Count(EsVentaAnulada);

            var detalleMap = new Dictionary<int, object>();
            foreach (var venta in Ventas)
            {
                var detalles = await V_DetalleCajaControler.Lista_IdVenta(db, venta.id, 0) ?? new List<V_DetalleCaja>();
                detalleMap[venta.id] = new
                {
                    id = venta.id,
                    factura = FacturaLabel(venta),
                    cliente = string.IsNullOrWhiteSpace(venta.nombreCliente) ? "Cliente contado" : venta.nombreCliente,
                    nit = string.IsNullOrWhiteSpace(venta.nit) ? "0" : venta.nit,
                    fecha = FechaLarga(venta.fechaVenta),
                    hora = HoraLarga(venta.fechaVenta),
                    alias = string.IsNullOrWhiteSpace(venta.aliasVenta) ? "Sin alias" : venta.aliasVenta,
                    medioPago = string.IsNullOrWhiteSpace(venta.medioDePago) ? "Sin definir" : venta.medioDePago,
                    estado = EstadoTexto(venta),
                    total = venta.total_A_Pagar,
                    pagado = venta.totalPagadoVenta,
                    pendiente = venta.totalPendienteVenta,
                    observacion = string.IsNullOrWhiteSpace(venta.observacionVenta) ? "Sin observacion" : venta.observacionVenta,
                    detalles = detalles.Select(d => new
                    {
                        producto = string.IsNullOrWhiteSpace(d.nombreProducto) ? "Producto" : d.nombreProducto,
                        presentacion = string.IsNullOrWhiteSpace(d.presentacion) ? string.Empty : d.presentacion,
                        cantidad = d.unidad,
                        precio = d.precioVenta,
                        total = d.totalDetalle,
                        nota = string.IsNullOrWhiteSpace(d.adiciones) ? (string.IsNullOrWhiteSpace(d.observacion) ? "Sin nota" : d.observacion) : d.adiciones,
                        cuenta = string.IsNullOrWhiteSpace(d.nombreCuenta) ? "General" : d.nombreCuenta
                    }).ToList()
                };
            }

            VentasDetalleJson = JsonConvert.SerializeObject(detalleMap).Replace("</", "<\\/");
        }

        private async System.Threading.Tasks.Task EncolarImpresionFactura(string db, string eventArgument)
        {
            if (!int.TryParse((eventArgument ?? string.Empty).Trim(), out int idVenta) || idVenta <= 0)
            {
                AlertModerno.Warning(this, "Atención", "No se recibió una venta válida para imprimir.", true, 1800);
                return;
            }

            var printer = new ImprimirFactura { id = 0, idventa = idVenta };
            var ordenPrinter = await ImprimirFacturaControler.CRUD(db, printer, 0);

            if (ordenPrinter == null || !ordenPrinter.estado)
            {
                AlertModerno.Error(this, "Error", "La factura no se pudo enviar a impresión.", true, 2200);
                return;
            }

            AlertModerno.Success(this, "OK", "Factura enviada a impresión correctamente.", true, 1200);
        }

        private async System.Threading.Tasks.Task ReenviarFacturaDian(string db, string eventArgument)
        {
            if (!int.TryParse((eventArgument ?? string.Empty).Trim(), out int idVenta) || idVenta <= 0)
            {
                AlertModerno.Warning(this, "Atención", "No se recibió una venta válida para reenviar a la DIAN.", true, 1800);
                return;
            }

            var venta = await V_TablaVentasControler.Consultar_Id(db, idVenta);
            if (venta == null)
            {
                AlertModerno.Error(this, "Error", "No se encontró la venta seleccionada.", true, 1800);
                return;
            }

            if (!PuedeReenviarDian(venta))
            {
                AlertModerno.Warning(this, "Atención", "Solo puedes reenviar a la DIAN facturas electrónicas que no han sido aceptadas.", true, 2200);
                return;
            }

            if (venta.idCliente <= 0)
            {
                AlertModerno.Warning(this, "Atención", "La venta no tiene cliente asignado. Actualiza el cliente antes de reenviar.", true, 2200);
                return;
            }

            if (venta.idResolucion <= 0)
            {
                AlertModerno.Warning(this, "Atención", "La venta no tiene resolución asignada. Actualiza la resolución antes de reenviar.", true, 2200);
                return;
            }

            var tokenFe = Session[SessionContextHelper.TokenFeKey]?.ToString() ?? Models?.TokenEmpresa ?? string.Empty;
            if (string.IsNullOrWhiteSpace(tokenFe))
            {
                AlertModerno.Warning(this, "Atención", "No se encontró el token de facturación electrónica para reenviar la factura.", true, 2200);
                return;
            }

            var detalles = await V_DetalleCajaControler.Lista_IdVenta(db, idVenta, 0) ?? new List<V_DetalleCaja>();
            if (!detalles.Any())
            {
                AlertModerno.Warning(this, "Atención", "La venta no tiene detalles para reenviar a la DIAN.", true, 2200);
                return;
            }

            var respFe = await ClassFE.FacturaElectronica(db, venta, detalles, tokenFe);
            if (!respFe)
            {
                AlertModerno.Error(this, "Error", "La factura electrónica no fue reenviada a la DIAN.", true, 2200);
                return;
            }

            var urlRefresco = Request?.RawUrl;
            if (string.IsNullOrWhiteSpace(urlRefresco))
            {
                urlRefresco = ResolveUrl("~/HVentas.aspx");
            }

            AlertModerno.SuccessGoTo(this, "OK", "Factura reenviada a la DIAN correctamente.", urlRefresco, true, 1200);
        }

        private async System.Threading.Tasks.Task SeleccionarClienteFactura(string db, string eventArgument)
        {
            if (!int.TryParse((hfVentaClienteId?.Value ?? string.Empty).Trim(), out var idVenta) || idVenta <= 0)
            {
                AlertModerno.Warning(this, "Atención", "No se recibió una venta válida para relacionar el cliente.", true, 1800);
                return;
            }

            if (!int.TryParse((eventArgument ?? hfClienteEditarSeleccionadoId?.Value ?? string.Empty).Trim(), out var idCliente) || idCliente <= 0)
            {
                AlertModerno.Warning(this, "Atención", "Debes seleccionar un cliente antes de continuar.", true, 2000);
                AbrirModalCliente(idVenta, 0);
                return;
            }

            var vistaVenta = await V_TablaVentasControler.Consultar_Id(db, idVenta);
            if (vistaVenta == null)
            {
                AlertModerno.Error(this, "Error", "No se encontró la venta seleccionada.", true, 1800);
                return;
            }

            if (!PuedeEditarCliente(vistaVenta))
            {
                AlertModerno.Warning(this, "Atención", "Solo puedes editar el cliente en ventas que no han sido aceptadas por la DIAN.", true, 2200);
                return;
            }

            var cliente = await ClientesControler.Consultar_id(db, idCliente);
            if (cliente == null)
            {
                AlertModerno.Error(this, "Error", "No se encontró el cliente seleccionado.", true, 1800);
                AbrirModalCliente(idVenta, 0);
                return;
            }

            var relacion = await R_VentaCliente_Controler.ConsultarRelacion(db, idVenta);
            var funcion = 1;
            if (relacion == null)
            {
                relacion = new R_VentaCliente
                {
                    id = 0,
                    idVenta = idVenta,
                    idCliente = idCliente,
                    idSede = vistaVenta.IdSede
                };
                funcion = 0;
            }
            else
            {
                relacion.idCliente = idCliente;
                if (relacion.idSede <= 0)
                {
                    relacion.idSede = vistaVenta.IdSede;
                }
            }

            var ok = await R_VentaCliente_Controler.CRUD(db, relacion, funcion);
            if (!ok)
            {
                AlertModerno.Error(this, "Error", "No fue posible actualizar el cliente de la factura.", true, 2200);
                AbrirModalCliente(idVenta, idCliente);
                return;
            }

            var urlRefresco = Request?.RawUrl;
            if (string.IsNullOrWhiteSpace(urlRefresco))
            {
                urlRefresco = ResolveUrl("~/HVentas.aspx");
            }

            AlertModerno.SuccessGoTo(this, "OK", "Cliente actualizado correctamente.", urlRefresco, true, 1200);
        }

        private async System.Threading.Tasks.Task GuardarClienteCatalogo(string db, string eventArgument)
        {
            if (string.IsNullOrWhiteSpace(eventArgument))
            {
                AlertModerno.Warning(this, "Atención", "No llegó información del cliente.", true, 2000);
                return;
            }

            string json;
            try
            {
                var bytes = Convert.FromBase64String(eventArgument);
                json = System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                json = eventArgument;
            }

            var payload = JsonConvert.DeserializeObject<ClienteGuardarPayloadHV>(json);
            if (payload == null)
            {
                AlertModerno.Error(this, "Error", "Payload inválido para guardar el cliente.", true);
                return;
            }

            var nit = (payload.nit ?? string.Empty).Trim();
            var nombre = (payload.nombre ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(nit) || string.IsNullOrWhiteSpace(nombre))
            {
                AlertModerno.Warning(this, "Atención", "Debes completar identificación y nombre del cliente.", true, 2200);
                ReabrirModalClienteConDatos(payload.idVenta, payload);
                return;
            }

            int funcion = 0;
            var existente = (payload.clienteId > 0)
                ? ClientesDisponibles.FirstOrDefault(x => x.id == payload.clienteId)
                : ClientesDisponibles.FirstOrDefault(x => (x.identificationNumber ?? string.Empty) == nit);

            var cliente = existente ?? new Clientes();
            if (existente != null)
            {
                funcion = 1;
            }

            cliente.typeDocumentIdentification_id = payload.typeDocId;
            cliente.identificationNumber = nit;
            cliente.typeOrganization_id = payload.orgId;
            cliente.municipality_id = payload.municipioId;
            cliente.typeRegime_id = payload.regimenId;
            cliente.typeLiability_id = payload.responsabilidadId;
            cliente.typeTaxDetail_id = payload.impuestoId;
            cliente.nameCliente = nombre;
            cliente.tradeName = string.IsNullOrWhiteSpace(payload.comercio) ? "-" : payload.comercio.Trim();
            cliente.phone = string.IsNullOrWhiteSpace(payload.telefono) ? "0" : payload.telefono.Trim();
            cliente.adress = string.IsNullOrWhiteSpace(payload.direccion) ? "-" : payload.direccion.Trim();
            cliente.email = string.IsNullOrWhiteSpace(payload.correo) ? string.Empty : payload.correo.Trim();
            cliente.merchantRegistration = string.IsNullOrWhiteSpace(payload.matricula) ? "0" : payload.matricula.Trim();
            cliente.idTipoTercero = payload.esCliente && payload.esProveedor ? 3 : (payload.esProveedor ? 2 : 1);

            var respuesta = await ClientesControler.CRUD(db, cliente, funcion);
            if (respuesta == null || !respuesta.estado)
            {
                AlertModerno.Error(this, "Error", respuesta?.mensaje ?? "No fue posible guardar el cliente.", true);
                ReabrirModalClienteConDatos(payload.idVenta, payload);
                return;
            }

            int idClienteGuardado = existente?.id ?? 0;
            if (idClienteGuardado <= 0 && respuesta.data != null)
            {
                int.TryParse(respuesta.data.ToString(), out idClienteGuardado);
            }

            await CargarClientes(db, true);

            if (idClienteGuardado <= 0)
            {
                var recargado = ClientesDisponibles.FirstOrDefault(x => (x.identificationNumber ?? string.Empty) == nit);
                idClienteGuardado = recargado?.id ?? 0;
            }

            if (idClienteGuardado <= 0)
            {
                AlertModerno.Warning(this, "Atención", "El cliente se guardó, pero no fue posible seleccionarlo automáticamente.", true, 2200);
                ReabrirModalClienteConDatos(payload.idVenta, payload);
                return;
            }

            await SeleccionarClienteFactura(db, idClienteGuardado.ToString());
        }

        [WebMethod(EnableSession = true)]
        public static string BuscarNitClienteAjax(string nit)
        {
            try
            {
                nit = (nit ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(nit))
                {
                    return JsonConvert.SerializeObject(ClienteBusquedaAjaxResponse.Fail("Debes escribir un documento para buscar.", ClienteBusquedaAjaxData.Empty(nit)));
                }

                var session = HttpContext.Current == null ? null : HttpContext.Current.Session;
                var models = session == null ? null : SessionContextHelper.LoadModels(session);
                var db = Convert.ToString((session == null ? null : session[SessionContextHelper.DbKey]) ?? models?.db ?? string.Empty);
                if (string.IsNullOrWhiteSpace(db))
                {
                    return JsonConvert.SerializeObject(ClienteBusquedaAjaxResponse.Fail("La sesión expiró o no tiene contexto de base de datos.", ClienteBusquedaAjaxData.Empty(nit)));
                }
                var clientes = ClientesControler.ListaClientes(db).GetAwaiter().GetResult() ?? new List<Clientes>();
                var cliente = clientes.FirstOrDefault(x => (x.identificationNumber ?? string.Empty) == nit);
                var encontradoEnBase = cliente != null;

                if (cliente == null)
                {
                    int nitNumero;
                    if (!int.TryParse(nit, out nitNumero))
                    {
                        return JsonConvert.SerializeObject(ClienteBusquedaAjaxResponse.Fail("El documento debe ser numérico para consultar en la DIAN.", ClienteBusquedaAjaxData.Empty(nit)));
                    }

                    var acquirerResponse = ConsultarNitDianStatic(nitNumero);
                    if (acquirerResponse == null || string.IsNullOrWhiteSpace(acquirerResponse.name))
                    {
                        return JsonConvert.SerializeObject(ClienteBusquedaAjaxResponse.Fail("El documento no se encuentra registrado ni en la base de datos ni en la DIAN. Debes crearlo manualmente.", ClienteBusquedaAjaxData.Empty(nit)));
                    }

                    cliente = new Clientes
                    {
                        id = 0,
                        typeDocumentIdentification_id = 6,
                        identificationNumber = nit,
                        typeOrganization_id = 2,
                        municipality_id = 605,
                        typeRegime_id = 2,
                        typeLiability_id = 29,
                        typeTaxDetail_id = 5,
                        nameCliente = acquirerResponse.name,
                        tradeName = "-",
                        phone = "0",
                        adress = "-",
                        email = acquirerResponse.email,
                        merchantRegistration = "0",
                        idTipoTercero = 1
                    };
                }

                return JsonConvert.SerializeObject(ClienteBusquedaAjaxResponse.Ok(ClienteBusquedaAjaxData.FromCliente(cliente, encontradoEnBase)));
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(ClienteBusquedaAjaxResponse.Fail("No fue posible consultar el NIT. " + ex.Message, ClienteBusquedaAjaxData.Empty(nit)));
            }
        }

        private static Acquirer_Response ConsultarNitDianStatic(int nit)
        {
            var facturacionElectronica = new FacturacionElectronicaDIANFactory();
            var acquirerRequest = new Acquirer_Request();
            acquirerRequest.environment = new Acquirer_Request.Environment();
            acquirerRequest.environment.type_environment_id = 1;
            acquirerRequest.type_document_identification_id = 6;
            acquirerRequest.identification_number = nit;
            string tokenFe = controlador_tokenEmpresa.ConsultarTokenSerinsisPC().GetAwaiter().GetResult();
            return facturacionElectronica.ConsultarAcquirer(acquirerRequest, tokenFe).GetAwaiter().GetResult();
        }
        private async System.Threading.Tasks.Task BuscarNitCliente(string db, string nit)
        {
            try
            {
                nit = (nit ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(nit))
                {
                    return;
                }

                var cliente = ClientesDisponibles.FirstOrDefault(x => (x.identificationNumber ?? string.Empty) == nit);
                var encontradoEnBase = cliente != null;

                if (cliente == null)
                {
                    var acquirerResponse = await Consultar_NIT_DIAN(Convert.ToInt32(nit));
                    if (acquirerResponse == null || string.IsNullOrWhiteSpace(acquirerResponse.name))
                    {
                        var limpiarPayload = new
                        {
                            clienteId = 0,
                            typeDocId = "",
                            nit = nit,
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
                        var scriptError = "Swal.fire({icon:'error',title:'Error',text:'El documento no se encuentra registrado ni en la base de datos ni en la DIAN. Debes crearlo manualmente.',confirmButtonColor:'#2563eb'}).then(function(){if(window.setHvClienteData){window.setHvClienteData(" + limpiarJson + ");} if(window.hvClienteModal){window.hvClienteModal.preseleccionar();} var modalEl=document.getElementById('mdlClienteVenta'); if(window.hvMostrarModalCliente){window.hvMostrarModalCliente();} else if(modalEl && window.bootstrap && window.bootstrap.Modal){window.bootstrap.Modal.getOrCreateInstance(modalEl).show();}});";
                        ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString("N"), scriptError, true);
                        return;
                    }

                    cliente = new Clientes
                    {
                        id = 0,
                        typeDocumentIdentification_id = 6,
                        identificationNumber = nit,
                        typeOrganization_id = 2,
                        municipality_id = 605,
                        typeRegime_id = 2,
                        typeLiability_id = 29,
                        typeTaxDetail_id = 5,
                        nameCliente = acquirerResponse.name,
                        tradeName = "-",
                        phone = "0",
                        adress = "-",
                        email = acquirerResponse.email,
                        merchantRegistration = "0"
                    };
                }

                var payload = new
                {
                    clienteId = cliente.id,
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
                    matricula = cliente.merchantRegistration,
                    actionLabel = encontradoEnBase ? "Editar" : "Guardar"
                };

                var json = JsonConvert.SerializeObject(payload);
                var script = "if(window.setHvClienteData){window.setHvClienteData(" + json + ");} if(window.hvClienteModal){window.hvClienteModal.preseleccionar();} var modalEl=document.getElementById('mdlClienteVenta'); if(window.hvMostrarModalCliente){window.hvMostrarModalCliente();} else if(modalEl && window.bootstrap && window.bootstrap.Modal){window.bootstrap.Modal.getOrCreateInstance(modalEl).show();}";
                ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString("N"), script, true);
            }
            catch
            {
                var scriptError = "Swal.fire({icon:'error',title:'Error',text:'No fue posible consultar el NIT.',confirmButtonColor:'#2563eb'}).then(function(){var modalEl=document.getElementById('mdlClienteVenta'); if(window.hvMostrarModalCliente){window.hvMostrarModalCliente();} else if(modalEl && window.bootstrap && window.bootstrap.Modal){window.bootstrap.Modal.getOrCreateInstance(modalEl).show();}});";
                ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString("N"), scriptError, true);
            }
        }

        private async System.Threading.Tasks.Task<Acquirer_Response> Consultar_NIT_DIAN(int nit)
        {
            FacturacionElectronicaDIANFactory facturacionElectronica = new FacturacionElectronicaDIANFactory();
            Acquirer_Request acquirerRequest = new Acquirer_Request();
            acquirerRequest.environment = new Acquirer_Request.Environment();
            acquirerRequest.environment.type_environment_id = 1;
            acquirerRequest.type_document_identification_id = 6;
            acquirerRequest.identification_number = nit;
            string tokenFe = await controlador_tokenEmpresa.ConsultarTokenSerinsisPC();
            return await facturacionElectronica.ConsultarAcquirer(acquirerRequest, tokenFe);
        }

        public class ClienteBusquedaAjaxResponse
        {
            public bool estado { get; set; }
            public string mensaje { get; set; }
            public ClienteBusquedaAjaxData data { get; set; }

            public static ClienteBusquedaAjaxResponse Ok(ClienteBusquedaAjaxData data)
            {
                return new ClienteBusquedaAjaxResponse { estado = true, mensaje = string.Empty, data = data };
            }

            public static ClienteBusquedaAjaxResponse Fail(string mensaje, ClienteBusquedaAjaxData data)
            {
                return new ClienteBusquedaAjaxResponse { estado = false, mensaje = mensaje ?? string.Empty, data = data };
            }
        }

        public class ClienteBusquedaAjaxData
        {
            public int clienteId { get; set; }
            public int typeDocId { get; set; }
            public string nit { get; set; }
            public int orgId { get; set; }
            public int municipioId { get; set; }
            public int regimenId { get; set; }
            public int responsabilidadId { get; set; }
            public int impuestoId { get; set; }
            public string nombre { get; set; }
            public string comercio { get; set; }
            public string telefono { get; set; }
            public string direccion { get; set; }
            public string correo { get; set; }
            public string matricula { get; set; }
            public bool esCliente { get; set; }
            public bool esProveedor { get; set; }
            public string actionLabel { get; set; }

            public static ClienteBusquedaAjaxData Empty(string nit)
            {
                return new ClienteBusquedaAjaxData
                {
                    clienteId = 0,
                    typeDocId = 0,
                    nit = nit ?? string.Empty,
                    orgId = 0,
                    municipioId = 0,
                    regimenId = 0,
                    responsabilidadId = 0,
                    impuestoId = 0,
                    nombre = string.Empty,
                    comercio = string.Empty,
                    telefono = string.Empty,
                    direccion = string.Empty,
                    correo = string.Empty,
                    matricula = string.Empty,
                    esCliente = true,
                    esProveedor = false,
                    actionLabel = "Guardar"
                };
            }

            public static ClienteBusquedaAjaxData FromCliente(Clientes cliente, bool encontradoEnBase)
            {
                cliente = cliente ?? new Clientes();
                return new ClienteBusquedaAjaxData
                {
                    clienteId = cliente.id,
                    typeDocId = cliente.typeDocumentIdentification_id,
                    nit = cliente.identificationNumber ?? string.Empty,
                    orgId = cliente.typeOrganization_id,
                    municipioId = cliente.municipality_id,
                    regimenId = cliente.typeRegime_id,
                    responsabilidadId = cliente.typeLiability_id,
                    impuestoId = cliente.typeTaxDetail_id,
                    nombre = cliente.nameCliente ?? string.Empty,
                    comercio = cliente.tradeName ?? string.Empty,
                    telefono = cliente.phone ?? string.Empty,
                    direccion = cliente.adress ?? string.Empty,
                    correo = cliente.email ?? string.Empty,
                    matricula = cliente.merchantRegistration ?? string.Empty,
                    esCliente = cliente.idTipoTercero == 1 || cliente.idTipoTercero == 3 || cliente.idTipoTercero == 0,
                    esProveedor = cliente.idTipoTercero == 2 || cliente.idTipoTercero == 3,
                    actionLabel = encontradoEnBase ? "Editar" : "Guardar"
                };
            }
        }
        private class ClienteGuardarPayloadHV
        {
            public int idVenta { get; set; }
            public int clienteId { get; set; }
            public int typeDocId { get; set; }
            public string nit { get; set; }
            public int orgId { get; set; }
            public int municipioId { get; set; }
            public int regimenId { get; set; }
            public int responsabilidadId { get; set; }
            public int impuestoId { get; set; }
            public string nombre { get; set; }
            public string comercio { get; set; }
            public string telefono { get; set; }
            public string direccion { get; set; }
            public string correo { get; set; }
            public string matricula { get; set; }
            public bool esCliente { get; set; }
            public bool esProveedor { get; set; }
        }
        protected string Moneda(decimal valor)
        {
            return valor.ToString("C0");
        }

        protected string FechaLarga(DateTime fecha)
        {
            return fecha.ToString("yyyy-MM-dd");
        }

        protected string HoraLarga(DateTime fecha)
        {
            return fecha.ToString("hh:mm tt");
        }

        protected string FacturaLabel(V_TablaVentas venta)
        {
            if (!string.IsNullOrWhiteSpace(venta.prefijo) && venta.numeroVenta > 0)
            {
                return venta.prefijo + "-" + venta.numeroVenta.ToString("000000");
            }
            if (venta.numeroVenta > 0)
            {
                return venta.numeroVenta.ToString();
            }
            return "Sin numerar";
        }

        protected string EstadoClase(V_TablaVentas venta)
        {
            if (EsVentaAnulada(venta)) return "danger";
            if (venta.totalPendienteVenta > 0) return "warning";
            return "success";
        }

        protected string EstadoTexto(V_TablaVentas venta)
        {
            if (!string.IsNullOrWhiteSpace(venta?.estadoVenta))
            {
                return venta.estadoVenta;
            }

            if (venta != null && venta.totalPendienteVenta > 0) return "PENDIENTE";
            return "PAGADA";
        }

        protected string MedioClase(string medio)
        {
            return string.IsNullOrWhiteSpace(medio) ? "gray" : "info";
        }

        protected string FeClase(V_TablaVentas venta)
        {
            return string.IsNullOrWhiteSpace(venta?.cufe) && string.IsNullOrWhiteSpace(venta?.estadoFE) ? "gray" : "success";
        }

        protected string FeTexto(V_TablaVentas venta)
        {
            if (string.IsNullOrWhiteSpace(venta?.cufe) && string.IsNullOrWhiteSpace(venta?.estadoFE))
            {
                return "No aplica";
            }

            return string.IsNullOrWhiteSpace(venta?.estadoFE) ? "Emitida" : venta.estadoFE;
        }

        protected string FeIcono(V_TablaVentas venta)
        {
            return string.IsNullOrWhiteSpace(venta?.cufe) && string.IsNullOrWhiteSpace(venta?.estadoFE)
                ? "bi-dash-circle"
                : "bi-qr-code-scan";
        }

        protected bool PuedeEditarResolucion(V_TablaVentas venta)
        {
            if (venta == null)
            {
                return false;
            }

            var estadoFe = (venta.estadoFE ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(estadoFe))
            {
                return true;
            }

            return !string.Equals(estadoFe, "ACEPTADA", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(estadoFe, "ACEPTADO", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(estadoFe, "APROBADA", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(estadoFe, "VALIDADA", StringComparison.OrdinalIgnoreCase);
        }

        protected bool PuedeEditarCliente(V_TablaVentas venta)
        {
            return PuedeEditarResolucion(venta);
        }

        protected bool PuedeReenviarDian(V_TablaVentas venta)
        {
            return EsFacturaElectronica(venta) && PuedeEditarResolucion(venta);
        }

        protected bool EsFacturaElectronica(V_TablaVentas venta)
        {
            var tipoFactura = (venta?.tipoFactura ?? string.Empty).Trim();
            return tipoFactura.IndexOf("ELECTR", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        protected string VentaBusqueda(V_TablaVentas venta)
        {
            return string.Join(" ", new[]
            {
                FacturaLabel(venta),
                venta.aliasVenta,
                venta.nombreCliente,
                venta.nit,
                venta.numeroReferenciaPago,
                venta.medioDePago,
                venta.estadoVenta
            }.Where(x => !string.IsNullOrWhiteSpace(x))).ToLowerInvariant();
        }

        protected bool EsVentaAnulada(V_TablaVentas venta)
        {
            return string.Equals(venta?.estadoVenta, "ANULADA", StringComparison.OrdinalIgnoreCase);
        }

        protected async void btnGuardarResolucion_Click(object sender, EventArgs e)
        {
            Models = SessionContextHelper.LoadModels(Session) ?? new MenuViewModels();
            var db = Convert.ToString(Session[SessionContextHelper.DbKey] ?? Models.db);
            var idBase = SessionContextHelper.ResolveBaseCajaId(Session, Models);

            if (string.IsNullOrWhiteSpace(db) || idBase <= 0)
            {
                AlertModerno.ErrorRedirect(this, "Error", "La sesion expiro o no contiene el contexto activo.", "Default.aspx");
                return;
            }

            if (!int.TryParse((hfVentaResolucionId?.Value ?? string.Empty).Trim(), out var idVenta) || idVenta <= 0)
            {
                AlertModerno.Warning(this, "Atención", "No se recibió una venta válida para editar la resolución.", true, 1800);
                return;
            }

            if (!int.TryParse((ddlResolucionEditar?.SelectedValue ?? string.Empty).Trim(), out var idResolucion) || idResolucion <= 0)
            {
                AlertModerno.Warning(this, "Atención", "Seleccione una resolución válida.", true, 1800);
                AbrirModalResolucion(idVenta, 0);
                return;
            }

            var venta = await TablaVentasControler.ConsultarIdVenta(db, idVenta);
            if (venta == null)
            {
                AlertModerno.Error(this, "Error", "No se encontró la venta seleccionada.", true, 1800);
                return;
            }

            var vistaVenta = await new SqlAutoDAL().ConsultarUno<V_TablaVentas>(db, x => x.id == idVenta);
            if (!PuedeEditarResolucion(vistaVenta))
            {
                AlertModerno.Warning(this, "Atención", "Solo puedes editar la resolución en ventas que no han sido aceptadas por la DIAN.", true, 2200);
                await CargarVentas(db, idBase);
                return;
            }

            venta.idResolucion = idResolucion;
            var resp = await TablaVentasControler.CRUD(db, venta, 1);
            if (resp == null || !resp.estado)
            {
                AlertModerno.Error(this, "Error", resp?.mensaje ?? "No fue posible actualizar la resolución.", true, 2200);
                AbrirModalResolucion(idVenta, idResolucion);
                return;
            }

            var urlRefresco = Request?.RawUrl;
            if (string.IsNullOrWhiteSpace(urlRefresco))
            {
                urlRefresco = ResolveUrl("~/HVentas.aspx");
            }

            AlertModerno.SuccessGoTo(this, "OK", "Resolución actualizada correctamente.", urlRefresco, true, 1200);
        }

        protected async void btnGuardarCliente_Click(object sender, EventArgs e)
        {
            Models = SessionContextHelper.LoadModels(Session) ?? new MenuViewModels();
            var db = Convert.ToString(Session[SessionContextHelper.DbKey] ?? Models.db);
            var idBase = SessionContextHelper.ResolveBaseCajaId(Session, Models);

            if (string.IsNullOrWhiteSpace(db) || idBase <= 0)
            {
                AlertModerno.ErrorRedirect(this, "Error", "La sesion expiro o no contiene el contexto activo.", "Default.aspx");
                return;
            }

            if (!int.TryParse((hfVentaClienteId?.Value ?? string.Empty).Trim(), out var idVenta) || idVenta <= 0)
            {
                AlertModerno.Warning(this, "Atención", "No se recibió una venta válida para editar el cliente.", true, 1800);
                return;
            }

            var clienteSeleccionadoRaw = (hfClienteEditarSeleccionadoId?.Value ?? ddlClienteEditar?.SelectedValue ?? string.Empty).Trim();
            if (!int.TryParse(clienteSeleccionadoRaw, out var idCliente) || idCliente <= 0)
            {
                AlertModerno.Warning(this, "Atención", "Seleccione un cliente válido.", true, 1800);
                AbrirModalCliente(idVenta, 0);
                return;
            }

            var vistaVenta = await V_TablaVentasControler.Consultar_Id(db, idVenta);
            if (vistaVenta == null)
            {
                AlertModerno.Error(this, "Error", "No se encontró la venta seleccionada.", true, 1800);
                return;
            }

            if (!PuedeEditarCliente(vistaVenta))
            {
                AlertModerno.Warning(this, "Atención", "Solo puedes editar el cliente en ventas que no han sido aceptadas por la DIAN.", true, 2200);
                return;
            }

            var cliente = await ClientesControler.Consultar_id(db, idCliente);
            if (cliente == null)
            {
                AlertModerno.Error(this, "Error", "No se encontró el cliente seleccionado.", true, 1800);
                AbrirModalCliente(idVenta, 0);
                return;
            }

            var relacion = await R_VentaCliente_Controler.ConsultarRelacion(db, idVenta);
            var funcion = 1;
            if (relacion == null)
            {
                relacion = new R_VentaCliente
                {
                    id = 0,
                    idVenta = idVenta,
                    idCliente = idCliente,
                    idSede = vistaVenta.IdSede
                };
                funcion = 0;
            }
            else
            {
                relacion.idCliente = idCliente;
                if (relacion.idSede <= 0)
                {
                    relacion.idSede = vistaVenta.IdSede;
                }
            }

            var ok = await R_VentaCliente_Controler.CRUD(db, relacion, funcion);
            if (!ok)
            {
                AlertModerno.Error(this, "Error", "No fue posible actualizar el cliente de la factura.", true, 2200);
                AbrirModalCliente(idVenta, idCliente);
                return;
            }

            var urlRefresco = Request?.RawUrl;
            if (string.IsNullOrWhiteSpace(urlRefresco))
            {
                urlRefresco = ResolveUrl("~/HVentas.aspx");
            }

            AlertModerno.SuccessGoTo(this, "OK", "Cliente actualizado correctamente.", urlRefresco, true, 1200);
        }

        private void ReabrirModalClienteConDatos(int idVenta, object payload)
        {
            var json = JsonConvert.SerializeObject(payload ?? new { });
            var script = "setTimeout(function(){ if (typeof hvEditarCliente === 'function') { hvEditarCliente(" + idVenta + ", 0); } if (window.setHvClienteData) { window.setHvClienteData(" + json + "); } }, 120);";
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString("N"), script, true);
        }

        private void AbrirModalResolucion(int idVenta, int idResolucion)
        {
            var script = string.Format("setTimeout(function(){{ if (typeof hvEditarResolucion === 'function') {{ hvEditarResolucion({0}, {1}); }} }}, 120);", idVenta, idResolucion);
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString("N"), script, true);
        }

        private void AbrirModalCliente(int idVenta, int idCliente)
        {
            var script = string.Format("setTimeout(function(){{ if (typeof hvEditarCliente === 'function') {{ hvEditarCliente({0}, {1}); }} }}, 120);", idVenta, idCliente);
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString("N"), script, true);
        }
    }
}

















