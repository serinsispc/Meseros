using DAL;
using DAL.Controler;
using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Class;
using WebApplication.Helpers;
using WebApplication.ViewModels;

namespace WebApplication
{
    public partial class CerrarCaja : System.Web.UI.Page
    {
        private MenuViewModels models = new MenuViewModels();
        private string db;
        private BaseCaja baseCaja;
        private List<V_TablaVentas> ventas = new List<V_TablaVentas>();

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!CargarContexto())
                {
                    AlertModerno.ErrorRedirect(this, "Error", "La sesion expiro o no contiene el contexto de trabajo.", "Default.aspx");
                    return;
                }

                await InicializarPagina();
            }
        }

        private bool CargarContexto()
        {
            models = SessionContextHelper.LoadModels(Session) ?? new MenuViewModels();
            db = Convert.ToString(Session[SessionContextHelper.DbKey] ?? models.db);
            if (string.IsNullOrWhiteSpace(db))
            {
                return false;
            }

            models.db = db;
            baseCaja = models.BaseCaja;
            if ((baseCaja == null || baseCaja.id <= 0) && Session[SessionContextHelper.BaseCajaKey] is string baseJson && !string.IsNullOrWhiteSpace(baseJson))
            {
                baseCaja = Newtonsoft.Json.JsonConvert.DeserializeObject<BaseCaja>(baseJson);
                models.BaseCaja = baseCaja;
            }

            return baseCaja != null && baseCaja.id > 0;
        }

        private async Task InicializarPagina()
        {
            var dal = new SqlAutoDAL();
            ventas = await dal.ConsultarLista<V_TablaVentas>(db, x => x.idBaseCaja == baseCaja.id && x.eliminada == false);
            ventas = ventas.OrderByDescending(x => x.fechaVenta).ToList();

            var totalIngresos = ventas.Where(x => !string.Equals(x.estadoVenta, "CANCELADO", StringComparison.OrdinalIgnoreCase)).Sum(x => x.total_A_Pagar);
            var totalEfectivo = ventas.Where(x => !string.Equals(x.estadoVenta, "CANCELADO", StringComparison.OrdinalIgnoreCase)).Sum(x => x.abonoEfectivo);
            var ventasTarjeta = ventas.Where(x => !string.Equals(x.estadoVenta, "CANCELADO", StringComparison.OrdinalIgnoreCase)).Sum(x => x.abonoTarjeta);
            var ventasCredito = ventas.Where(x => !string.Equals(x.estadoVenta, "CANCELADO", StringComparison.OrdinalIgnoreCase)).Sum(x => x.totalPendienteVenta);
            var anuladas = ventas.Count(x => string.Equals(x.estadoVenta, "CANCELADO", StringComparison.OrdinalIgnoreCase));
            var totalEgresos = 0m;
            var producido = totalIngresos - totalEgresos;

            lblValorBase.InnerText = FormatearMoneda(baseCaja.valorBase);
            lblTotalIngresos.InnerText = FormatearMoneda(totalIngresos);
            lblTotalEgresos.InnerText = FormatearMoneda(totalEgresos);
            lblProducido.InnerText = FormatearMoneda(producido);
            lblIdTurno.InnerText = baseCaja.id.ToString();
            lblNombreUsuario.InnerText = models.vendedor?.nombreVendedor ?? "-";
            lblFechaApertura.InnerText = baseCaja.fechaApertura.ToString("yyyy-MM-dd hh:mm tt");
            lblFechaCierre.InnerText = baseCaja.fechaCierre.HasValue ? baseCaja.fechaCierre.Value.ToString("yyyy-MM-dd hh:mm tt") : "Pendiente";
            lblEstadoBase.InnerText = baseCaja.estadoBase ?? "-";
            lblEstadoBaseBadge.Attributes["class"] = "cc-badge " + (string.Equals(baseCaja.estadoBase, "ACTIVA", StringComparison.OrdinalIgnoreCase) ? "ok" : "warn");
            lblTotalEfectivo.InnerText = FormatearMoneda(totalEfectivo);
            lblEfectivoMasBase.InnerText = FormatearMoneda(baseCaja.valorBase + totalEfectivo);
            lblVentasTarjeta.InnerText = FormatearMoneda(ventasTarjeta);
            lblVentasEfectivo.InnerText = FormatearMoneda(totalEfectivo);
            lblVentasTargeta2.InnerText = FormatearMoneda(ventasTarjeta);
            lblVentasCredito.InnerText = FormatearMoneda(ventasCredito);
            lblTotalIngresos2.InnerText = FormatearMoneda(totalIngresos);
            lblGastosEfectivo.InnerText = FormatearMoneda(totalEgresos);
            lblPagoCC_Efectivo.InnerText = FormatearMoneda(0);
            lblPagoCC_Targeta.InnerText = FormatearMoneda(0);
            lblPagoCP_Efectivo.InnerText = FormatearMoneda(0);
            lblTotalEgresos2.InnerText = FormatearMoneda(totalEgresos);
        }

        protected async void btn_Click(object sender, EventArgs e)
        {
            if (!CargarContexto())
            {
                AlertModerno.ErrorRedirect(this, "Error", "La sesion expiro o no contiene el contexto de trabajo.", "Default.aspx");
                return;
            }

            string accion = hidAccion.Value;
            string eventArgument = hidArgumento.Value;

            switch (accion)
            {
                case "ConfirmarCierre":
                    await ConfirmarCierre(eventArgument);
                    break;
            }
        }

        private async Task ConfirmarCierre(string eventArgument)
        {
            if (!string.Equals(baseCaja.estadoBase, "ACTIVA", StringComparison.OrdinalIgnoreCase))
            {
                AlertModerno.Warning(this, "Atencion", "La caja ya se encuentra cerrada.", true);
                return;
            }

            baseCaja.estadoBase = "CERRADA";
            baseCaja.fechaCierre = DateTime.Now;
            baseCaja.idUsuarioCierre = models.vendedor?.id;

            var resp = await BaseCajaControler.CRUD(db, baseCaja, 1);
            if (!resp)
            {
                AlertModerno.Error(this, "Error", "No fue posible cerrar la caja. Verifique e intente nuevamente.", true);
                return;
            }

            models.BaseCaja = baseCaja;
            SessionContextHelper.ApplyOperationalContext(Session, models);
            AlertModerno.SuccessGoTo(this, "OK", "Caja cerrada con exito.", "~/Salir.aspx", false, 1200);
        }

        private string FormatearMoneda(decimal valor)
        {
            return valor.ToString("C0");
        }
    }
}