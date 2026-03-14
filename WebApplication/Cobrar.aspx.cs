using DAL;
using DAL.Controler;
using DAL.Funciones;
using DAL.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RFacturacionElectronicaDIAN.Entities.Request;
using RFacturacionElectronicaDIAN.Entities.Response;
using RFacturacionElectronicaDIAN.Factories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using WebApplication.Class;
using WebApplication.Helpers;
using WebApplication.ViewModels;
using Acquirer_Response = RFacturacionElectronicaDIAN.Entities.Response.Acquirer_Response;

namespace WebApplication
{
    public partial class Cobrar : System.Web.UI.Page
    {
        #region Constantes de sesión / claves
        private const string SessionVendedorKey = "Vendedor";
        private const string SessionZonaActivaKey = "zonaactiva";
        private const string SessionModelsKey = "Models";
        private const string SessionIdVendedorKey = "idvendedor";
        #endregion

        private List<type_document_identifications> _tiposDocumento;

        public MenuViewModels Models { get; private set; }

        private MenuViewModels ModelSesion
        {
            get
            {
                if (Models != null)
                {
                    return Models;
                }

                Models = SessionContextHelper.LoadModels(Session);
                return Models;
            }
        }

        private string DbActual => Convert.ToString(Session[SessionContextHelper.DbKey] ?? ModelSesion?.db ?? string.Empty);
        private V_TablaVentas VentaActual => ModelSesion?.venta;

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!EnsureCobroContext())
            {
                Response.Redirect("~/caja.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                await InicializarPantalla();
            }
            else
            {
                await ProcesarPostBack();
            }

            int descu = Convert.ToInt32(Session["descuento_valor"] ?? 0);
            int pro = Convert.ToInt32(Session["propina_valor"] ?? 0);
            if (Session["saldo"] != null)
            {
                int saldo = Convert.ToInt32(Session["saldo"]);
                txtEfectivo.Value = Convert.ToString(saldo);
            }
            txtRazonDescuento.Value = Session["descuento_razon"]?.ToString() ?? "";
            txtDescuento.Value = Convert.ToString(descu);
            txtPropina.Value = Convert.ToString(pro);
        }

        private bool EnsureCobroContext()
        {
            var model = SessionContextHelper.LoadModels(Session);
            if (model?.venta == null)
            {
                return false;
            }

            Models = model;
            SessionContextHelper.ApplyOperationalContext(Session, model);
            return !string.IsNullOrWhiteSpace(DbActual);
        }

        private async Task InicializarPantalla()
        {
            await CargarMediosPago();
            await CargarTiposDocumento();
            await CargarTiposOrganizacion();
            await CargarMunicipios();
            await CargarTiposRegimen();
            await CargarTiposResponsabilidad();
            await CargarDetallesImpuesto();
            await CargarClientesModal();
            CargarDatosVenta();
            await CargarRelMediosInternos();
            await AsegurarPagoJsonInicial();

            ModelSesion.cargoDescuentoVentas = await CargoDescuentoVentasControler.ObtenerPorVenta(DbActual, ModelSesion.venta.id);
            var descuento = (ModelSesion.cargoDescuentoVentas ?? new List<CargoDescuentoVentas>()).FirstOrDefault(x => x.tipo == false);
            Session["descuento_valor"] = descuento?.valor ?? 0;
            Session["descuento_razon"] = descuento?.razon ?? "";

            var propina = (ModelSesion.cargoDescuentoVentas ?? new List<CargoDescuentoVentas>()).FirstOrDefault(x => x.tipo == true);
            Session["propina_valor"] = propina?.valor ?? 0;

            var cliente = await ClientesControler.Consultar_id(DbActual, ModelSesion.venta.idCliente);
            if (cliente != null)
            {
                Session["cliente_seleccionado_id"] = cliente.id;
                Session["cliente_seleccionado_nombre"] = cliente.nameCliente ?? "";
                Session["cliente_seleccionado_nit"] = cliente.identificationNumber ?? "";
                Session["cliente_seleccionado_correo"] = cliente.email ?? "";

                cliente_seleccionado_nit.Text = cliente.identificationNumber ?? "";
                cliente_seleccionado_nombre.Text = cliente.nameCliente ?? "";
                cliente_seleccionado_correo.Text = cliente.email ?? "";
            }

            hfIdVentaActual.Value = VentaActual.id.ToString();
        }
        private async Task ProcesarPostBack()
        {
            var eventTarget = Request["__EVENTTARGET"];
            var eventArgument = Request["__EVENTARGUMENT"];

            System.Diagnostics.Debug.WriteLine("EventTarget: " + eventTarget);
            System.Diagnostics.Debug.WriteLine("EventArgument: " + eventArgument);

            if (string.IsNullOrEmpty(eventTarget)) return;

            switch (eventTarget)
            {
                case "btnSeleccionarPagoInterno":
                    await btnSeleccionarPagoInterno(eventArgument);
                    break;

                case "btnGuardarDescuento":
                    await btnGuardarDescuento(eventArgument);
                    break;

                case "btnEliminarDescuento":
                    await btnEliminarDescuento(eventArgument);
                    break;

                case "btnGuardarPropina":
                    await btnGuardarPropina(eventArgument);
                    break;

                case "btnEliminarPropina":
                    await btnEliminarPropina(eventArgument);
                    break;

                case "btnGuardarPagoMixto":
                    await btnGuardarPagoMixto(eventArgument);
                    break;

                case "btnBuscarNIT":
                    await btnBuscarNIT(eventArgument);
                    break;

                case "btnSeleccionarCliente":
                    await btnSeleccionarCliente(eventArgument);
                    break;

                case "btnGuardarCliente":
                    await btnGuardarCliente(eventArgument);
                    break;

                case "btnGuardar":
                    await btnGuardar(eventArgument);
                    break;

                case "AsegurarPagoJsonInicial":
                    await AsegurarPagoJsonInicial();
                    break;

                default:
                    break;
            }
        }


