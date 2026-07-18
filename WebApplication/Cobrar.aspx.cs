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
        private const string SessionTipoVentaCobroKey = "cobro_tipo_venta";
        private const string SessionFacturaElectronicaKey = "fe";
        private const string SessionCobrarMediosPagoKey = "Cobrar_MediosPago";
        private const string SessionCobrarTiposDocumentoKey = "Cobrar_TiposDocumento";
        private const string SessionCobrarTiposOrganizacionKey = "Cobrar_TiposOrganizacion";
        private const string SessionCobrarMunicipiosKey = "Cobrar_Municipios";
        private const string SessionCobrarTiposRegimenKey = "Cobrar_TiposRegimen";
        private const string SessionCobrarTiposResponsabilidadKey = "Cobrar_TiposResponsabilidad";
        private const string SessionCobrarDetallesImpuestoKey = "Cobrar_DetallesImpuesto";
        private const string SessionCajaMesasKey = "Caja_Mesas";
        private const string SessionCobrarClientesKey = "Cobrar_Clientes";
        private const string SessionCobrarRelMediosInternosKey = "Cobrar_RelMediosInternos";
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

            ResolverTipoVentaUI();
            ResolverFacturaElectronicaUI();

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
            hfTipoVenta.Value = Convert.ToString(Session[SessionTipoVentaCobroKey] ?? "contado");
            hfFacturaElectronica.Value = Convert.ToBoolean(Session[SessionFacturaElectronicaKey] ?? false) ? "true" : "false";
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
            var mediosPagoTask = ObtenerMediosPagoAsync();
            var tiposDocumentoTask = ObtenerTiposDocumentoAsync();
            var tiposOrganizacionTask = ObtenerTiposOrganizacionAsync();
            var municipiosTask = ObtenerMunicipiosAsync();
            var tiposRegimenTask = ObtenerTiposRegimenAsync();
            var tiposResponsabilidadTask = ObtenerTiposResponsabilidadAsync();
            var detallesImpuestoTask = ObtenerDetallesImpuestoAsync();
            var clientesTask = ObtenerClientesCobroAsync();
            var cargoDescuentoTask = CargoDescuentoVentasControler.ObtenerPorVenta(DbActual, ModelSesion.venta.id);
            var ventaTask = V_TablaVentasControler.Consultar_Id(DbActual, ModelSesion.venta.id);
            var relMediosInternosTask = ObtenerRelMediosInternosAsync();

            await Task.WhenAll(
                mediosPagoTask,
                tiposDocumentoTask,
                tiposOrganizacionTask,
                municipiosTask,
                tiposRegimenTask,
                tiposResponsabilidadTask,
                detallesImpuestoTask,
                clientesTask,
                cargoDescuentoTask,
                ventaTask,
                relMediosInternosTask);

            BindMediosPago(mediosPagoTask.Result ?? new List<payment_methods>());
            _tiposDocumento = tiposDocumentoTask.Result ?? new List<type_document_identifications>();
            BindTiposDocumento(_tiposDocumento);
            BindTiposOrganizacion(tiposOrganizacionTask.Result ?? new List<type_organizations>());
            BindMunicipios(municipiosTask.Result ?? new List<V_Municipios>());
            BindTiposRegimen(tiposRegimenTask.Result ?? new List<type_regimes>());
            BindTiposResponsabilidad(tiposResponsabilidadTask.Result ?? new List<type_liabilities>());
            BindDetallesImpuesto(detallesImpuestoTask.Result ?? new List<tax_details>());

            ModelSesion.clientes = clientesTask.Result ?? new List<Clientes>();
            BindClientesModal();

            ModelSesion.cargoDescuentoVentas = cargoDescuentoTask.Result;
            await SincronizarPropinaDesdeVista();

            ModelSesion.venta = ventaTask.Result ?? ModelSesion.venta;
            var descuento = (ModelSesion.cargoDescuentoVentas ?? new List<CargoDescuentoVentas>()).FirstOrDefault(x => x.tipo == false);
            Session["descuento_valor"] = descuento?.valor ?? 0;
            Session["descuento_razon"] = descuento?.razon ?? "";

            var propina = (ModelSesion.cargoDescuentoVentas ?? new List<CargoDescuentoVentas>()).FirstOrDefault(x => x.tipo == true);
            Session["propina_valor"] = propina?.valor ?? 0;
            Session["propina_pct"] = Convert.ToInt32(Math.Round((ModelSesion.venta?.por_propina ?? 0m) * 100m, 0));

            CargarDatosVenta();
            hfRelMediosInternos.Value = JsonConvert.SerializeObject(relMediosInternosTask.Result ?? new List<V_R_MediosDePago_MediosDePagoInternos>());
            await AsegurarPagoJsonInicial(true);

            var clienteIdInicial = await ObtenerClienteSeleccionadoIdAsync();
            var cliente = clienteIdInicial > 0
                ? await ClientesControler.Consultar_id(DbActual, clienteIdInicial)
                : null;
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
            hfTipoVenta.Value = Convert.ToString(Session[SessionTipoVentaCobroKey] ?? "contado");
            hfFacturaElectronica.Value = Convert.ToBoolean(Session[SessionFacturaElectronicaKey] ?? false) ? "true" : "false";
        }

        private async Task SincronizarPropinaDesdeVista()
        {
            if (ModelSesion?.venta == null || ModelSesion.venta.id <= 0)
            {
                return;
            }

            var propinaVista = Convert.ToDecimal(ModelSesion.venta.propina);
            if (propinaVista <= 0)
            {
                return;
            }

            var cargos = ModelSesion.cargoDescuentoVentas ?? new List<CargoDescuentoVentas>();
            var propinaExistente = cargos.FirstOrDefault(x => x.tipo == true);
            if (propinaExistente != null)
            {
                return;
            }

            var creada = await GuardarPropinaUnica(ModelSesion.venta.id, propinaVista);
            if (creada)
            {
                ModelSesion.cargoDescuentoVentas = await CargoDescuentoVentasControler.ObtenerPorVenta(DbActual, ModelSesion.venta.id);
            }
        }

        private async Task<bool> GuardarPropinaUnica(int idVenta, decimal valorPropina)
        {
            var db = DbActual;
            if (idVenta <= 0 || string.IsNullOrWhiteSpace(db))
            {
                return false;
            }

            try
            {
                using (var cn = new Conection_SQL(db))
                {
                    await cn.EjecutarConsulta($"DELETE FROM CargoDescuentoVentas WHERE idVenta = {idVenta} AND tipo = 1; SELECT CAST(1 AS bit) AS estado;", false);
                }
            }
            catch
            {
                return false;
            }

            if (valorPropina <= 0)
            {
                return true;
            }

            var ventaVista = ModelSesion?.venta ?? await V_TablaVentasControler.Consultar_Id(db, idVenta);
            var propina = new CargoDescuentoVentas
            {
                id = 0,
                idVenta = idVenta,
                tipo = true,
                codigo = 1,
                razon = "propina",
                valor = valorPropina,
                baseCD = ventaVista?.totalVenta ?? 0,
                descripcionCargoDescuento = "propina"
            };

            return await CargoDescuentoVentasControler.CRUD(db, propina, 0);
        }

        private void ResolverTipoVentaUI()
        {
            if (!IsPostBack)
            {
                Session[SessionTipoVentaCobroKey] = "contado";
                return;
            }

            var tipoVentaPost = (hfTipoVenta?.Value ?? string.Empty).Trim().ToLowerInvariant();
            if (tipoVentaPost == "credito" || tipoVentaPost == "contado")
            {
                Session[SessionTipoVentaCobroKey] = tipoVentaPost;
                return;
            }

            if (Session[SessionTipoVentaCobroKey] == null)
            {
                Session[SessionTipoVentaCobroKey] = "contado";
            }
        }

        private void ResolverFacturaElectronicaUI()
        {
            if (!IsPostBack)
            {
                if (Session[SessionFacturaElectronicaKey] == null)
                {
                    Session[SessionFacturaElectronicaKey] = false;
                }

                return;
            }

            var facturaElectronicaPost = (hfFacturaElectronica?.Value ?? string.Empty).Trim().ToLowerInvariant();
            if (facturaElectronicaPost == "true" || facturaElectronicaPost == "false")
            {
                Session[SessionFacturaElectronicaKey] = facturaElectronicaPost == "true";
                return;
            }

            if (Session[SessionFacturaElectronicaKey] == null)
            {
                Session[SessionFacturaElectronicaKey] = false;
            }
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

                if (string.IsNullOrWhiteSpace(eventArgument))
                {
                    AlertModerno.Warning(this, "Atenci\u00f3n", "No lleg\u00f3 informaci\u00f3n del cobro.", true, 2000);
                    return;
                }

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
                // 1) Leer pagos desde Session (solo contado)
                // ==========================================================
                var Pagos = new List<PagosVenta>();
                decimal abonoEfectivo = 0;
                decimal abonoBanco = 0;

                if (payload.idFormaDePago != 2)
                {
                    Pagos = await ObtenerPagosCobroAsync(payload.idMetodoPago);
                    if (Pagos.Count == 0)
                    {
                        AlertModerno.Warning(this, "Atención", "No se especificó el medio de pago.", true, 2000);
                        return;
                    }

                    abonoEfectivo = Pagos.Where(x => x.payment_methods_id == 10).Sum(x => x.valorPago);
                    abonoBanco = Pagos.Where(x => x.payment_methods_id != 10).Sum(x => x.valorPago);
                }

                // ==========================================================
                // 3) Guardar datos de cobro en Session / ModelSesion
                // ==========================================================
                Session["efectivo"] = payload.idFormaDePago == 2 ? 0 : payload.efectivo;
                Session["cambio"] = payload.idFormaDePago == 2 ? 0 : payload.cambio;
                Session["fe"] = payload.facturaElectronica;

                try
                {
                    ModelSesion.venta.efectivoVenta = payload.idFormaDePago == 2 ? 0 : payload.efectivo;
                    ModelSesion.venta.cambioVenta = payload.idFormaDePago == 2 ? 0 : payload.cambio;
                    ModelSesion.venta.idFormaDePago = payload.idFormaDePago == 2 ? 2 : 1;
                }
                catch { }

                // ==========================================================
                // 4) Validaci\u00f3n FE / cr\u00e9dito: cliente seleccionado
                // ==========================================================
                if (payload.facturaElectronica || payload.idFormaDePago == 2)
                {
                    var clienteSeleccionadoId = await ObtenerClienteSeleccionadoIdAsync();
                    if (clienteSeleccionadoId <= 0)
                    {
                        var mensajeCliente = payload.idFormaDePago == 2
                            ? "Venta a cr\u00e9dito: debes seleccionar un cliente."
                            : "Factura electr\u00f3nica activa: debes seleccionar un cliente.";

                        AlertModerno.Warning(this, "Atenci\u00f3n", mensajeCliente, true, 2200);
                        return;
                    }

                    if (!await PersistirClienteEnVentaAsync(clienteSeleccionadoId))
                    {
                        AlertModerno.Error(this, "Error", "No fue posible dejar guardado el cliente en la venta antes del envío.", true, 2200);
                        return;
                    }

                    if (payload.facturaElectronica)
                    {
                        var clienteFe = await ClientesControler.Consultar_id(DbActual, clienteSeleccionadoId);
                        var erroresClienteFe = ValidarClienteParaFacturaElectronica(clienteFe);
                        if (erroresClienteFe.Count > 0)
                        {
                            var detalle = string.Join(" ", erroresClienteFe);
                            var documentoValidacion = string.Format("{0}-{1}", ModelSesion?.venta?.prefijo ?? string.Empty, ModelSesion?.venta?.numeroVenta ?? 0).Trim('-');

                            await FacturaElectronicaNotificacionControler.RegistrarAsync(
                                DbActual,
                                ModelSesion?.venta?.id ?? 0,
                                "VALIDACION_CLIENTE",
                                "Validacion previa de cliente",
                                "La venta no cumple los requisitos minimos del cliente para facturacion electronica.",
                                detalle,
                                documentoValidacion,
                                ModelSesion?.venta?.cufe,
                                ModelSesion?.venta?.numeroVenta);

                            AlertModerno.Warning(this, "Atención", detalle, true, 3500);
                            GuardarModelsEnSesion();
                            return;
                        }
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

                venta.numeroVenta = await TablaVentasControler.Consecutivo(db, resolucion.idResolucion);
                venta.fechaVenta = DateTime.Now;

                venta.descuentoVenta = ModelSesion.venta.descuentoVenta;
                venta.efectivoVenta = ModelSesion.venta.efectivoVenta;
                venta.cambioVenta = ModelSesion.venta.cambioVenta;

                venta.estadoVenta = "CANCELADO";

                var pagoPrincipal = Pagos.FirstOrDefault();

                venta.numeroReferenciaPago = payload.idFormaDePago == 2
                    ? "-"
                    : await MediosDePagoInternos_Controler.ConsultarReferencia(
                        db,
                        pagoPrincipal != null ? pagoPrincipal.idMedioDePagointerno : 0
                    );

                venta.diasCredito = payload.idFormaDePago == 2
                    ? ModelSesion.venta.diasCredito
                    : 0;
                venta.observacionVenta = ModelSesion.venta.observacionVenta;
                venta.IdSede = ModelSesion.venta.IdSede;
                venta.guidVenta = ModelSesion.venta.guidVenta;

                venta.abonoTarjeta = abonoBanco;
                venta.propina = valorPropina;
                venta.abonoEfectivo = abonoEfectivo;

                venta.idMedioDePago = payload.idFormaDePago == 2
                    ? 0
                    : (payload.idMetodoPago > 0 ? payload.idMetodoPago : (pagoPrincipal != null ? pagoPrincipal.payment_methods_id : 0));
                venta.idResolucion = resolucion.idResolucion;
                venta.idFormaDePago = payload.idFormaDePago == 2 ? 2 : 1;

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
                ModelSesion.venta.fechaVenta = venta.fechaVenta;

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

                var pagosExistentes = await PagosVenta_controler.ConsultarListaPagos(db, ModelSesion.IdCuentaActiva);
                if (pagosExistentes.Count > 0)
                {
                    await PagosVenta_controler.CRUD(db, pagosExistentes, 2);
                }

                if (payload.idFormaDePago != 2 && Pagos.Count > 0)
                {
                    bool respPagos = await GuardarPagosVentaDirectoAsync(db, ModelSesion.IdCuentaActiva, Pagos);
                    if (!respPagos)
                    {
                        AlertModerno.Error(this, "Error", "No fue posible registrar los pagos internos en la tabla PagosVenta.", true, 2200);
                        return;
                    }
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
                        Session[SessionFacturaElectronicaKey] = false;
                        hfFacturaElectronica.Value = "false";

                        ScriptManager.RegisterStartupScript(
                            this,
                            GetType(),
                            "desactivarFeError",
                            "setTimeout(function(){ if(window.desactivarFacturaElectronicaCobro){ window.desactivarFacturaElectronicaCobro(false); } }, 100);",
                            true
                        );

                        AlertModerno.Error(this, "Error", "La factura electr\u00f3nica no fue enviada.", true, 1200);
                        GuardarModelsEnSesion();
                        DataBind();
                        return;
                    }
                }


                //aca guardamos la respuesta del cuadro de dialogo 
                var respuestaDialogo = payload.imprimirFactura;

                // ==========================================================
                // 11) Orden de impresión
                // ==========================================================
                if (respuestaDialogo)
                {
                    var printer = new ImprimirFactura { id = 0, idventa = venta.id };
                    PuntoDePagoPrinterHelper.Apply(printer, Session, ModelSesion);
                    var ordenPrinter = await ImprimirFacturaControler.CRUD(db, printer, 0);

                    if (!ordenPrinter.estado)
                    {
                        AlertModerno.Error(this, "Error", "La factura no se pudo imprimir.", true, 1200);
                        GuardarModelsEnSesion();
                        DataBind();
                        return;
                    }
                }

                //cargamos orden para abrir el cajon
                var respCajon = await AperturarCajonRequestHelper.EnviarAsync(Session["db"].ToString(), Session, ModelSesion);

                var liberacionMesas = await LiberarMesasDeLaVenta(db, venta.id);

                // ==========================================================
                // 12) Final OK
                // ==========================================================
                Session[SessionFacturaElectronicaKey] = false;
                hfFacturaElectronica.Value = "false";
                Session.Remove(SessionCajaMesasKey);

                if (liberacionMesas.estado)
                {
                    AlertModerno.Success(this, "OK", "Datos de cobro recibidos correctamente.", true, 1200);
                }
                else
                {
                    AlertModerno.Warning(this, "Atenci\u00f3n", $"El cobro fue registrado, pero hubo novedades al liberar la mesa: {liberacionMesas.mensaje}", true, 2200);
                }

                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "redirOK",
                    "setTimeout(function(){ if(window.desactivarFacturaElectronicaCobro){ window.desactivarFacturaElectronicaCobro(true); } if(window.bloquearCobroHastaSalir){ window.bloquearCobroHastaSalir('Finalizando cobro'); } }, 1150); setTimeout(function(){ window.location.href='caja.aspx'; }, 1500);",
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

        private async Task<(bool estado, string mensaje)> LiberarMesasDeLaVenta(string db, int idVenta)
        {
            var relaciones = await R_VentaMesaControler.ListaRelacion(db, idVenta) ?? new List<R_VentaMesa>();
            if (relaciones.Count == 0)
            {
                return (true, string.Empty);
            }

            foreach (var relacion in relaciones)
            {
                if (relacion == null)
                {
                    continue;
                }

                var mesa = await MesasControler.Consultar_id(db, relacion.idMesa);
                if (mesa != null)
                {
                    mesa.estadoMesa = 0;
                    var actualizarMesa = await MesasControler.CRUD(db, mesa, 1);
                    if (!actualizarMesa.estado)
                    {
                        return (false, $"No fue posible liberar la mesa #{mesa.id}.");
                    }
                }

                var eliminarRelacion = await R_VentaMesaControler.CRUD(db, relacion, 2);
                if (!eliminarRelacion.estado)
                {
                    return (false, $"No fue posible eliminar la relaci\u00f3n de la venta #{idVenta} con la mesa #{relacion.idMesa}.");
                }
            }

            return (true, string.Empty);
        }

        public class GuardarCobroPayload
        {
            public int efectivo { get; set; }
            public int cambio { get; set; }
            public bool facturaElectronica { get; set; }
            public bool imprimirFactura { get; set; }
            public int idFormaDePago { get; set; }
            public bool esCredito { get; set; }
            public int idMetodoPago { get; set; }
            public int idVenta { get; set; }
        }

        private void GuardarModelsEnSesion()
        {
            SessionContextHelper.ApplyOperationalContext(Session, ModelSesion);
        }

        private async Task<int> ObtenerClienteSeleccionadoIdAsync()
        {
            if (Session["cliente_seleccionado_id"] != null
                && int.TryParse(Session["cliente_seleccionado_id"].ToString(), out var clienteIdSesion)
                && clienteIdSesion > 0)
            {
                return clienteIdSesion;
            }

            if (ModelSesion?.venta?.idCliente > 0)
            {
                return ModelSesion.venta.idCliente;
            }

            if (ModelSesion?.venta?.id > 0)
            {
                var relacion = await R_VentaCliente_Controler.ConsultarRelacion(DbActual, ModelSesion.venta.id);
                if (relacion != null && relacion.idCliente > 0)
                {
                    return relacion.idCliente;
                }
            }

            return 0;
        }

        private async Task<bool> PersistirClienteEnVentaAsync(int clienteId)
        {
            if (clienteId <= 0 || ModelSesion?.venta == null)
            {
                return false;
            }

            if (ModelSesion.venta.idCliente == clienteId)
            {
                return true;
            }

            ModelSesion.venta.idCliente = clienteId;
            ModelSesion.venta = await V_TablaVentasControler.Consultar_Id(DbActual, ModelSesion.venta.id) ?? ModelSesion.venta;
            ModelSesion.venta.idCliente = clienteId;
            GuardarModelsEnSesion();
            return true;
        }

        private List<string> ValidarClienteParaFacturaElectronica(Clientes cliente)
        {
            var errores = new List<string>();

            if (cliente == null)
            {
                errores.Add("No fue posible cargar el cliente seleccionado.");
                return errores;
            }

            if (string.IsNullOrWhiteSpace(cliente.identificationNumber))
            {
                errores.Add("El cliente no tiene identificación configurada.");
            }

            if (string.IsNullOrWhiteSpace(cliente.nameCliente))
            {
                errores.Add("El cliente no tiene nombre o razón social configurada.");
            }

            if (cliente.municipality_id <= 0)
            {
                errores.Add("El cliente no tiene municipio DIAN configurado.");
            }

            if (string.IsNullOrWhiteSpace(cliente.adress))
            {
                errores.Add("El cliente no tiene dirección configurada.");
            }

            if (string.IsNullOrWhiteSpace(cliente.email))
            {
                errores.Add("El cliente no tiene correo configurado.");
            }

            if (cliente.typeDocumentIdentification_id <= 0)
            {
                errores.Add("El cliente no tiene tipo de documento configurado.");
            }

            if (cliente.typeOrganization_id <= 0)
            {
                errores.Add("El cliente no tiene tipo de organización configurado.");
            }

            return errores;
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

            if (!await PersistirClienteEnVentaAsync(clienteId))
            {
                AlertModerno.Error(this, "Error", "El cliente se relacionó, pero no fue posible actualizar la venta principal.", true);
                return;
            }

            Session["cliente_seleccionado_id"] = clienteId;
            Session["cliente_seleccionado_nombre"] = cliente.nameCliente ?? "";
            Session["cliente_seleccionado_nit"] = cliente.identificationNumber ?? "";
            Session["cliente_seleccionado_correo"] = cliente.email ?? "";

            cliente_seleccionado_nit.Text = cliente.identificationNumber ?? "";
            cliente_seleccionado_nombre.Text = cliente.nameCliente ?? "";
            cliente_seleccionado_correo.Text = cliente.email ?? "";

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

                ModelSesion.clientes = await ObtenerClientesCobroAsync(true);
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

                var respuestaCRUD = await GuardarPropinaUnica(idVentaActual, valor);

                if (!respuestaCRUD)
                {
                    AlertModerno.Error(this, "\u00a1Error!", "No fue posible agregar la propina.", true);
                    return;
                }

                var ventaTabla = await TablaVentasControler.ConsultarIdVenta(db, idVentaActual);
                if (ventaTabla != null)
                {
                    ventaTabla.porpropina = pct / 100m;
                    await TablaVentasControler.CRUD(db, ventaTabla, 1);
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

                var ventaTabla = await TablaVentasControler.ConsultarIdVenta(Session["db"].ToString(), idventa);
                if (ventaTabla != null)
                {
                    ventaTabla.porpropina = 0;
                    await TablaVentasControler.CRUD(Session["db"].ToString(), ventaTabla, 1);
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

                if (idMedioInterno <= 0 || idMetodoPago <= 0)
                {
                    return;
                }

                var pagoventa = CrearPagoVenta(
                    VentaActual.id,
                    idMedioInterno,
                    idMetodoPago,
                    Convert.ToDecimal(VentaActual.total_A_Pagar));

                GuardarPagoVentaEnSesion(new List<PagosVenta> { pagoventa });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error btnSeleccionarPagoInterno: " + ex.Message);
            }
        }

        private async Task CargarMediosPago()
        {
            BindMediosPago(await ObtenerMediosPagoAsync());
        }

        private async Task CargarTiposDocumento()
        {
            _tiposDocumento = await ObtenerTiposDocumentoAsync();
            BindTiposDocumento(_tiposDocumento);
        }

        private async Task CargarMunicipios()
        {
            BindMunicipios(await ObtenerMunicipiosAsync());
        }

        private async Task CargarTiposRegimen()
        {
            BindTiposRegimen(await ObtenerTiposRegimenAsync());
        }

        private async Task CargarTiposResponsabilidad()
        {
            BindTiposResponsabilidad(await ObtenerTiposResponsabilidadAsync());
        }

        private async Task CargarDetallesImpuesto()
        {
            BindDetallesImpuesto(await ObtenerDetallesImpuestoAsync());
        }

        private async Task CargarTiposOrganizacion()
        {
            BindTiposOrganizacion(await ObtenerTiposOrganizacionAsync());
        }

        private async Task CargarClientesModal()
        {
            ModelSesion.clientes = await ObtenerClientesCobroAsync();
            if (_tiposDocumento == null)
            {
                _tiposDocumento = await ObtenerTiposDocumentoAsync();
            }

            BindClientesModal();
        }

        private void BindClientesModal()
        {
            var clientes = ModelSesion?.clientes ?? new List<Clientes>();

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

        private async Task btnGuardarPagoMixto(string eventArgument)
        {
            try
            {
                if (VentaActual == null) return;
                if (string.IsNullOrWhiteSpace(eventArgument)) return;

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
                if (payload?.pagos == null || payload.pagos.Count == 0) return;

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

                if (listaPagos.Count == 0) return;

                decimal totalAPagar = Convert.ToDecimal(VentaActual.total_A_Pagar);
                decimal saldo = totalAPagar - sumaInternos;
                if (saldo < 0) saldo = 0;

                if (saldo > 0)
                {
                    var idMedioInternoSaldo = await ObtenerIdMedioInternoPorMetodoAsync(idMetodoPago);
                    if (idMedioInternoSaldo <= 0)
                    {
                        return;
                    }

                    Session["saldo"] = Convert.ToInt32(Math.Round(saldo));
                    listaPagos.Add(new PagosVenta
                    {
                        id = 0,
                        idMedioDePagointerno = idMedioInternoSaldo,
                        idVenta = VentaActual.id,
                        payment_methods_id = idMetodoPago,
                        valorPago = Convert.ToInt32(Math.Round(saldo))
                    });
                }

                GuardarPagoVentaEnSesion(listaPagos);
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error btnGuardarPagoMixto: " + ex.Message);
                return;
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
            var porcentajePropina = Convert.ToInt32(Math.Round(VentaActual.por_propina * 100m, 0));

            Session["propina_valor"] = Convert.ToInt32(Math.Round(VentaActual.propina, 0));
            Session["propina_pct"] = porcentajePropina;

            txtSubTotal.Value = VentaActual.subtotalVenta.ToString("N0", co);
            txtIVA.Value = VentaActual.ivaVenta.ToString("N0", co);
            txtDescuento.Value = VentaActual.descuentoVenta.ToString("N0", co);
            txtPropina.Value = VentaActual.propina.ToString("N0", co);

            lblTotalGrande.InnerText = VentaActual.total_A_Pagar.ToString("N0", co);

            txtEfectivo.Value = VentaActual.efectivoVenta.ToString("N0", co);
            txtCambio.Value = VentaActual.cambioVenta.ToString("N0", co);

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "SetPropinaPctDesdeVista",
                $"(function(){{var el=document.getElementById('txtPropinaPorcentaje'); if(el) el.value='{porcentajePropina}';}})();",
                true
            );
        }

        private async Task CargarRelMediosInternos()
        {
            hfRelMediosInternos.Value = JsonConvert.SerializeObject(await ObtenerRelMediosInternosAsync());
        }

        private async Task<List<payment_methods>> ObtenerMediosPagoAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCobrarMediosPagoKey] is List<payment_methods> cache)
            {
                return cache;
            }

            var lista = await payment_methodsControler.ListaMetodosDePago(DbActual) ?? new List<payment_methods>();
            Session[SessionCobrarMediosPagoKey] = lista;
            return lista;
        }

        private async Task<List<type_document_identifications>> ObtenerTiposDocumentoAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCobrarTiposDocumentoKey] is List<type_document_identifications> cache)
            {
                return cache;
            }

            var lista = await type_document_identificationsControler.ListaTiposDocumento(DbActual) ?? new List<type_document_identifications>();
            Session[SessionCobrarTiposDocumentoKey] = lista;
            return lista;
        }

        private async Task<List<type_organizations>> ObtenerTiposOrganizacionAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCobrarTiposOrganizacionKey] is List<type_organizations> cache)
            {
                return cache;
            }

            var lista = await type_organizationsControler.ListaTiposOrganizacion(DbActual) ?? new List<type_organizations>();
            Session[SessionCobrarTiposOrganizacionKey] = lista;
            return lista;
        }

        private async Task<List<V_Municipios>> ObtenerMunicipiosAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCobrarMunicipiosKey] is List<V_Municipios> cache)
            {
                return cache;
            }

            var lista = await V_MunicipiosControler.ListaMunicipios(DbActual) ?? new List<V_Municipios>();
            Session[SessionCobrarMunicipiosKey] = lista;
            return lista;
        }

        private async Task<List<type_regimes>> ObtenerTiposRegimenAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCobrarTiposRegimenKey] is List<type_regimes> cache)
            {
                return cache;
            }

            var lista = await type_regimesControler.ListaTiposRegimen(DbActual) ?? new List<type_regimes>();
            Session[SessionCobrarTiposRegimenKey] = lista;
            return lista;
        }

        private async Task<List<type_liabilities>> ObtenerTiposResponsabilidadAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCobrarTiposResponsabilidadKey] is List<type_liabilities> cache)
            {
                return cache;
            }

            var lista = await type_liabilitiesControler.ListaTiposResponsabilidad(DbActual) ?? new List<type_liabilities>();
            Session[SessionCobrarTiposResponsabilidadKey] = lista;
            return lista;
        }

        private async Task<List<tax_details>> ObtenerDetallesImpuestoAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCobrarDetallesImpuestoKey] is List<tax_details> cache)
            {
                return cache;
            }

            var lista = await tax_detailsControler.ListaDetallesImpuesto(DbActual) ?? new List<tax_details>();
            Session[SessionCobrarDetallesImpuestoKey] = lista;
            return lista;
        }

        private async Task<List<Clientes>> ObtenerClientesCobroAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && ModelSesion?.clientes != null && ModelSesion.clientes.Any())
            {
                return ModelSesion.clientes;
            }

            if (!forceRefresh && Session[SessionCobrarClientesKey] is List<Clientes> cache)
            {
                return cache;
            }

            var lista = await ClientesControler.ListaClientes(DbActual) ?? new List<Clientes>();
            Session[SessionCobrarClientesKey] = lista;
            return lista;
        }

        private async Task<List<V_R_MediosDePago_MediosDePagoInternos>> ObtenerRelMediosInternosAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCobrarRelMediosInternosKey] is List<V_R_MediosDePago_MediosDePagoInternos> cache)
            {
                return cache;
            }

            var lista = await V_R_MediosDePago_MediosDePagoInternosControler.GetAll(DbActual) ?? new List<V_R_MediosDePago_MediosDePagoInternos>();
            Session[SessionCobrarRelMediosInternosKey] = lista;
            return lista;
        }

        private void BindMediosPago(List<payment_methods> paymentMethods)
        {
            var listaActivos = (paymentMethods ?? new List<payment_methods>())
                .Where(x => x != null && x.state)
                .ToList();

            ddlMedioPago.DataSource = listaActivos;
            ddlMedioPago.DataTextField = "name";
            ddlMedioPago.DataValueField = "id";
            ddlMedioPago.DataBind();

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

            var efectivoItem = ddlMedioPago.Items.Cast<System.Web.UI.WebControls.ListItem>()
                .FirstOrDefault(i => (i.Text ?? "").Trim().ToLower().Contains("efectivo"));

            if (efectivoItem != null)
            {
                ddlMedioPago.ClearSelection();
                efectivoItem.Selected = true;
                return;
            }

            if (ddlMedioPago.Items.Count > 0)
            {
                ddlMedioPago.SelectedIndex = 0;
            }
        }

        private void BindTiposDocumento(List<type_document_identifications> tipos)
        {
            ddlTipoDocumento.DataSource = tipos;
            ddlTipoDocumento.DataTextField = "name";
            ddlTipoDocumento.DataValueField = "id";
            ddlTipoDocumento.DataBind();

            if (ddlTipoDocumento.Items.Count == 0)
            {
                ddlTipoDocumento.Items.Add(new System.Web.UI.WebControls.ListItem("Sin datos", ""));
            }
        }

        private void BindMunicipios(List<V_Municipios> municipios)
        {
            ddlMunicipio.DataSource = municipios;
            ddlMunicipio.DataTextField = "name";
            ddlMunicipio.DataValueField = "idMunicipio";
            ddlMunicipio.DataBind();

            if (ddlMunicipio.Items.Count == 0)
            {
                ddlMunicipio.Items.Add(new System.Web.UI.WebControls.ListItem("Sin datos", ""));
            }
        }

        private void BindTiposRegimen(List<type_regimes> tipos)
        {
            ddlTipoRegimen.DataSource = tipos;
            ddlTipoRegimen.DataTextField = "name";
            ddlTipoRegimen.DataValueField = "id";
            ddlTipoRegimen.DataBind();

            if (ddlTipoRegimen.Items.Count == 0)
            {
                ddlTipoRegimen.Items.Add(new System.Web.UI.WebControls.ListItem("Sin datos", ""));
            }
        }

        private void BindTiposResponsabilidad(List<type_liabilities> tipos)
        {
            ddlTipoResponsabilidad.DataSource = tipos;
            ddlTipoResponsabilidad.DataTextField = "name";
            ddlTipoResponsabilidad.DataValueField = "id";
            ddlTipoResponsabilidad.DataBind();

            if (ddlTipoResponsabilidad.Items.Count == 0)
            {
                ddlTipoResponsabilidad.Items.Add(new System.Web.UI.WebControls.ListItem("Sin datos", ""));
            }
        }

        private void BindDetallesImpuesto(List<tax_details> detalles)
        {
            ddlDetalleImpuesto.DataSource = detalles;
            ddlDetalleImpuesto.DataTextField = "name";
            ddlDetalleImpuesto.DataValueField = "id";
            ddlDetalleImpuesto.DataBind();

            if (ddlDetalleImpuesto.Items.Count == 0)
            {
                ddlDetalleImpuesto.Items.Add(new System.Web.UI.WebControls.ListItem("Sin datos", ""));
            }
        }

        private void BindTiposOrganizacion(List<type_organizations> tipos)
        {
            ddlTipoOrganizacion.DataSource = tipos;
            ddlTipoOrganizacion.DataTextField = "name";
            ddlTipoOrganizacion.DataValueField = "id";
            ddlTipoOrganizacion.DataBind();

            if (ddlTipoOrganizacion.Items.Count == 0)
            {
                ddlTipoOrganizacion.Items.Add(new System.Web.UI.WebControls.ListItem("Sin datos", ""));
            }
        }

        private async Task AsegurarPagoJsonInicial(bool forceRefresh = false)
        {
            try
            {
                if (VentaActual == null) return;

                // Si ya existe, no hacer nada
                if (!forceRefresh && Session["PagoVentaJSON"] != null) return;

                int idMetodoPago = 0;
                int.TryParse(ddlMedioPago.SelectedValue, out idMetodoPago);
                if (idMetodoPago <= 0) return;

                int idMedioInterno = await ObtenerIdMedioInternoPorMetodoAsync(idMetodoPago);
                if (idMedioInterno <= 0) return;

                var pagoventa = CrearPagoVenta(
                    VentaActual.id,
                    idMedioInterno,
                    idMetodoPago,
                    Convert.ToDecimal(VentaActual.total_A_Pagar));

                GuardarPagoVentaEnSesion(new List<PagosVenta> { pagoventa });
            }
            catch
            {
            }
        }

        private async Task<List<PagosVenta>> ObtenerPagosCobroAsync(int idMetodoPago)
        {
            if (VentaActual == null)
            {
                return new List<PagosVenta>();
            }

            if (idMetodoPago <= 0)
            {
                int.TryParse(ddlMedioPago.SelectedValue, out idMetodoPago);
            }

            var pagos = DeserializarPagosVenta(Session["PagoVentaJSON"]?.ToString())
                .Where(x => x != null && x.idMedioDePagointerno > 0 && x.valorPago > 0)
                .Select(x => new PagosVenta
                {
                    id = x.id,
                    idVenta = VentaActual.id,
                    idMedioDePagointerno = x.idMedioDePagointerno,
                    payment_methods_id = x.payment_methods_id,
                    valorPago = x.valorPago
                })
                .ToList();

            if (idMetodoPago > 0 && pagos.Count > 0 && pagos.All(x => x.payment_methods_id != idMetodoPago))
            {
                pagos.Clear();
            }

            if (pagos.Count > 0)
            {
                return pagos;
            }

            await AsegurarPagoJsonInicial(true);

            return DeserializarPagosVenta(Session["PagoVentaJSON"]?.ToString())
                .Where(x => x != null && x.idMedioDePagointerno > 0 && x.valorPago > 0)
                .Select(x => new PagosVenta
                {
                    id = x.id,
                    idVenta = VentaActual.id,
                    idMedioDePagointerno = x.idMedioDePagointerno,
                    payment_methods_id = x.payment_methods_id,
                    valorPago = x.valorPago
                })
                .ToList();
        }

        private List<PagosVenta> DeserializarPagosVenta(string pagojson)
        {
            if (string.IsNullOrWhiteSpace(pagojson))
            {
                return new List<PagosVenta>();
            }

            try
            {
                var trimmed = pagojson.Trim();
                if (trimmed.StartsWith("["))
                {
                    return JsonConvert.DeserializeObject<List<PagosVenta>>(trimmed) ?? new List<PagosVenta>();
                }

                var item = JsonConvert.DeserializeObject<PagosVenta>(trimmed);
                return item != null ? new List<PagosVenta> { item } : new List<PagosVenta>();
            }
            catch
            {
                return new List<PagosVenta>();
            }
        }

        private void GuardarPagoVentaEnSesion(List<PagosVenta> pagos)
        {
            Session["PagoVentaJSON"] = JsonConvert.SerializeObject(pagos ?? new List<PagosVenta>());
        }

        private PagosVenta CrearPagoVenta(int idVenta, int idMedioInterno, int idMetodoPago, decimal valorPago)
        {
            return new PagosVenta
            {
                id = 0,
                idVenta = idVenta,
                idMedioDePagointerno = idMedioInterno,
                payment_methods_id = idMetodoPago,
                valorPago = valorPago
            };
        }

        private async Task<int> ObtenerIdMedioInternoPorMetodoAsync(int idMetodoPago)
        {
            if (idMetodoPago <= 0)
            {
                return 0;
            }

            var rel = await ObtenerRelMediosInternosAsync();
            return (rel ?? new List<V_R_MediosDePago_MediosDePagoInternos>())
                .Where(x => x != null && x.idMedioDePago == idMetodoPago && x.idMediosDePagoInternos > 0)
                .Select(x => x.idMediosDePagoInternos)
                .FirstOrDefault();
        }

        private async Task<bool> GuardarPagosVentaDirectoAsync(string db, int idVenta, List<PagosVenta> pagos)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(db) || idVenta <= 0 || pagos == null || pagos.Count == 0)
                {
                    return false;
                }

                var pagosValidos = pagos
                    .Where(x => x != null && x.idMedioDePagointerno > 0 && x.payment_methods_id > 0 && x.valorPago > 0)
                    .ToList();

                if (pagosValidos.Count == 0)
                {
                    return false;
                }

                var valores = string.Join(",",
                    pagosValidos.Select(x =>
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "({0}, {1}, {2}, {3})",
                            idVenta,
                            x.idMedioDePagointerno,
                            x.valorPago,
                            x.payment_methods_id)));

                var sql = $@"
BEGIN TRY
    BEGIN TRANSACTION;
    DELETE FROM PagosVenta WHERE idVenta = {idVenta};
    INSERT INTO PagosVenta (idVenta, idMedioDePagointerno, valorPago, payment_methods_id)
    VALUES {valores};
    COMMIT TRANSACTION;
    SELECT CAST(1 AS bit) AS estado, 'OK' AS mensaje;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    SELECT CAST(0 AS bit) AS estado, ERROR_MESSAGE() AS mensaje;
END CATCH";

                var dal = new SqlAutoDAL();
                var resp = await dal.EjecutarSQLObjeto<RespuestaCRUD>(db, sql);
                return resp != null && resp.estado;
            }
            catch
            {
                return false;
            }
        }
    }
}







