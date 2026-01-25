using DAL;
using DAL.Controler;
using DAL.Model;
using Newtonsoft.Json;
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
                var json = Session["modelsJSON"] as string;
                if (string.IsNullOrWhiteSpace(json)) return null;

                try { return JsonConvert.DeserializeObject<MenuViewModels>(json); }
                catch { return null; }
            }
        }

        private V_TablaVentas VentaActual => ModelSesion?.venta;

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (ModelSesion == null || VentaActual == null || Session["db"] == null)
                {
                    Response.Redirect("~/Menu.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

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

                ModelSesion.cargoDescuentoVentas = await CargoDescuentoVentasControler.ObtenerPorVenta(Session["db"].ToString(), ModelSesion.venta.id);
                CargoDescuentoVentas descuento_ = new CargoDescuentoVentas();
                descuento_ = ModelSesion.cargoDescuentoVentas.Where(X => X.tipo == false).FirstOrDefault();
                if (descuento_ != null)
                {
                    Session["descuento_valor"] = descuento_.valor;
                    Session["descuento_razon"] = descuento_.razon;
                }
                else
                {
                    Session["descuento_valor"] = 0;
                    Session["descuento_razon"] = "";
                }

                descuento_ = new CargoDescuentoVentas();
                descuento_ = ModelSesion.cargoDescuentoVentas.Where(X => X.tipo == true).FirstOrDefault();
                if (descuento_ != null)
                {
                    Session["propina_valor"] = descuento_.valor;
                }
                else
                {
                    Session["propina_valor"] = 0;
                }


                hfIdVentaActual.Value = VentaActual.id.ToString();
            }
            else
            {
                // ✅ Captura __EVENTTARGET/__EVENTARGUMENT desde __doPostBack
                await ProcesarPostBack();
            }
            int descu=Convert.ToInt32(Session["descuento_valor"]);
            int pro=Convert.ToInt32(Session["propina_valor"]);
            if (Session["saldo"] != null)
            {
                int saldo = Convert.ToInt32(Session["saldo"]);
                txtEfectivo.Value = Convert.ToString(saldo);
            }
            txtRazonDescuento.Value= Session["descuento_razon"]?.ToString() ?? "";
            txtDescuento.Value = Convert.ToString(descu);
            txtPropina.Value = Convert.ToString(pro);
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

                default:
                    break;
            }
        }
        private void GuardarModelsEnSesion()
        {
            Session[SessionModelsKey] = ModelSesion;
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
                        var limpiarScript = $"Swal.fire({{icon:'error',title:'¡Error!',text:'El documento no se encuentra registrado ni en la base de datos ni en la DIAN. Debes crearlo manualmente.',confirmButtonColor:'#2563eb'}}).then(function(){{if(window.setClienteData){{window.setClienteData({limpiarJson});}} var modalEl=document.getElementById('mdlCliente'); if(modalEl){{bootstrap.Modal.getOrCreateInstance(modalEl).show();}}}});";

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
                    "Swal.fire({icon:'error',title:'¡Error!',text:'No fue posible consultar el NIT.',confirmButtonColor:'#2563eb'}).then(function(){var modalEl=document.getElementById('mdlCliente'); if(modalEl){bootstrap.Modal.getOrCreateInstance(modalEl).show();}});",
                    true);
            }

            return;
        }

        private Task<Acquirer_Response> Consultar_NIT_DIAN(int nit)
        {
            return Task.FromResult<Acquirer_Response>(null);
        }
        // ========= DESCUENTO =========
        // eventArgument: "valor|razon"
        private async Task btnGuardarDescuento(string eventArgument)
        {
            try
            {
               
                int valor = 0;
                string razon = "";

                if (!string.IsNullOrWhiteSpace(eventArgument))
                {
                    var parts = eventArgument.Split('|');
                    if (parts.Length >= 1) int.TryParse(parts[0], out valor);
                    if (parts.Length >= 2) razon = parts[1] ?? "";
                }

                // ✅ Guardamos en Session (puedes cambiar el nombre si quieres)
                Session["descuento_valor"] = valor;
                Session["descuento_razon"] = razon;

                var descuento = new CargoDescuentoVentas
                {
                    id = 0,
                    idVenta = ModelSesion.venta.id,
                    tipo = false,
                    codigo = 1,
                    razon = razon,
                    valor = valor,
                    baseCD = ModelSesion.venta.totalVenta,
                    descripcionCargoDescuento = razon
                };

                var respuestaCRUD = await CargoDescuentoVentasControler.CRUD(Session["db"].ToString(),descuento,0);

                //en esta parte actualisamos la venta para que se refleje el descuento
                if (!respuestaCRUD)
                {
                    AlertModerno.Error(this, "¡Error!", "No fue posible agregar el descuento.", true);
                    return;
                }

                ModelSesion.venta = new V_TablaVentas();
                ModelSesion.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), ModelSesion.venta.id);
                ModelSesion.cargoDescuentoVentas =await CargoDescuentoVentasControler.ObtenerPorVenta(Session["db"].ToString(), ModelSesion.venta.id);

                AlertModerno.Success(this, "¡OK!", $"Descuento agregado con éxito", true, 800);

                GuardarModelsEnSesion();
                DataBind();
            }
            catch
            {
                AlertModerno.Error(this, "¡Error!", "No fue posible crear la relación del vendedor con la venta.", true);
            }
        }

        private async Task btnEliminarDescuento(string eventArgument)
        {
            try
            {
                Session["descuento_valor"] = 0;
                Session["descuento_razon"] = "";
                
                int idventa=ModelSesion.venta.id;

                var dal = new SqlAutoDAL();
                var r = await dal.EjecutarSQLObjeto<RespuestaCRUD>(Session["db"].ToString(), $"EXEC dbo.DELETE_CargoDescuentoVentas {idventa},0;");

                if(r.estado==false)
                {
                    AlertModerno.Error(this, "¡Error!", "No fue posible eliminar el descuento.", true);
                    return;
                }

                ModelSesion.venta = new V_TablaVentas();
                ModelSesion.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), idventa);
                AlertModerno.Success(this, "¡OK!", $"Descuento eliminado con éxito", true, 800);
                GuardarModelsEnSesion();
                DataBind();

            }
            catch
            {
                AlertModerno.Error(this, "¡Error!", "No fue posible eliminar el descuento.", true);
            }
        }

        // ========= PROPINA =========
        // eventArgument: "valor"
        private async Task btnGuardarPropina(string eventArgument)
        {
            try
            {
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
                    idVenta = ModelSesion.venta.id,
                    tipo = true,
                    codigo = 1,
                    razon = "propina",
                    valor = valor,
                    baseCD = ModelSesion.venta.totalVenta,
                    descripcionCargoDescuento = "propina"
                };

                var respuestaCRUD = await CargoDescuentoVentasControler.CRUD(Session["db"].ToString(), descuento, 0);

                //en esta parte actualisamos la venta para que se refleje el descuento
                if (!respuestaCRUD)
                {
                    AlertModerno.Error(this, "¡Error!", "No fue posible agregar la propina.", true);
                    return;
                }

                ModelSesion.venta = new V_TablaVentas();
                ModelSesion.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), ModelSesion.venta.id);
                ModelSesion.cargoDescuentoVentas = await CargoDescuentoVentasControler.ObtenerPorVenta(Session["db"].ToString(), ModelSesion.venta.id);

                AlertModerno.Success(this, "¡OK!", $"propina agregado con éxito", true, 800);

                GuardarModelsEnSesion();
                DataBind();
            }
            catch
            {
                AlertModerno.Error(this, "¡Error!", "No fue posible agregar la propina.", true);
            }
        }


        private async Task btnEliminarPropina(string eventArgument)
        {
            try
            {
                Session["descuento_valor"] = 0;
                Session["descuento_razon"] = "";

                int idventa = ModelSesion.venta.id;

                var dal = new SqlAutoDAL();
                var r = await dal.EjecutarSQLObjeto<RespuestaCRUD>(Session["db"].ToString(), $"EXEC dbo.DELETE_CargoDescuentoVentas {idventa},1;");

                if (r.estado == false)
                {
                    AlertModerno.Error(this, "¡Error!", "No fue posible eliminar la propina.", true);
                    return;
                }

                ModelSesion.venta = new V_TablaVentas();
                ModelSesion.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), idventa);
                AlertModerno.Success(this, "¡OK!", $"Propina eliminado con éxito", true, 800);
                GuardarModelsEnSesion();
                DataBind();

            }
            catch
            {
                AlertModerno.Error(this, "¡Error!", "No fue posible eliminar la propina.", true);
            }
        }

        // ✅ Recibe: "idInterno|idMetodoPago"
        private async Task btnSeleccionarPagoInterno(string eventArgument)
        {
            try
            {
                // Seguridad
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

                var pagoventa = new PagosVenta { id=0, idMedioDePagointerno=idMedioInterno, idVenta=VentaActual.id, payment_methods_id=idMetodoPago, valorPago=Convert.ToInt32(VentaActual.total_A_Pagar) };

                var pagosJSON=JsonConvert.SerializeObject(pagoventa);

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

        private class ClienteModalItem
        {
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

                // 1) Base64 -> JSON (payload)
                string json;
                try
                {
                    var bytes = Convert.FromBase64String(eventArgument);
                    json = System.Text.Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                    json = eventArgument; // fallback
                }

                var payload = JsonConvert.DeserializeObject<PagoMixtoPayload>(json);
                if (payload?.pagos == null || payload.pagos.Count == 0) return Task.CompletedTask;

                int idMetodoPago = payload.idMetodoPago;

                // 2) Construir lista de pagos y sumar internos
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

                // 3) Calcular saldo y agregar EFECTIVO (restante)
                decimal totalAPagar = Convert.ToDecimal(VentaActual.total_A_Pagar);
                decimal saldo = totalAPagar - sumaInternos;
                if (saldo < 0) saldo = 0;

                // ⚠️ Necesitas el ID del medio interno EFECTIVO
                // O lo guardas en Session, o lo pones fijo aquí temporalmente.
                int idMedioInternoEfectivo = 0;

                // opción: desde session
                if (Session["idMedioInternoEfectivo"] != null)
                    int.TryParse(Session["idMedioInternoEfectivo"].ToString(), out idMedioInternoEfectivo);

                // opción temporal (AJUSTA)

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

                // 4) Guardar TODO en la MISMA session
                // ✅ Aquí va la clave: ahora guardamos una LISTA en el mismo key

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

        // ====== Modelos para el payload ======
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
                await V_R_MediosDePago_MediosDePagoInternosControler.GetAll(Session["db"].ToString());

            hfRelMediosInternos.Value = JsonConvert.SerializeObject(rel ?? new List<V_R_MediosDePago_MediosDePagoInternos>());
        }
    }
}