        private async Task btnGuardar(string eventArgument)
        {
            try
            {
                // ==========================================================
                // 0) Validaciones base
                // ==========================================================
                if (VentaActual == null) return;

                if (Session["PagoVentaJSON"] == null)
                {
                    await AsegurarPagoJsonInicial();
                }

                if (Session["PagoVentaJSON"] == null)
                {
                    AlertModerno.Warning(this, "Atención", "No se especificó el medio de pago.", true, 2000);
                    return;
                }

                if (string.IsNullOrWhiteSpace(eventArgument))
                {
                    AlertModerno.Warning(this, "Atenci\u00f3n", "No lleg\u00f3 informaci\u00f3n del cobro.", true, 2000);
                    return;
                }

                // ==========================================================
                // 1) Leer pagos desde Session (asegurar lista)
                // ==========================================================
                string pagojson = Session["PagoVentaJSON"]?.ToString() ?? "";
                if (!pagojson.Contains("[") && !pagojson.Contains("]"))
                    pagojson = $"[{pagojson}]";

                var Pagos = JsonConvert.DeserializeObject<List<PagosVenta>>(pagojson) ?? new List<PagosVenta>();

                decimal abonoEfectivo = Pagos.Where(x => x.payment_methods_id == 10).Sum(x => x.valorPago);
                decimal abonoBanco = Pagos.Where(x => x.payment_methods_id != 10).Sum(x => x.valorPago);

                // ==========================================================
                // 2) eventArgument: Base64 -> JSON (con fallback)
                // ==========================================================
                string jsonPayload;
                try
                {
                    var bytes = Convert.FromBase64String(eventArgument);
                    jsonPayload = System.Text.Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                    jsonPayload = eventArgument; // fallback si llega plano
                }

                var payload = JsonConvert.DeserializeObject<GuardarCobroPayload>(jsonPayload);
                if (payload == null)
                {
                    AlertModerno.Error(this, "Error", "Payload inv\u00e1lido para guardar el cobro.", true);
                    return;
                }

                // ==========================================================
                // 3) Guardar datos de cobro en Session / ModelSesion
                // ==========================================================
                Session["efectivo"] = payload.efectivo;
                Session["cambio"] = payload.cambio;
                Session["fe"] = payload.facturaElectronica;

                try
                {
                    ModelSesion.venta.efectivoVenta = payload.efectivo;
                    ModelSesion.venta.cambioVenta = payload.cambio;
                }
                catch { }

                // ==========================================================
                // 4) Validaci\u00f3n FE: cliente seleccionado
                // ==========================================================
                if (payload.facturaElectronica)
                {
                    if (Session["cliente_seleccionado_id"] == null)
                    {
                        AlertModerno.Warning(this, "Atenci\u00f3n", "Factura electr\u00f3nica activa: debes seleccionar un cliente.", true, 2200);
                        return;
                    }
                }

                // ==========================================================
                // 5) Resolver tipo factura y resoluci\u00f3n
                // ==========================================================
                string tipoFactura = payload.facturaElectronica
                    ? "FACTURA ELECTR\u00d3NICA DE VENTA"
                    : "POS";

                var db = DbActual;
                var resolucion = await V_Resoluciones_Controler.ConsulrarResolucion(db, tipoFactura);
                if (resolucion == null)
                {
                    AlertModerno.Warning(this, "Atenci\u00f3n", "No se encontr\u00f3 la resoluci\u00f3n.", true, 2200);
                    return;
                }

                // ==========================================================
                // 6) Propina (Session -> ModelSesion)
                // ==========================================================
                decimal valorPropina = 0;
                if (Session["propina_valor"] != null)
                    valorPropina = Convert.ToDecimal(Session["propina_valor"]);

                ModelSesion.venta.propina = valorPropina;

                // ==========================================================
                // 7) Cargar venta y setear campos
                // ==========================================================
                var venta = await TablaVentasControler.ConsultarIdVenta(db, ModelSesion.IdCuentaActiva);

                venta.fechaVenta = DateTime.Now;
                venta.numeroVenta = await TablaVentasControler.Consecutivo(db, resolucion.idResolucion);

                venta.descuentoVenta = ModelSesion.venta.descuentoVenta;
                venta.efectivoVenta = ModelSesion.venta.efectivoVenta;
                venta.cambioVenta = ModelSesion.venta.cambioVenta;

                venta.estadoVenta = "CANCELADO";

                // ?? OJO: mantengo tu lógica: FirstOrDefault() como estaba
                venta.numeroReferenciaPago = await MediosDePagoInternos_Controler.ConsultarReferencia(
                    db,
                    Pagos.FirstOrDefault().idMedioDePagointerno
                );

                venta.diasCredito = ModelSesion.venta.diasCredito;
                venta.observacionVenta = ModelSesion.venta.observacionVenta;
                venta.IdSede = ModelSesion.venta.IdSede;
                venta.guidVenta = ModelSesion.venta.guidVenta;

                venta.abonoTarjeta = abonoBanco;
                venta.propina = valorPropina;
                venta.abonoEfectivo = abonoEfectivo;

                venta.idMedioDePago = Pagos.FirstOrDefault().payment_methods_id;
                venta.idResolucion = resolucion.idResolucion;
                venta.idFormaDePago = 1;

                venta.razonDescuento = ModelSesion.venta.razonDescuento;
                var idBaseCaja = SessionContextHelper.ResolveBaseCajaId(Session, ModelSesion);
                if (idBaseCaja <= 0)
                {
                    AlertModerno.Warning(this, "Atención", "No se encontró una base de caja activa para finalizar el cobro.", true, 2200);
                    GuardarModelsEnSesion();
                    return;
                }

                venta.idBaseCaja = idBaseCaja;

                venta.aliasVenta = ModelSesion.venta.aliasVenta;
                venta.porpropina = ModelSesion.venta.por_propina;
                venta.eliminada = ModelSesion.venta.eliminada;

                // ==========================================================
                // 8) Guardar venta
                // ==========================================================
                var resp = await TablaVentasControler.CRUD(db, venta, 1);
                if (resp.estado == false)
                {
                    AlertModerno.Error(this, "Error", "No se proceso la venta.", true, 1200);
                }

                // ==========================================================
                // 9) Cargar vista venta y guardar pagos
                // ==========================================================
                var tv = await V_TablaVentasControler.Consultar_Id(db, ModelSesion.IdCuentaActiva);
                if (tv == null)
                {

                }

                bool respPagos = await PagosVenta_controler.CRUD(db, Pagos, 0);
                if (!respPagos)
                {

                }


                // ==========================================================
                // 10) Factura electr\u00f3nica (si aplica)
                // ==========================================================
                if (payload.facturaElectronica)
                {
                    var tokenFe = Session[SessionContextHelper.TokenFeKey]?.ToString() ?? ModelSesion?.TokenEmpresa ?? string.Empty;

                    var respfe = await ClassFE.FacturaElectronica(db, tv, ModelSesion.detalleCaja, tokenFe);
                    if (respfe)
                    {
                        AlertModerno.Success(this, "OK", "Factura electr\u00f3nica enviada correctamente.", true, 1200);
                    }
                    else
                    {
                        AlertModerno.Error(this, "Error", "La factura electr\u00f3nica no fue enviada.", true, 1200);
                        GuardarModelsEnSesion();
                        DataBind();
                        return;
                    }
                }

                // ==========================================================
                // 11) Orden de impresión
                // ==========================================================
                var printer = new ImprimirFactura { id = 0, idventa = venta.id };
                var ordenPrinter = await ImprimirFacturaControler.CRUD(db, printer, 0);

                if (!ordenPrinter.estado)
                {
                    AlertModerno.Error(this, "Error", "La factura no se pudo imprimir.", true, 1200);
                    GuardarModelsEnSesion();
                    DataBind();
                    return;
                }

                //antes de terminar liberamos las mesas que est\u00e1n ancladas a esta cuenta
                var relaciones = await R_VentaMesaControler.ListaRelacion(db,venta.id);
                if (relaciones.Count > 0)
                {
                    foreach (var rec in relaciones) 
                    {
                        //recorremos la lista de las relaciones y vamos liberando las mesas
                        var mesa = await MesasControler.Consultar_id(db,rec.idMesa);
                        if (mesa != null)
                        {
                            mesa.estadoMesa = 0;
                            var respCRUD = await MesasControler.CRUD(db,mesa,1);
                        }
                    }
                }

                // ==========================================================
                // 12) Final OK
                // ==========================================================
                AlertModerno.Success(this, "OK", "Datos de cobro recibidos correctamente.", true, 1200);

                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "redirOK",
                    "setTimeout(function(){ if(window.bloquearCobroHastaSalir){ window.bloquearCobroHastaSalir('Finalizando cobro'); } }, 1150); setTimeout(function(){ window.location.href='caja.aspx'; }, 1500);",
                    true
                );

                GuardarModelsEnSesion();
                DataBind();
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error btnGuardar: " + ex.Message);
                AlertModerno.Error(this, "\u00a1Error!", "No fue posible guardar el cobro.", true);
            }

