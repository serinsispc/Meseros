using DAL;
using DAL.Controler;
using DAL.Model;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
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
        private List<GastoTurnoItem> gastosTurno = new List<GastoTurnoItem>();
        private List<InformeProductoVendidoTurnoItem> productosVendidosTurno = new List<InformeProductoVendidoTurnoItem>();
        protected DBConexion ajustes;
        protected bool MostrarCierreCajaHabilitado => ajustes != null && ajustes.MostrarCierreCaja;

        protected async void Page_Load(object sender, EventArgs e)
        {
            // cargarmos el DBConexion
            string dbJson = Session["DBConexion"]?.ToString();
            ajustes = JsonConvert.DeserializeObject<DBConexion>(dbJson);

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
            gastosTurno = await ConsultarGastosTurno();
            productosVendidosTurno = await ConsultarProductosVendidosTurno();
            hidGastosTurnoJson.Value = JsonConvert.SerializeObject(gastosTurno);
            hidProductosVendidosJson.Value = JsonConvert.SerializeObject(productosVendidosTurno);

            if (turnoCaja != null)
            {
                PintarTurnoDesdeVista(turnoCaja);
                BindPagosInternosReporte();
                return;
            }

            ventas = await dal.ConsultarLista<V_TablaVentas>(db, x => x.idBaseCaja == baseCaja.id && x.eliminada == false);
            ventas = ventas.OrderByDescending(x => x.fechaVenta).ToList();

            var ventasValidas = ventas.Where(x => !EsVentaAnulada(x)).ToList();
            var totalIngresos = ventasValidas.Sum(x => x.total_A_Pagar);
            var totalEfectivo = ventasValidas.Sum(x => x.abonoEfectivo);
            var ventasTarjeta = ObtenerTotalPagosNoEfectivo(ventasValidas.Sum(x => x.abonoTarjeta));
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
            BindPagosInternosReporte();
        }

        private void PintarTurnoDesdeVista(V_TurnosCaja turno)
        {
            var totalEfectivo = turno.totalEfectivo;
            var ventasEfectivo = turno.ventasEfectivo;
            var ventasTarjeta = turno.ventasTargeta;
            var efectivoMasBase = turno.efectivoMasBase;
            var totalIngresos = turno.totalIngresos;
            var producido = turno.producido;

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
            lblVentasEfectivo.InnerText = FormatearMoneda(ventasEfectivo);
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
                case "ActualizarBase":
                    await ActualizarValorBase(eventArgument);
                    break;
                case "AperturarCajon":
                    await AperturarCajonMonedero();
                    break;
            }
        }

        private async Task AperturarCajonMonedero()
        {
            var cajon = new AperturarCajon() { estado = true };
            PuntoDePagoPrinterHelper.Apply(cajon, Session, models);
            var respCajon = await AperturarCajonControler.CRUD(db, cajon, 0);

            if (!respCajon)
            {
                AlertModerno.Error(this, "Error", "No fue posible aperturar el cajón monedero.", true, 1800);
                return;
            }

            AlertModerno.Success(this, "OK", "Cajón abierto correctamente.", true, 1600);
        }

        private async Task ActualizarValorBase(string eventArgument)
        {
            if (!string.Equals(baseCaja.estadoBase, "ACTIVA", StringComparison.OrdinalIgnoreCase))
            {
                AlertModerno.Warning(this, "Atencion", "Solo puedes modificar la base mientras la caja este activa.", true);
                return;
            }

            var valorRaw = ObtenerArgumento(eventArgument, "VALOR_BASE")?.Trim() ?? string.Empty;
            valorRaw = valorRaw.Replace(".", string.Empty).Replace(",", string.Empty);

            if (!decimal.TryParse(valorRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal nuevoValorBase) || nuevoValorBase <= 0)
            {
                AlertModerno.Warning(this, "Atencion", "Ingrese un valor de base valido.", true);
                return;
            }

            baseCaja.valorBase = nuevoValorBase;
            var resp = await BaseCajaControler.CRUD(db, baseCaja, 1);
            if (!resp)
            {
                AlertModerno.Error(this, "Error", "No fue posible actualizar la base activa. Verifique e intente nuevamente.", true);
                return;
            }

            if (turnoCaja != null)
            {
                turnoCaja.valorBase = nuevoValorBase;
            }

            models.BaseCaja = baseCaja;
            SessionContextHelper.ApplyOperationalContext(Session, models);
            RefrescarIndicadoresBase();

            var script = @"(function(){
                var modalEl = document.getElementById('mdlEditarBase');
                if (modalEl && window.bootstrap && bootstrap.Modal) {
                    var modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                    modal.hide();
                }
                var txt = document.getElementById('txtValorBaseEditar');
                if (txt) { txt.value = ''; }
            })();";

            System.Web.UI.ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString("N"), script, true);
            AlertModerno.Success(this, "OK", "La base activa fue actualizada correctamente.", true, 1800);
        }

        private async Task ConfirmarCierre(string eventArgument)
        {
            if (!string.Equals(baseCaja.estadoBase, "ACTIVA", StringComparison.OrdinalIgnoreCase))
            {
                AlertModerno.Warning(this, "Atencion", "La caja ya se encuentra cerrada.", true);
                return;
            }

            var observacion = ObtenerArgumento(eventArgument, "OBS");

            baseCaja.estadoBase = "CERRADA";
            baseCaja.fechaCierre = DateTime.Now;
            baseCaja.idUsuarioCierre = models.vendedor?.id;

            var resp = await BaseCajaControler.CRUD(db, baseCaja, 1);
            if (!resp)
            {
                AlertModerno.Error(this, "Error", "No fue posible cerrar la caja. Verifique e intente nuevamente.", true);
                return;
            }

            lblEstadoBase.InnerText = baseCaja.estadoBase ?? "-";
            lblFechaCierre.InnerText = baseCaja.fechaCierre.HasValue ? baseCaja.fechaCierre.Value.ToString("yyyy-MM-dd hh:mm tt") : "Pendiente";
            lblEstadoBaseBadge.Attributes["class"] = "cc-badge warn";

            models.BaseCaja = baseCaja;
            SessionContextHelper.ApplyOperationalContext(Session, models);
            pagosInternosTurno = await ConsultarPagosInternosTurno();

            var cierreData = ConstruirDatosWhatsApp(observacion);
            var cierreMessage = "Caja cerrada con exito.";
            try
            {
                var whatsApp = new WhatsAppMetaHelper();
                var envioWhatsApp = await whatsApp.EnviarCierreCajaAsync(db, models, cierreData);
                if (!string.IsNullOrWhiteSpace(envioWhatsApp?.Message))
                {
                    cierreMessage += " " + envioWhatsApp.Message;
                }
            }
            catch (Exception ex)
            {
                cierreMessage += " WhatsApp pendiente: " + ex.Message;
            }

            try
            {
                var email = new EmailCierreCajaHelper();
                var envioCorreo = await email.EnviarCierreCajaAsync(db, ConstruirDatosCorreo(cierreData));
                if (!string.IsNullOrWhiteSpace(envioCorreo?.Message))
                {
                    cierreMessage += " " + envioCorreo.Message;
                }
            }
            catch (Exception ex)
            {
                cierreMessage += " Correo pendiente: " + ex.Message;
            }

            var script = @"if (window.SerinsisLoading && typeof window.SerinsisLoading.show === 'function') { window.SerinsisLoading.show(); }
            var txtObs = document.getElementById('txtObsCierre');
            if (txtObs) { txtObs.value = '" + EscapeJs(observacion) + @"'; }
            Swal.fire({
                icon: 'success',
                title: 'OK',
                text: '" + EscapeJs(cierreMessage) + @"',
                timer: 2600,
                timerProgressBar: true,
                showConfirmButton: false
            }).then(function(){
                if (" + (MostrarCierreCajaHabilitado ? "true" : "false") + @" && typeof window.ccImprimirTicketInline === 'function') {
                    window.ccImprimirTicketInline();
                }
                setTimeout(function(){ window.location.href = '" + ResolveUrl("~/Salir.aspx") + @"'; }, 2800);
            });";

            System.Web.UI.ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString("N"), script, true);
        }


        private string ObtenerArgumento(string raw, string key)
        {
            if (string.IsNullOrWhiteSpace(raw) || string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            var partes = raw.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var parte in partes)
            {
                var idx = parte.IndexOf('=');
                if (idx <= 0)
                {
                    continue;
                }

                var nombre = parte.Substring(0, idx);
                if (!string.Equals(nombre, key, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return Uri.UnescapeDataString(parte.Substring(idx + 1));
            }

            return string.Empty;
        }

        private string EscapeJs(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", string.Empty)
                .Replace("\n", "\\n");
        }

        private CierreCajaWhatsAppData ConstruirDatosWhatsApp(string observacion)
        {
            return new CierreCajaWhatsAppData
            {
                NombreCliente = models?.Sede?.nombreSede ?? "Cliente",
                NombreCajero = lblNombreUsuario?.InnerText ?? "-",
                FechaApertura = lblFechaApertura?.InnerText ?? "-",
                FechaCierre = lblFechaCierre?.InnerText ?? "-",
                ValorBase = ParseMoney(lblValorBase?.InnerText),
                VentasEfectivo = ParseMoney(lblVentasEfectivo?.InnerText),
                VentasTarjeta = ParseMoney(lblVentasTargeta2?.InnerText),
                VentasCredito = ParseMoney(lblVentasCredito?.InnerText),
                TotalEfectivo = ParseMoney(lblTotalEfectivo?.InnerText),
                EfectivoMasBase = ParseMoney(lblEfectivoMasBase?.InnerText),
                PagoCcEfectivo = ParseMoney(lblPagoCC_Efectivo?.InnerText),
                PagoCcTarjeta = ParseMoney(lblPagoCC_Targeta?.InnerText),
                PagoCpEfectivo = ParseMoney(lblPagoCP_Efectivo?.InnerText),
                GastosEfectivo = ParseMoney(lblGastosEfectivo?.InnerText),
                TotalIngresos = ParseMoney(lblTotalIngresos2?.InnerText),
                TotalEgresos = ParseMoney(lblTotalEgresos2?.InnerText),
                Producido = ParseMoney(lblProducido?.InnerText),
                EstadoBase = lblEstadoBase?.InnerText ?? "-",
                Observacion = observacion
            };
        }
        private CierreCajaEmailData ConstruirDatosCorreo(CierreCajaWhatsAppData cierreData)
        {
            return new CierreCajaEmailData
            {
                NombreCliente = cierreData.NombreCliente,
                IdTurno = baseCaja?.id ?? 0,
                NombreCajero = cierreData.NombreCajero,
                FechaApertura = cierreData.FechaApertura,
                FechaCierre = cierreData.FechaCierre,
                Observacion = cierreData.Observacion,
                EstadoBase = cierreData.EstadoBase,
                FechaGeneracion = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt"),
                ValorBase = cierreData.ValorBase,
                VentasEfectivo = cierreData.VentasEfectivo,
                VentasTarjeta = cierreData.VentasTarjeta,
                VentasCredito = cierreData.VentasCredito,
                TotalEfectivo = cierreData.TotalEfectivo,
                EfectivoMasBase = cierreData.EfectivoMasBase,
                PagoCcEfectivo = cierreData.PagoCcEfectivo,
                PagoCcTarjeta = cierreData.PagoCcTarjeta,
                PagoCpEfectivo = cierreData.PagoCpEfectivo,
                GastosEfectivo = cierreData.GastosEfectivo,
                TotalIngresos = cierreData.TotalIngresos,
                TotalEgresos = cierreData.TotalEgresos,
                Producido = cierreData.Producido,
                TotalPagosInternos = pagosInternosTurno?.Sum(x => x.total) ?? 0m,
                PagosInternos = pagosInternosTurno?.Select(x => new InformePagoInternoTurnoItem { nombreMPI = x.nombreMPI, total = x.total }).ToList() ?? new List<InformePagoInternoTurnoItem>()
            };
        }

        private decimal ParseMoney(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0m;
            }

            var cleaned = value
                .Replace("$", string.Empty)
                .Replace(".", string.Empty)
                .Replace(",", ".")
                .Trim();

            return decimal.TryParse(
                cleaned,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsed)
                ? parsed
                : 0m;
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

        private async Task<List<InformeProductoVendidoTurnoItem>> ConsultarProductosVendidosTurno()
        {
            if (ajustes == null || !ajustes.ImprimirProductosVendidos)
            {
                return new List<InformeProductoVendidoTurnoItem>();
            }

            try
            {
                return await InformeProductoVendidoTurnoControler.Lista(db, baseCaja.id);
            }
            catch
            {
                return new List<InformeProductoVendidoTurnoItem>();
            }
        }

        private async Task<List<GastoTurnoItem>> ConsultarGastosTurno()
        {
            try
            {
                using (var cn = new Conection_SQL(db))
                {
                    var sql = "select idGasto, fecha, concepto, valor, VidBolsillo as idBolsillo, VnombreBolsillo as nombreBolsillo, idTipoGasto, nombreTipoGasto, idBaseGasto from v_Gastos where idBaseGasto = " + baseCaja.id + " order by fecha desc, idGasto desc";
                    var json = await cn.EjecutarConsulta(sql, true);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return new List<GastoTurnoItem>();
                    }

                    return JsonConvert.DeserializeObject<List<GastoTurnoItem>>(json) ?? new List<GastoTurnoItem>();
                }
            }
            catch
            {
                return new List<GastoTurnoItem>();
            }
        }

        private decimal ObtenerTotalPagoInterno(string nombre, decimal fallback)
        {
            var item = pagosInternosTurno.FirstOrDefault(x => string.Equals(x.nombreMPI, nombre, StringComparison.OrdinalIgnoreCase));
            return item != null ? item.total : fallback;
        }

        private decimal ObtenerTotalPagosNoEfectivo(decimal fallback)
        {
            if (pagosInternosTurno == null || pagosInternosTurno.Count == 0)
            {
                return fallback;
            }

            return pagosInternosTurno
                .Where(x => !string.Equals(x.nombreMPI, "EFECTIVO", StringComparison.OrdinalIgnoreCase))
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

        private void RefrescarIndicadoresBase()
        {
            var totalEfectivo = ParseMoney(lblTotalEfectivo?.InnerText);
            lblValorBase.InnerText = FormatearMoneda(baseCaja.valorBase);
            lblEfectivoMasBase.InnerText = FormatearMoneda(baseCaja.valorBase + totalEfectivo);
        }

        private class GastoTurnoItem
        {
            public int idGasto { get; set; }
            public DateTime fecha { get; set; }
            public string concepto { get; set; }
            public decimal valor { get; set; }
            public int idBolsillo { get; set; }
            public string nombreBolsillo { get; set; }
            public int idTipoGasto { get; set; }
            public string nombreTipoGasto { get; set; }
            public int idBaseGasto { get; set; }
        }


    }
}













