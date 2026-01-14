using DAL.Controler;
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using WebApplication.ViewModels;

namespace WebApplication
{
    public partial class Cobrar : System.Web.UI.Page
    {
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
                CargarDatosVenta();
                await CargarRelMediosInternos();

                hfIdVentaActual.Value = VentaActual.id.ToString();
            }
            else
            {
                // ✅ Captura __EVENTTARGET/__EVENTARGUMENT desde __doPostBack
                await ProcesarPostBack();
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

                default:
                    break;
            }
        }

        // ========= DESCUENTO =========
        // eventArgument: "valor|razon"
        private Task btnGuardarDescuento(string eventArgument)
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

                // (Opcional) refrescar UI server-side si lo necesitas
                // txtDescuento.Value = valor.ToString();
                // txtRazonDescuento.Value = razon;

                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        private Task btnEliminarDescuento(string eventArgument)
        {
            try
            {
                Session["descuento_valor"] = 0;
                Session["descuento_razon"] = "";

                // (Opcional) reflejar en UI
                // txtDescuento.Value = "0";
                // txtRazonDescuento.Value = "";

                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        // ========= PROPINA =========
        // eventArgument: "valor"
        private Task btnGuardarPropina(string eventArgument)
        {
            try
            {
                int valor = 0;
                int.TryParse(eventArgument ?? "0", out valor);

                Session["propina_valor"] = valor;

                // (Opcional) reflejar en UI
                // txtPropina.Value = valor.ToString();

                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }

        private Task btnEliminarPropina(string eventArgument)
        {
            try
            {
                Session["propina_valor"] = 0;

                // (Opcional) reflejar en UI
                // txtPropina.Value = "0";

                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
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