            await Task.CompletedTask;
        }

        public class GuardarCobroPayload
        {
            public int efectivo { get; set; }
            public int cambio { get; set; }
            public bool facturaElectronica { get; set; }
        }

        private void GuardarModelsEnSesion()
        {
            SessionContextHelper.ApplyOperationalContext(Session, ModelSesion);
        }

        private async Task btnSeleccionarCliente(string eventArgument)
        {
            if (string.IsNullOrWhiteSpace(eventArgument))
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "Debes seleccionar un cliente antes de continuar.", true, 2000);
                return;
            }

            if (!int.TryParse(eventArgument, out var clienteId) || clienteId <= 0)
            {
                AlertModerno.Error(this, "Error", "ID de cliente inv\u00e1lido.", true);
                return;
            }

            var clientes = ModelSesion?.clientes ?? new List<Clientes>();
            var cliente = clientes.FirstOrDefault(c => c.id == clienteId);
            if (cliente == null)
            {
                AlertModerno.Error(this, "Error", "No se encontr\u00f3 el cliente seleccionado.", true);
                return;
            }

            int funcion = 0;
            var relacion = await R_VentaCliente_Controler.ConsultarRelacion(Session["db"].ToString(), ModelSesion.venta.id);
            if (relacion == null)
            {
                relacion = new R_VentaCliente();
                relacion.id = 0;
                relacion.idVenta = ModelSesion.venta.id;
                relacion.idCliente = clienteId;
                relacion.idSede = 0;
            }
            else
            {
                funcion = 1;
                relacion.idCliente = clienteId;
            }
            var resul = await R_VentaCliente_Controler.CRUD(Session["db"].ToString(), relacion, funcion);
            if (!resul)
            {
                AlertModerno.Error(this, "Error", "No se puso relacionar el cliente seleccionado.", true);
                return;
            }

            Session["cliente_seleccionado_id"] = clienteId;
            Session["cliente_seleccionado_nombre"] = cliente.nameCliente ?? "";
            Session["cliente_seleccionado_nit"] = cliente.identificationNumber ?? "";
            Session["cliente_seleccionado_correo"] = cliente.email ?? "";

            cliente_seleccionado_nit.Text = cliente.identificationNumber ?? "";
            cliente_seleccionado_nombre.Text = cliente.nameCliente ?? "";
            cliente_seleccionado_correo.Text = cliente.email ?? "";

            Session["fe"] = true;

            AlertModerno.Success(this, "OK", $"Cliente seleccionado: {cliente.nameCliente}", true, 1500);

            string scriptCerrar = @"
                (function(){
                    var modalEl = document.getElementById('mdlCliente');
                    if (modalEl && window.bootstrap) {
                        var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
                        modal.hide();
                    }
                })();
                ";
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "CerrarModalClienteSeleccionado",
                scriptCerrar,
                true);
        }

        private async Task btnGuardarCliente(string eventArgument)
        {
            try
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

                var payload = JsonConvert.DeserializeObject<ClienteGuardarPayload>(json);
                if (payload == null)
                {
                    AlertModerno.Error(this, "Error", "Payload inválido para guardar el cliente.", true);
                    return;
                }

                var db = DbActual;
                var nit = (payload.nit ?? string.Empty).Trim();
                var nombre = (payload.nombre ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(nit) || string.IsNullOrWhiteSpace(nombre))
                {
                    AlertModerno.Warning(this, "Atención", "Debes completar identificación y nombre del cliente.", true, 2200);
                    return;
                }

                int funcion = 0;
                var clientes = ModelSesion?.clientes ?? new List<Clientes>();
                var existente = (payload.clienteId > 0)
                    ? clientes.FirstOrDefault(x => x.id == payload.clienteId)
                    : clientes.FirstOrDefault(x => (x.identificationNumber ?? "") == nit);

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
                    return;
                }

                int idClienteGuardado = existente?.id ?? 0;
                if (idClienteGuardado <= 0 && respuesta.data != null)
                {
                    int.TryParse(respuesta.data.ToString(), out idClienteGuardado);
                }

                ModelSesion.clientes = await ClientesControler.ListaClientes(db);
                await CargarClientesModal();
                GuardarModelsEnSesion();

                if (idClienteGuardado <= 0)
                {
                    var recargado = (ModelSesion.clientes ?? new List<Clientes>()).FirstOrDefault(x => (x.identificationNumber ?? "") == nit);
                    idClienteGuardado = recargado?.id ?? 0;
                }

                if (idClienteGuardado <= 0)
                {
                    AlertModerno.Warning(this, "Atención", "El cliente se guardó, pero no fue posible seleccionarlo automáticamente.", true, 2200);
                    return;
                }

                await btnSeleccionarCliente(idClienteGuardado.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error btnGuardarCliente: " + ex.Message);
                AlertModerno.Error(this, "Error", "No fue posible guardar el cliente.", true);
            }
        }
        private async Task btnBuscarNIT(string nit)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nit))
                {
                    return;
                }

                var clientes = ModelSesion?.clientes ?? new List<Clientes>();
                var cliente = clientes.FirstOrDefault(x => x.identificationNumber == nit);
                var encontradoEnBase = cliente != null;

                if (cliente == null)
                {
                    cliente = new Clientes();
                    var acquirer_Response = await Consultar_NIT_DIAN(Convert.ToInt32(nit));

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
                        var limpiarScript = $"Swal.fire({{icon:'error',title:'\u00a1Error!',text:'El documento no se encuentra registrado ni en la base de datos ni en la DIAN. Debes crearlo manualmente.',confirmButtonColor:'#2563eb'}}).then(function(){{if(window.setClienteData){{window.setClienteData({limpiarJson});}} var modalEl=document.getElementById('mdlCliente'); if(modalEl){{bootstrap.Modal.getOrCreateInstance(modalEl).show();}}}});";

                        ClientScript.RegisterStartupScript(GetType(), "nitNoEncontrado", limpiarScript, true);
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
                    matricula = cliente.merchantRegistration,
                    actionLabel = encontradoEnBase ? "Editar" : "Guardar"
                };

                var json = JsonConvert.SerializeObject(payload);
                var script = $"Swal.close(); if(window.setClienteData){{window.setClienteData({json});}} var modalEl=document.getElementById('mdlCliente'); if(modalEl){{bootstrap.Modal.getOrCreateInstance(modalEl).show();}}";

                ClientScript.RegisterStartupScript(GetType(), "nitEncontrado", script, true);
            }
            catch
            {
                ClientScript.RegisterStartupScript(
                    GetType(),
                    "nitError",
                    "Swal.fire({icon:'error',title:'\u00a1Error!',text:'No fue posible consultar el NIT.',confirmButtonColor:'#2563eb'}).then(function(){var modalEl=document.getElementById('mdlCliente'); if(modalEl){bootstrap.Modal.getOrCreateInstance(modalEl).show();}});",
                    true);
            }

            return;
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

        private async Task btnGuardarDescuento(string eventArgument)
        {
            try
            {
                var db = DbActual;
                int idVentaActual = ModelSesion?.venta?.id ?? 0;
                if (idVentaActual <= 0)
                {
                    AlertModerno.Error(this, "\u00a1Error!", "No se encontr\u00f3 una venta activa para guardar el descuento.", true);
                    return;
                }
                int valor = 0;
                string razon = "";

                if (!string.IsNullOrWhiteSpace(eventArgument))
                {
                    var parts = eventArgument.Split('|');
                    if (parts.Length >= 1) int.TryParse(parts[0], out valor);
                    if (parts.Length >= 2) razon = parts[1] ?? "";
                }

                Session["descuento_valor"] = valor;
                Session["descuento_razon"] = razon;

                var descuento = new CargoDescuentoVentas
                {
                    id = 0,
                    idVenta = idVentaActual,
                    tipo = false,
                    codigo = 1,
                    razon = razon,
                    valor = valor,
                    baseCD = ModelSesion.venta.totalVenta,
                    descripcionCargoDescuento = razon
                };

                var respuestaCRUD = await CargoDescuentoVentasControler.CRUD(db, descuento, 0);

                if (!respuestaCRUD)
                {
                    AlertModerno.Error(this, "\u00a1Error!", "No fue posible agregar el descuento.", true);
                    return;
                }
                ModelSesion.venta = await V_TablaVentasControler.Consultar_Id(db, idVentaActual);
                ModelSesion.cargoDescuentoVentas = await CargoDescuentoVentasControler.ObtenerPorVenta(db, idVentaActual);

                AlertModerno.Success(this, "\u00a1OK!", "Descuento agregado con \u00e9xito", true, 800);

                GuardarModelsEnSesion();
                DataBind();
            }
            catch
            {
                AlertModerno.Error(this, "\u00a1Error!", "No fue posible crear la relaci\u00f3n del vendedor con la venta.", true);
            }
        }

        private async Task btnEliminarDescuento(string eventArgument)
        {
            try
            {
                Session["descuento_valor"] = 0;
                Session["descuento_razon"] = "";

                int idventa = ModelSesion.venta.id;

                var dal = new SqlAutoDAL();
                var r = await dal.EjecutarSQLObjeto<RespuestaCRUD>(Session["db"].ToString(), $"EXEC dbo.DELETE_CargoDescuentoVentas {idventa},0;");

                if (r.estado == false)
                {
                AlertModerno.Error(this, "\u00a1Error!", "No fue posible eliminar el descuento.", true);
                    return;
                }

                ModelSesion.venta = new V_TablaVentas();
                ModelSesion.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), idventa);
                AlertModerno.Success(this, "\u00a1OK!", "Descuento eliminado con \u00e9xito", true, 800);
                GuardarModelsEnSesion();
                DataBind();
            }
            catch
            {
                AlertModerno.Error(this, "\u00a1Error!", "No fue posible eliminar el descuento.", true);
            }
        }
        private async Task btnGuardarPropina(string eventArgument)
        {
            try
            {
                int idVentaActual = ModelSesion?.venta?.id ?? 0;
                var db = DbActual;
                if (idVentaActual <= 0)
                {
                    AlertModerno.Error(this, "\u00a1Error!", "No se encontr\u00f3 una venta activa para guardar la propina.", true);
                    return;
                }

                int valor = 0;
                int pct = 0;

                if (!string.IsNullOrWhiteSpace(eventArgument))
                {
                    var parts = eventArgument.Split('|');
                    if (parts.Length >= 1) int.TryParse(parts[0], out valor);
                    if (parts.Length >= 2) int.TryParse(parts[1], out pct);
                }

                Session["propina_valor"] = valor;
                Session["propina_pct"] = pct;

                var descuento = new CargoDescuentoVentas
                {
                    id = 0,
                    idVenta = idVentaActual,
                    tipo = true,
                    codigo = 1,
                    razon = "propina",
                    valor = valor,
                    baseCD = ModelSesion.venta.totalVenta,
                    descripcionCargoDescuento = "propina"
                };

                var respuestaCRUD = await CargoDescuentoVentasControler.CRUD(db, descuento, 0);

                if (!respuestaCRUD)
                {
                    AlertModerno.Error(this, "\u00a1Error!", "No fue posible agregar la propina.", true);
                    return;
                }

                ModelSesion.venta = await V_TablaVentasControler.Consultar_Id(db, idVentaActual);
                ModelSesion.cargoDescuentoVentas = await CargoDescuentoVentasControler.ObtenerPorVenta(db, idVentaActual);

                AlertModerno.Success(this, "\u00a1OK!", "Propina agregada con \u00e9xito", true, 800);

                GuardarModelsEnSesion();
                DataBind();
            }
            catch
            {
                AlertModerno.Error(this, "\u00a1Error!", "No fue posible agregar la propina.", true);
            }
        }

        private async Task btnEliminarPropina(string eventArgument)
        {
            try
            {
                Session["propina_valor"] = 0;
                Session["propina_pct"] = 0;

                int idventa = ModelSesion.venta.id;

                var dal = new SqlAutoDAL();
                var r = await dal.EjecutarSQLObjeto<RespuestaCRUD>(Session["db"].ToString(), $"EXEC dbo.DELETE_CargoDescuentoVentas {idventa},1;");

                if (r.estado == false)
                {
                    AlertModerno.Error(this, "\u00a1Error!", "No fue posible eliminar la propina.", true);
                    return;
                }

                ModelSesion.venta = new V_TablaVentas();
                ModelSesion.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), idventa);
                AlertModerno.Success(this, "\u00a1OK!", "Propina eliminada con \u00e9xito", true, 800);
                GuardarModelsEnSesion();
                DataBind();
            }
            catch
            {
                    AlertModerno.Error(this, "\u00a1Error!", "No fue posible eliminar la propina.", true);
            }
        }

        private async Task btnSeleccionarPagoInterno(string eventArgument)
        {
            try
            {
                if (VentaActual == null) return;

                int idMedioInterno = 0;
                int idMetodoPago = 0;

                if (!string.IsNullOrWhiteSpace(eventArgument))
                {
                    var parts = eventArgument.Split('|');
                    if (parts.Length >= 2)
                    {
                        int.TryParse(parts[0], out idMedioInterno);
                        int.TryParse(parts[1], out idMetodoPago);
                    }
                }

                var pagoventa = new PagosVenta { id = 0, idMedioDePagointerno = idMedioInterno, idVenta = VentaActual.id, payment_methods_id = idMetodoPago, valorPago = Convert.ToInt32(VentaActual.total_A_Pagar) };

                var pagosJSON = JsonConvert.SerializeObject(pagoventa);

                Session["PagoVentaJSON"] = pagosJSON;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error btnSeleccionarPagoInterno: " + ex.Message);
            }
        }

        private async Task CargarMediosPago()
        {
            var db = Session["db"]?.ToString();
            if (string.IsNullOrWhiteSpace(db)) return;

            var payment_Methods = await payment_methodsControler.ListaMetodosDePago(db);

            var listaActivos = (payment_Methods ?? new List<payment_methods>())
                .Where(x => x != null && x.state)
                .ToList();

            ddlMedioPago.DataSource = listaActivos;
            ddlMedioPago.DataTextField = "name";
            ddlMedioPago.DataValueField = "id";
            ddlMedioPago.DataBind();

            // 1) Si la venta ya trae medio, resp\u00e9talo
            if (VentaActual != null && VentaActual.idMedioDePago > 0)
            {
                var valor = VentaActual.idMedioDePago.ToString();
                var item = ddlMedioPago.Items.FindByValue(valor);
                if (item != null)
                {
                    ddlMedioPago.ClearSelection();
                    item.Selected = true;
                    return;
                }
            }

            // 2) Si NO trae, dejar EFECTIVO como predeterminado (por texto)
            var efectivoItem = ddlMedioPago.Items.Cast<System.Web.UI.WebControls.ListItem>()
                .FirstOrDefault(i => (i.Text ?? "").Trim().ToLower().Contains("efectivo"));

            if (efectivoItem != null)
            {
                ddlMedioPago.ClearSelection();
                efectivoItem.Selected = true;
                return;
            }

            // 3) Fallback: primer item
            if (ddlMedioPago.Items.Count > 0)
                ddlMedioPago.SelectedIndex = 0;
        }

        private async Task CargarTiposDocumento()
        {
            var db = Session["db"]?.ToString();
            if (string.IsNullOrWhiteSpace(db)) return;

            _tiposDocumento = await type_document_identificationsControler.ListaTiposDocumento(db);

            ddlTipoDocumento.DataSource = _tiposDocumento;
            ddlTipoDocumento.DataTextField = "name";
            ddlTipoDocumento.DataValueField = "id";
            ddlTipoDocumento.DataBind();

            if (ddlTipoDocumento.Items.Count == 0)
            {
                ddlTipoDocumento.Items.Add(new System.Web.UI.WebControls.ListItem("Sin datos", ""));
            }
        }

        private async Task CargarMunicipios()
        {
            var db = Session["db"]?.ToString();
            if (string.IsNullOrWhiteSpace(db)) return;

            var municipios = await V_MunicipiosControler.ListaMunicipios(db);

            ddlMunicipio.DataSource = municipios;
            ddlMunicipio.DataTextField = "name";
            ddlMunicipio.DataValueField = "idMunicipio";
            ddlMunicipio.DataBind();

            if (ddlMunicipio.Items.Count == 0)
            {
                ddlMunicipio.Items.Add(new System.Web.UI.WebControls.ListItem("Sin datos", ""));
            }
        }

        private async Task CargarTiposRegimen()
        {
            var db = Session["db"]?.ToString();
            if (string.IsNullOrWhiteSpace(db)) return;

            var tipos = await type_regimesControler.ListaTiposRegimen(db);

            ddlTipoRegimen.DataSource = tipos;
            ddlTipoRegimen.DataTextField = "name";
            ddlTipoRegimen.DataValueField = "id";
            ddlTipoRegimen.DataBind();

            if (ddlTipoRegimen.Items.Count == 0)
            {
                ddlTipoRegimen.Items.Add(new System.Web.UI.WebControls.ListItem("Sin datos", ""));
            }
        }

        private async Task CargarTiposResponsabilidad()
        {
            var db = Session["db"]?.ToString();
            if (string.IsNullOrWhiteSpace(db)) return;

            var tipos = await type_liabilitiesControler.ListaTiposResponsabilidad(db);

            ddlTipoResponsabilidad.DataSource = tipos;
            ddlTipoResponsabilidad.DataTextField = "name";
            ddlTipoResponsabilidad.DataValueField = "id";
            ddlTipoResponsabilidad.DataBind();

            if (ddlTipoResponsabilidad.Items.Count == 0)
            {
                ddlTipoResponsabilidad.Items.Add(new System.Web.UI.WebControls.ListItem("Sin datos", ""));
            }
        }

        private async Task CargarDetallesImpuesto()
        {
            var db = Session["db"]?.ToString();
            if (string.IsNullOrWhiteSpace(db)) return;

            var detalles = await tax_detailsControler.ListaDetallesImpuesto(db);

            ddlDetalleImpuesto.DataSource = detalles;
            ddlDetalleImpuesto.DataTextField = "name";
            ddlDetalleImpuesto.DataValueField = "id";
            ddlDetalleImpuesto.DataBind();

            if (ddlDetalleImpuesto.Items.Count == 0)
            {
                ddlDetalleImpuesto.Items.Add(new System.Web.UI.WebControls.ListItem("Sin datos", ""));
            }
        }

        private async Task CargarTiposOrganizacion()
        {
            var db = Session["db"]?.ToString();
            if (string.IsNullOrWhiteSpace(db)) return;

            var tipos = await type_organizationsControler.ListaTiposOrganizacion(db);

            ddlTipoOrganizacion.DataSource = tipos;
            ddlTipoOrganizacion.DataTextField = "name";
            ddlTipoOrganizacion.DataValueField = "id";
            ddlTipoOrganizacion.DataBind();

            if (ddlTipoOrganizacion.Items.Count == 0)
            {
                ddlTipoOrganizacion.Items.Add(new System.Web.UI.WebControls.ListItem("Sin datos", ""));
            }
        }

        private async Task CargarClientesModal()
        {
            var clientes = ModelSesion?.clientes ?? new List<Clientes>();

            if (_tiposDocumento == null)
            {
                var db = Session["db"]?.ToString();
                _tiposDocumento = string.IsNullOrWhiteSpace(db)
                    ? new List<type_document_identifications>()
                    : await type_document_identificationsControler.ListaTiposDocumento(db);
            }

            var tipoDocumentoPorId = (_tiposDocumento ?? new List<type_document_identifications>())
                .Where(t => t != null && t.id.HasValue)
                .ToDictionary(t => t.id.Value, t => t.name);

            var items = clientes.Select(c =>
            {
                var tipoNombre = tipoDocumentoPorId.TryGetValue(c.typeDocumentIdentification_id, out var nombre)
                    ? nombre
                    : c.typeDocumentIdentification_id.ToString();

                return new ClienteModalItem
                {
                    ClienteId = c.id,
                    TipoDocumentoId = c.typeDocumentIdentification_id,
                    TipoDocumento = tipoNombre,
                    Nit = c.identificationNumber,
                    NombreCliente = c.nameCliente,
                    Correo = c.email,
                    TipoOrganizacionId = c.typeOrganization_id,
                    MunicipioId = c.municipality_id,
                    TipoRegimenId = c.typeRegime_id,
                    TipoResponsabilidadId = c.typeLiability_id,
                    DetalleImpuestoId = c.typeTaxDetail_id,
                    NombreComercio = c.tradeName,
                    Telefono = c.phone,
                    Direccion = c.adress,
                    MatriculaMercantil = c.merchantRegistration
                };
            }).ToList();

            rptClientesModal.DataSource = items;
            rptClientesModal.DataBind();
        }

        private class ClienteGuardarPayload
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
        }
        private class ClienteModalItem
        {
            public int ClienteId { get; set; }
            public int TipoDocumentoId { get; set; }
            public string TipoDocumento { get; set; }
            public string Nit { get; set; }
            public string NombreCliente { get; set; }
            public string Correo { get; set; }
            public int TipoOrganizacionId { get; set; }
            public int MunicipioId { get; set; }
            public int TipoRegimenId { get; set; }
            public int TipoResponsabilidadId { get; set; }
            public int DetalleImpuestoId { get; set; }
            public string NombreComercio { get; set; }
            public string Telefono { get; set; }
            public string Direccion { get; set; }
            public string MatriculaMercantil { get; set; }
        }

        private Task btnGuardarPagoMixto(string eventArgument)
        {
            try
            {
                if (VentaActual == null) return Task.CompletedTask;
                if (string.IsNullOrWhiteSpace(eventArgument)) return Task.CompletedTask;

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

                var payload = JsonConvert.DeserializeObject<PagoMixtoPayload>(json);
                if (payload?.pagos == null || payload.pagos.Count == 0) return Task.CompletedTask;

                int idMetodoPago = payload.idMetodoPago;

                var listaPagos = new List<PagosVenta>();
                decimal sumaInternos = 0;

                foreach (var p in payload.pagos)
                {
                    if (p == null) continue;

                    int idMedioInterno = p.idMedioInterno;
                    int valor = p.valor;

                    if (idMedioInterno <= 0) continue;
                    if (valor <= 0) continue;

                    sumaInternos += valor;

                    listaPagos.Add(new PagosVenta
                    {
                        id = 0,
                        idMedioDePagointerno = idMedioInterno,
                        idVenta = VentaActual.id,
                        payment_methods_id = idMetodoPago,
                        valorPago = valor
                    });
                }

                if (listaPagos.Count == 0) return Task.CompletedTask;

                decimal totalAPagar = Convert.ToDecimal(VentaActual.total_A_Pagar);
                decimal saldo = totalAPagar - sumaInternos;
                if (saldo < 0) saldo = 0;

                int idMedioInternoEfectivo = 0;
                if (Session["idMedioInternoEfectivo"] != null)
                    int.TryParse(Session["idMedioInternoEfectivo"].ToString(), out idMedioInternoEfectivo);

                if (saldo > 0)
                {
                    Session["saldo"] = Convert.ToInt32(Math.Round(saldo));
                    listaPagos.Add(new PagosVenta
                    {
                        id = 0,
                        idMedioDePagointerno = 1,
                        idVenta = VentaActual.id,
                        payment_methods_id = idMetodoPago,
                        valorPago = Convert.ToInt32(Math.Round(saldo))
                    });
                }

                var pagosJSON = JsonConvert.SerializeObject(listaPagos);

                Session["PagoVentaJSON"] = pagosJSON;

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error btnGuardarPagoMixto: " + ex.Message);
                return Task.CompletedTask;
            }
        }

        public class PagoMixtoPayload
        {
            public int idMetodoPago { get; set; }
            public List<PagoMixtoItem> pagos { get; set; }
        }
        public class PagoMixtoItem
        {
            public int idMedioInterno { get; set; }
            public int valor { get; set; }
        }

        private void CargarDatosVenta()
        {
            if (VentaActual == null) return;

            var co = new CultureInfo("es-CO");

            txtSubTotal.Value = VentaActual.subtotalVenta.ToString("N0", co);
            txtIVA.Value = VentaActual.ivaVenta.ToString("N0", co);
            txtDescuento.Value = VentaActual.descuentoVenta.ToString("N0", co);
            txtPropina.Value = VentaActual.propina.ToString("N0", co);

            lblTotalGrande.InnerText = VentaActual.total_A_Pagar.ToString("N0", co);

            txtEfectivo.Value = VentaActual.efectivoVenta.ToString("N0", co);
            txtCambio.Value = VentaActual.cambioVenta.ToString("N0", co);
        }

        private async Task CargarRelMediosInternos()
        {
            List<V_R_MediosDePago_MediosDePagoInternos> rel =
                await V_R_MediosDePago_MediosDePagoInternosControler.GetAll(DbActual);

            hfRelMediosInternos.Value = JsonConvert.SerializeObject(rel ?? new List<V_R_MediosDePago_MediosDePagoInternos>());
        }

        private async Task AsegurarPagoJsonInicial()
        {
            try
            {
                if (VentaActual == null) return;

                // Si ya existe, no hacer nada
                if (Session["PagoVentaJSON"] != null) return;

                int idMetodoPago = 0;
                int.TryParse(ddlMedioPago.SelectedValue, out idMetodoPago);
                if (idMetodoPago <= 0) return;

                var rel = await V_R_MediosDePago_MediosDePagoInternosControler
                    .GetAll(DbActual);

                var r = (rel ?? new List<V_R_MediosDePago_MediosDePagoInternos>())
                    .FirstOrDefault(x => x.idMedioDePago == idMetodoPago);

                if (r == null) return;

                int idMedioInterno = r.idMediosDePagoInternos;
                if (idMedioInterno <= 0) return;

                var pagoventa = new PagosVenta
                {
                    id = 0,
                    idMedioDePagointerno = idMedioInterno,
                    idVenta = VentaActual.id,
                    payment_methods_id = idMetodoPago,
                    valorPago = Convert.ToDecimal(VentaActual.total_A_Pagar)
                };

                Session["PagoVentaJSON"] = JsonConvert.SerializeObject(pagoventa);
            }
            catch
            {
            }
        }
    }
}







