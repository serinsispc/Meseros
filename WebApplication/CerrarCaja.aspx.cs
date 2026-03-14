using DAL;
using DAL.Controler;
using DAL.Model;
using System;
using Newtonsoft.Json;
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
        private V_TurnosCaja turnoCaja;
        private List<V_TablaVentas> ventas = new List<V_TablaVentas>();
        private List<InformePagoInternoTurnoItem> pagosInternosTurno = new List<InformePagoInternoTurnoItem>();

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
            turnoCaja = await dal.ConsultarUno<V_TurnosCaja>(db, x => x.id == baseCaja.id);
            pagosInternosTurno = await ConsultarPagosInternosTurno();

            if (turnoCaja != null)
            {
                PintarTurnoDesdeVista(turnoCaja);
                return;
            }

            ventas = await dal.ConsultarLista<V_TablaVentas>(db, x => x.idBaseCaja == baseCaja.id && x.eliminada == false);
            ventas = ventas.OrderByDescending(x => x.fechaVenta).ToList();

            var ventasValidas = ventas.Where(x => !EsVentaAnulada(x)).ToList();
            var totalIngresos = ventasValidas.Sum(x => x.total_A_Pagar);
            var totalEfectivo = ventasValidas.Sum(x => x.abonoEfectivo);
            var ventasTarjeta = ventasValidas.Sum(x => x.abonoTarjeta);
            var ventasCredito = ventasValidas.Sum(x => x.totalPendienteVenta);
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

        private void PintarTurnoDesdeVista(V_TurnosCaja turno)
        {
            var totalEfectivo = ObtenerTotalPagoInterno("EFECTIVO", turno.totalEfectivo);
            var ventasTarjeta = ObtenerTotalPagoInterno("TARJETA", turno.ventasTargeta);
            var otrosPagosInternos = ObtenerTotalPagosInternosVarios();
            var efectivoMasBase = turno.efectivoMasBase > 0 ? turno.efectivoMasBase : (baseCaja.valorBase + totalEfectivo);
            var totalIngresos = turno.totalIngresos > 0 ? turno.totalIngresos : (totalEfectivo + ventasTarjeta + otrosPagosInternos + turno.ventasCredito);
            var producido = turno.producido > 0 ? turno.producido : (totalIngresos - turno.totalEgresos);

            lblValorBase.InnerText = FormatearMoneda(turno.valorBase);
            lblTotalIngresos.InnerText = FormatearMoneda(totalIngresos);
            lblTotalEgresos.InnerText = FormatearMoneda(turno.totalEgresos);
            lblProducido.InnerText = FormatearMoneda(producido);
            lblIdTurno.InnerText = turno.id.ToString();
            lblNombreUsuario.InnerText = string.IsNullOrWhiteSpace(turno.nombreUsuario) ? (models.vendedor?.nombreVendedor ?? "-") : turno.nombreUsuario;
            lblFechaApertura.InnerText = turno.fechaApertura.ToString("yyyy-MM-dd hh:mm tt");
            lblFechaCierre.InnerText = turno.fechaCierre.HasValue ? turno.fechaCierre.Value.ToString("yyyy-MM-dd hh:mm tt") : "Pendiente";
            lblEstadoBase.InnerText = string.IsNullOrWhiteSpace(turno.estadoBase) ? (baseCaja.estadoBase ?? "-") : turno.estadoBase;
            lblEstadoBaseBadge.Attributes["class"] = "cc-badge " + (string.Equals(lblEstadoBase.InnerText, "ACTIVA", StringComparison.OrdinalIgnoreCase) ? "ok" : "warn");
            lblTotalEfectivo.InnerText = FormatearMoneda(totalEfectivo);
            lblEfectivoMasBase.InnerText = FormatearMoneda(efectivoMasBase);
            lblVentasTarjeta.InnerText = FormatearMoneda(ventasTarjeta);
            lblVentasEfectivo.InnerText = FormatearMoneda(totalEfectivo);
            lblVentasTargeta2.InnerText = FormatearMoneda(ventasTarjeta);
            lblVentasCredito.InnerText = FormatearMoneda(turno.ventasCredito);
            lblTotalIngresos2.InnerText = FormatearMoneda(totalIngresos);
            lblGastosEfectivo.InnerText = FormatearMoneda(turno.gastos_Efectivo);
            lblPagoCC_Efectivo.InnerText = FormatearMoneda(turno.pagoCC_Efectivo);
            lblPagoCC_Targeta.InnerText = FormatearMoneda(turno.pagoCC_Targeta);
            lblPagoCP_Efectivo.InnerText = FormatearMoneda(turno.pagoCP_Efectivo);
            lblTotalEgresos2.InnerText = FormatearMoneda(turno.totalEgresos);
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

        private async Task<List<InformePagoInternoTurnoItem>> ConsultarPagosInternosTurno()
        {
            try
            {
                using (var cn = new Conection_SQL(db))
                {
                    var sql = $"exec InformePagoInterno_Turno {baseCaja.id}";
                    var json = await cn.EjecutarConsulta(sql, true);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return new List<InformePagoInternoTurnoItem>();
                    }

                    return JsonConvert.DeserializeObject<List<InformePagoInternoTurnoItem>>(json) ?? new List<InformePagoInternoTurnoItem>();
                }
            }
            catch
            {
                return new List<InformePagoInternoTurnoItem>();
            }
        }

        private decimal ObtenerTotalPagoInterno(string nombre, decimal fallback)
        {
            var item = pagosInternosTurno.FirstOrDefault(x => string.Equals(x.nombreMPI, nombre, StringComparison.OrdinalIgnoreCase));
            return item != null ? item.total : fallback;
        }

        private decimal ObtenerTotalPagosInternosVarios()
        {
            if (pagosInternosTurno == null || pagosInternosTurno.Count == 0)
            {
                return 0m;
            }

            return pagosInternosTurno
                .Where(x =>
                    !string.Equals(x.nombreMPI, "EFECTIVO", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(x.nombreMPI, "TARJETA", StringComparison.OrdinalIgnoreCase))
                .Sum(x => x.total);
        }


        private void BindPagosInternosReporte()
        {
            var data = pagosInternosTurno ?? new List<InformePagoInternoTurnoItem>();
            rptPagosInternos.DataSource = data;
            rptPagosInternos.DataBind();

            if (pnlPagosInternos != null)
            {
                pnlPagosInternos.Visible = data.Count > 0;
            }

            if (lblTotalPagosInternos != null)
            {
                lblTotalPagosInternos.InnerText = FormatearMoneda(data.Sum(x => x.total));
            }
        }
        private bool EsVentaAnulada(V_TablaVentas venta)
        {
            return string.Equals(venta?.estadoVenta, "ANULADA", StringComparison.OrdinalIgnoreCase);
        }

        private string FormatearMoneda(decimal valor)
        {
            return valor.ToString("C0");
        }
    }
}






