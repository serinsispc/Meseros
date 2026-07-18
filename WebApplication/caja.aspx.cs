using DAL;
using DAL.Controler;
using DAL.Funciones;
using DAL.Helpers;
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using WebApplication.Class;
using WebApplication.Helpers;
using WebApplication.ViewModels;

namespace WebApplication
{
    public partial class caja : System.Web.UI.Page
    {
        protected bool AutoFocusBusquedaDesktop()
        {
            return true;
        }

        protected int DesktopMinWidth()
        {
            return 992;
        }

        private const string ok = "Ok";
        private const string SessionVistaCaja = "CajaVistaActual";
        private const string VistaCaja = "caja";
        private const string VistaVentas = "ventas";
        private const string SessionModelsJson = "ModelsJson";
        private const string SessionPuedeEditarDetalleVenta = "Caja_PuedeEditarDetalleVenta";
        private const string SessionPuedeEliminarDetalleVenta = "Caja_PuedeEliminarDetalleVenta";
        private const string SessionCodigoAutorizacion = "Caja_CodigoAutorizacion";
        private const string SessionTipoAutorizacion = "Caja_TipoAutorizacion";
        private const string SessionDetalleAutorizacion = "Caja_DetalleAutorizacion";
        private const string SessionVenceAutorizacion = "Caja_VenceAutorizacion";
        private const string SessionAutorizacionRemotaAprobada = "Caja_AutorizacionRemotaAprobada";
        private const string CodigoAutorizacionRemota = "__APROBADO_REMOTO__";
        private const string SessionCajaZonasKey = "Caja_Zonas";
        private const string SessionCajaCategoriasKey = "Caja_Categorias";
        private const string SessionCajaMesasKey = "Caja_Mesas";
        private const string SessionCajaProductosKey = "Caja_Productos";
        private const string SessionCajaMetodosPagoKey = "Caja_MetodosPago";
        private const string SessionCajaMediosPagoInternosKey = "Caja_MediosPagoInternos";
        private const string SessionCajaRelMediosPagoInternosKey = "Caja_RelMediosPagoInternos";
        private const string SessionCajaAdicionesKey = "Caja_Adiciones";
        private const string SessionCajaClienteDomiciliosKey = "Caja_ClienteDomicilios";
        private const string PermisoEditarDetalleVenta = "EDITAR DETALLE VENTA";
        private const string PermisoEliminarDetalleVenta = "ELIMINAR DETALLE VENTA";
        protected MenuViewModels models = new MenuViewModels();
        protected List<V_TablaVentas> VentasCaja = new List<V_TablaVentas>();
        protected decimal VentasCajaTotal;
        protected int VentasCajaCantidad;
        protected decimal VentasCajaPendiente;
        protected int VentasCajaAnuladas;
        protected DBConexion ajustes = new DBConexion();
        private readonly Dictionary<int, List<V_Precios>> _preciosDetallePorPresentacion = new Dictionary<int, List<V_Precios>>();
        private bool _puedeEditarDetalleVentaCajero;
        private bool _puedeEliminarDetalleVentaCajero;

        protected bool EnVistaVentas()
        {
            return string.Equals(Convert.ToString(Session[SessionVistaCaja] ?? VistaCaja), VistaVentas, StringComparison.OrdinalIgnoreCase);
        }

        protected string TextoBotonVentas()
        {
            return EnVistaVentas() ? "Caja" : "Ventas";
        }

        protected string AccionBotonVentas()
        {
            return EnVistaVentas() ? "VerCaja" : "Ventas";
        }

        protected string MonedaVistaVentas(decimal valor)
        {
            return valor.ToString("C0");
        }

        protected string FacturaLabelVista(V_TablaVentas venta)
        {
            if (!string.IsNullOrWhiteSpace(venta?.prefijo) && venta.numeroVenta > 0)
            {
                return venta.prefijo + "-" + venta.numeroVenta.ToString("000000");
            }
            if (venta?.numeroVenta > 0)
            {
                return venta.numeroVenta.ToString();
            }
            return "Sin numerar";
        }

        protected bool EsVentaAnulada(V_TablaVentas venta)
        {
            return string.Equals(venta?.estadoVenta, "CANCELADO", StringComparison.OrdinalIgnoreCase)
                || string.Equals(venta?.estadoVenta, "ANULADA", StringComparison.OrdinalIgnoreCase);
        }

        protected string EstadoVentaVista(V_TablaVentas venta)
        {
            if (EsVentaAnulada(venta)) return "Anulada";
            if ((venta?.totalPendienteVenta ?? 0) > 0) return "Pendiente";
            return "Pagada";
        }

        protected string NombreVendedorCorto(object nombreObj)
        {
            var nombreCompleto = Convert.ToString(nombreObj ?? string.Empty);
            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                return string.Empty;
            }

            var partes = nombreCompleto
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            if (partes.Length == 0)
            {
                return string.Empty;
            }

            if (partes.Length == 1)
            {
                return partes[0];
            }

            var dosPalabras = partes[0] + " " + partes[1];
            if (dosPalabras.Length <= 18)
            {
                return dosPalabras;
            }

            return partes[0] + " " + partes[1].Substring(0, 1).ToUpper() + ".";
        }

        protected string NombreVendedorMesa(object nombreMesaObj)
        {
            var nombreMesa = Convert.ToString(nombreMesaObj ?? string.Empty);
            if (string.IsNullOrWhiteSpace(nombreMesa) || models?.cuentasMesasVista == null || !models.cuentasMesasVista.Any())
            {
                return string.Empty;
            }

            var cuentaMesa = models.cuentasMesasVista
                .FirstOrDefault(x => string.Equals(
                    Convert.ToString(x.nombremesa ?? string.Empty).Trim(),
                    nombreMesa.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (cuentaMesa == null)
            {
                return string.Empty;
            }

            return NombreVendedorCorto(cuentaMesa.nombrevendedor);
        }

        protected bool EsCuentaDomicilio(object nombreClienteDomicilioObj)
        {
            var nombreClienteDomicilio = Convert.ToString(nombreClienteDomicilioObj ?? string.Empty).Trim();
            return !string.IsNullOrWhiteSpace(nombreClienteDomicilio)
                && !string.Equals(nombreClienteDomicilio, "-", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(nombreClienteDomicilio, "N/A", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(nombreClienteDomicilio, "NULL", StringComparison.OrdinalIgnoreCase);
        }

        private string CapitalizarNombre(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return string.Empty;
            }

            valor = valor.Trim().ToLowerInvariant();
            return char.ToUpperInvariant(valor[0]) + valor.Substring(1);
        }

        protected string ResumirNombreClienteDomicilio(object nombreClienteDomicilioObj)
        {
            var nombreClienteDomicilio = Convert.ToString(nombreClienteDomicilioObj ?? string.Empty).Trim();
            if (!EsCuentaDomicilio(nombreClienteDomicilio))
            {
                return string.Empty;
            }

            var partes = nombreClienteDomicilio
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            if (partes.Length == 0)
            {
                return string.Empty;
            }

            if (partes.Length == 1)
            {
                return CapitalizarNombre(partes[0]);
            }

            var nombrePrincipal = CapitalizarNombre(partes[0]);
            var iniciales = string.Join(".", partes.Skip(1).Select(x => char.ToUpperInvariant(x.Trim()[0]).ToString()));
            return nombrePrincipal + " " + iniciales;
        }

        protected string TextoSecundarioCuenta(object nombreMesaObj, object nombreClienteDomicilioObj)
        {
            var nombreClienteDomicilio = Convert.ToString(nombreClienteDomicilioObj ?? string.Empty).Trim();
            if (EsCuentaDomicilio(nombreClienteDomicilio))
            {
                return ResumirNombreClienteDomicilio(nombreClienteDomicilio);
            }

            return Convert.ToString(nombreMesaObj ?? string.Empty).Trim();
        }

        protected bool CuentaActivaEsDomicilio()
        {
            if (models?.cuentas == null || !models.cuentas.Any() || models.IdCuentaActiva <= 0)
            {
                return false;
            }

            var cuenta = models.cuentas.FirstOrDefault(x => x.id == models.IdCuentaActiva);
            return cuenta != null && EsCuentaDomicilio(cuenta.nombreCD);
        }

        protected string NombreClienteDomicilioActivo()
        {
            if (models?.clienteDomicilioActivo != null && !string.IsNullOrWhiteSpace(models.clienteDomicilioActivo.nombreCliente))
            {
                return ResumirNombreClienteDomicilio(models.clienteDomicilioActivo.nombreCliente);
            }

            if (models?.cuentas == null || !models.cuentas.Any() || models.IdCuentaActiva <= 0)
            {
                return string.Empty;
            }

            var cuenta = models.cuentas.FirstOrDefault(x => x.id == models.IdCuentaActiva);
            return cuenta == null ? string.Empty : ResumirNombreClienteDomicilio(cuenta.nombreCD);
        }

        protected string EstadoDomicilioActualCodigo()
        {
            if (!CuentaActivaEsDomicilio())
            {
                return string.Empty;
            }

            var estado = DomicilioEstadoVentaHelper.ObtenerEstado(models?.venta?.observacionVenta);
            return string.IsNullOrWhiteSpace(estado) ? DomicilioEstadoVentaHelper.EstadoRecibido : estado;
        }

        protected string EstadoDomicilioActualTexto()
        {
            return DomicilioEstadoVentaHelper.EtiquetaEstado(EstadoDomicilioActualCodigo());
        }

        protected string ObservacionVentaVisibleActual()
        {
            return DomicilioEstadoVentaHelper.LimpiarObservacionVisible(models?.venta?.observacionVenta);
        }

        protected string ClaseEstadoDomicilioActual()
        {
            switch (EstadoDomicilioActualCodigo())
            {
                case "RECIBIDO":
                    return "estado-recibido";
                case "EN_PREPARACION":
                    return "estado-preparacion";
                case "LISTO_PARA_DESPACHO":
                    return "estado-listo";
                case "EN_CAMINO":
                    return "estado-camino";
                case "ENTREGADO":
                    return "estado-entregado";
                case "NOVEDAD":
                    return "estado-novedad";
                default:
                    return "estado-recibido";
            }
        }

        protected bool EstadoDomicilioEs(string estado)
        {
            return string.Equals(EstadoDomicilioActualCodigo(), DomicilioEstadoVentaHelper.NormalizarEstado(estado), StringComparison.OrdinalIgnoreCase);
        }

        protected int MetodoPagoDomicilioActualId()
        {
            return DomicilioEstadoVentaHelper.ObtenerMetodoPagoId(models?.venta?.observacionVenta);
        }

        protected int MetodoPagoDomicilioBaseActualId()
        {
            var idMetodo = MetodoPagoDomicilioActualId();
            if (idMetodo > 0)
            {
                return idMetodo;
            }

            if (idMetodo < 0)
            {
                var relacion = models?.relMediosPagoInternos?.FirstOrDefault(x => x.idMediosDePagoInternos == Math.Abs(idMetodo));
                return relacion?.idMedioDePago ?? 0;
            }

            return 0;
        }

        protected string MetodoPagoDomicilioActualNombre()
        {
            var idMetodo = MetodoPagoDomicilioActualId();
            if (idMetodo == 0)
            {
                return "Sin definir";
            }

            if (idMetodo < 0)
            {
                var relacion = models?.relMediosPagoInternos?.FirstOrDefault(x => x.idMediosDePagoInternos == Math.Abs(idMetodo));
                var nombreBase = relacion?.idMedioDePago > 0
                    ? models?.metodosPago?.FirstOrDefault(x => x.id == relacion.idMedioDePago)?.name ?? string.Empty
                    : string.Empty;
                var medioInterno = models?.mediosPagoInternos?.FirstOrDefault(x => x.id == Math.Abs(idMetodo));
                var nombreInterno = string.IsNullOrWhiteSpace(medioInterno?.nombreMPI) ? string.Empty : medioInterno.nombreMPI.Trim();
                if (string.IsNullOrWhiteSpace(nombreInterno))
                {
                    return "Sin definir";
                }

                return string.IsNullOrWhiteSpace(nombreBase)
                    ? nombreInterno
                    : nombreBase + " / " + nombreInterno;
            }

            if (models?.metodosPago == null || !models.metodosPago.Any())
            {
                return "Sin definir";
            }

            return models.metodosPago.FirstOrDefault(x => x.id == idMetodo)?.name ?? "Sin definir";
        }

        protected decimal MontoBilleteDomicilioActual()
        {
            return DomicilioEstadoVentaHelper.ObtenerMontoBillete(models?.venta?.observacionVenta);
        }

        protected bool MetodoPagoDomicilioEsEfectivo()
        {
            var idMetodoBase = MetodoPagoDomicilioBaseActualId();
            var nombreBase = models?.metodosPago?.FirstOrDefault(x => x.id == idMetodoBase)?.name ?? string.Empty;
            return idMetodoBase == 10 || nombreBase.IndexOf("efect", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        protected string CambioSugeridoDomicilio()
        {
            var billete = MontoBilleteDomicilioActual();
            var total = models?.venta?.total_A_Pagar ?? 0m;
            if (billete <= 0 || total <= 0 || billete < total)
            {
                return "$ 0";
            }

            return FormatearMoneda(billete - total);
        }

        protected string RelMediosPagoInternosJson()
        {
            var lista = models?.relMediosPagoInternos ?? new List<V_R_MediosDePago_MediosDePagoInternos>();
            return JsonConvert.SerializeObject(lista).Replace("</", "<\\/");
        }

        private V_CuentasVenta CuentaActivaPorMesa(int idMesa)
        {
            var mesa = models?.MesasLista?.FirstOrDefault(x => x.id == idMesa);
            if (mesa == null || models?.cuentasMesasVista == null || !models.cuentasMesasVista.Any())
            {
                return null;
            }

            return models.cuentasMesasVista.FirstOrDefault(x =>
                string.Equals(
                    Convert.ToString(x.nombremesa ?? string.Empty).Trim(),
                    Convert.ToString(mesa.nombreMesa ?? string.Empty).Trim(),
                    StringComparison.OrdinalIgnoreCase));
        }

        private bool PuedeGestionarMesa(int idMesa, out V_CuentasVenta cuentaMesa)
        {
            cuentaMesa = CuentaActivaPorMesa(idMesa);

            if (models?.vendedor?.cajaMovil == 1)
            {
                return true;
            }

            if (ajustes?.meserosCompartidos == true)
            {
                return true;
            }

            if (cuentaMesa == null)
            {
                return true;
            }

            return cuentaMesa.idvendedor == models.vendedor.id;
        }

        private void EstablecerVistaCaja(string vista)
        {
            Session[SessionVistaCaja] = string.IsNullOrWhiteSpace(vista) ? VistaCaja : vista;
        }

        private async Task CargarVistaVentasCaja()
        {
            VentasCaja = new List<V_TablaVentas>();
            VentasCajaTotal = 0;
            VentasCajaCantidad = 0;
            VentasCajaPendiente = 0;
            VentasCajaAnuladas = 0;

            var db = models?.db ?? Convert.ToString(Session[SessionContextHelper.DbKey]);
            var idBase = SessionContextHelper.ResolveBaseCajaId(Session, models);
            if (string.IsNullOrWhiteSpace(db) || idBase <= 0)
            {
                return;
            }

            var dal = new SqlAutoDAL();
            VentasCaja = await dal.ConsultarLista<V_TablaVentas>(db, x => x.idBaseCaja == idBase && x.eliminada == false) ?? new List<V_TablaVentas>();
            VentasCaja = VentasCaja.OrderByDescending(x => x.fechaVenta).ToList();

            VentasCajaTotal = VentasCaja.Where(x => !EsVentaAnulada(x)).Sum(x => x.total_A_Pagar);
            VentasCajaCantidad = VentasCaja.Count(x => x.numeroVenta > 0);
            VentasCajaPendiente = VentasCaja.Where(x => !EsVentaAnulada(x)).Sum(x => x.totalPendienteVenta);
            VentasCajaAnuladas = VentasCaja.Count(EsVentaAnulada);
        }

        protected bool PuedeEliminarServicioActivo()
        {
            return models?.IdCuentaActiva > 0 && (models.detalleCaja == null || !models.detalleCaja.Any());
        }

        protected bool PuedeEliminarDetalleCaja()
        {
            return ajustes?.EliminarDetalleCaja == true;
        }

        protected bool PuedeEditarDetalleCaja()
        {
            return ajustes?.DecuentoVendedorJSON == true;
        }

        private bool CodigoSupervisorValido(string codigo, string tipo, int idDetalle)
        {
            var esperado = (ajustes?.ClaveSupervisorCaja ?? string.Empty).Trim();
            var ingresado = (codigo ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(esperado)
                && !string.Equals(esperado, "-", StringComparison.OrdinalIgnoreCase)
                && string.Equals(esperado, ingresado, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var codigoTemporal = Convert.ToString(Session[SessionCodigoAutorizacion] ?? string.Empty);
            var tipoTemporal = Convert.ToString(Session[SessionTipoAutorizacion] ?? string.Empty);
            var detalleTemporal = Convert.ToInt32(Session[SessionDetalleAutorizacion] ?? 0);
            var vence = Session[SessionVenceAutorizacion] as DateTime?;
            var contextoValido = vence.HasValue && vence.Value >= DateTime.UtcNow
                && detalleTemporal == idDetalle
                && string.Equals(tipoTemporal, tipo, StringComparison.OrdinalIgnoreCase);
            var aprobadoRemotamente = string.Equals(ingresado, CodigoAutorizacionRemota, StringComparison.Ordinal)
                && Convert.ToBoolean(Session[SessionAutorizacionRemotaAprobada] ?? false);
            var valido = contextoValido && (aprobadoRemotamente
                || string.Equals(codigoTemporal, ingresado, StringComparison.OrdinalIgnoreCase));

            if (valido)
            {
                LimpiarAutorizacionTemporal();
            }
            return valido;
        }

        private void LimpiarAutorizacionTemporal()
        {
            Session.Remove(SessionCodigoAutorizacion);
            Session.Remove(SessionTipoAutorizacion);
            Session.Remove(SessionDetalleAutorizacion);
            Session.Remove(SessionVenceAutorizacion);
            Session.Remove(SessionAutorizacionRemotaAprobada);
        }

        [WebMethod(EnableSession = true)]
        public static bool ConsultarAutorizacionSupervisor()
        {
            var context = HttpContext.Current;
            var session = context?.Session;
            if (session == null)
            {
                return false;
            }

            var codigo = Convert.ToString(session[SessionCodigoAutorizacion] ?? string.Empty).Trim();
            var vence = session[SessionVenceAutorizacion] as DateTime?;
            if (string.IsNullOrWhiteSpace(codigo) || !vence.HasValue || vence.Value < DateTime.UtcNow)
            {
                return false;
            }

            var desde = vence.Value.AddMinutes(-10).ToLocalTime();
            const string sql = @"
select top (1) 1
from dbo.AprobarNotificacionMovil
where codigo = @codigo
  and fecha >= @desde
order by fecha desc;";

            try
            {
                using (var cn = new SqlConnection(RuntimeSettings.BuildSqlConnectionString("DBNotificacionesMovil")))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@codigo", SqlDbType.VarChar, 100).Value = codigo;
                    cmd.Parameters.Add("@desde", SqlDbType.DateTime).Value = desde;
                    cn.Open();
                    var aprobado = cmd.ExecuteScalar() != null;
                    if (aprobado)
                    {
                        session[SessionAutorizacionRemotaAprobada] = true;
                    }
                    return aprobado;
                }
            }
            catch
            {
                return false;
            }
        }

        [WebMethod(EnableSession = true)]
        public static ListaPreciosDetalleResponse ConsultarListaPreciosDetalle(int idPresentacion)
        {
            var response = new ListaPreciosDetalleResponse();
            var session = HttpContext.Current?.Session;
            if (session == null || idPresentacion <= 0)
            {
                return response;
            }

            var ajustesJson = Convert.ToString(session["DBConexion"] ?? string.Empty);
            var configuracion = string.IsNullOrWhiteSpace(ajustesJson)
                ? null
                : JsonConvert.DeserializeObject<DBConexion>(ajustesJson);
            response.Habilitada = configuracion != null && configuracion.PrecioLT == 1m;
            if (!response.Habilitada)
            {
                return response;
            }

            var model = SessionContextHelper.LoadModels(session);
            var db = model?.db ?? Convert.ToString(session[SessionContextHelper.DbKey] ?? string.Empty);
            if (string.IsNullOrWhiteSpace(db))
            {
                throw new InvalidOperationException("No existe una base de datos activa para consultar la lista de precios.");
            }

            const string sql = @"
select id, idPresentacion, isnull(nombrePrecio, '') nombrePrecio, isnull(valorPrecio, 0) valorPrecio
from dbo.V_Precios
where idPresentacion = @idPresentacion
order by nombrePrecio, id;";

            using (var cn = new SqlConnection(RuntimeSettings.BuildSqlConnectionString(db)))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@idPresentacion", SqlDbType.Int).Value = idPresentacion;
                cn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        response.Precios.Add(new PrecioDetalleResponse
                        {
                            id = Convert.ToInt32(reader["id"]),
                            nombrePrecio = Convert.ToString(reader["nombrePrecio"] ?? string.Empty),
                            valorPrecio = Convert.ToDecimal(reader["valorPrecio"])
                        });
                    }
                }
            }

            return response;
        }

        public sealed class ListaPreciosDetalleResponse
        {
            public bool Habilitada { get; set; }
            public List<PrecioDetalleResponse> Precios { get; set; } = new List<PrecioDetalleResponse>();
        }

        public sealed class PrecioDetalleResponse
        {
            public int id { get; set; }
            public string nombrePrecio { get; set; }
            public decimal valorPrecio { get; set; }
        }

        protected IEnumerable<V_CuentaCliente> CuentasClienteActivas()
        {
            return (models?.v_CuentaClientes ?? new List<V_CuentaCliente>())
                .Where(x => x.idVenta == models.IdCuentaActiva && !x.eliminada)
                .OrderBy(x => x.fecha);
        }

        protected bool TieneDetalleServicioActivo()
        {
            return models?.detalleCaja != null && models.detalleCaja.Any();
        }

        protected decimal ResumenSubtotal()
        {
            return models.IdCuenteClienteActiva > 0 ? models.ventaCuenta?.subtotalVenta ?? 0 : models.venta?.subtotalVenta ?? 0;
        }

        protected decimal ResumenImpuestos()
        {
            return models.IdCuenteClienteActiva > 0 ? models.ventaCuenta?.ivaVenta ?? 0 : models.venta?.ivaVenta ?? 0;
        }

        protected decimal ResumenTotal1()
        {
            return models.IdCuenteClienteActiva > 0 ? models.ventaCuenta?.totalVenta ?? 0 : models.venta?.totalVenta ?? 0;
        }

        protected decimal ResumenPropina()
        {
            return models.IdCuenteClienteActiva > 0 ? models.ventaCuenta?.propina ?? 0 : models.venta?.propina ?? 0;
        }

        protected decimal ResumenPorcentajePropina()
        {
            var valor = models.IdCuenteClienteActiva > 0 ? models.ventaCuenta?.por_propina ?? 0 : models.venta?.por_propina ?? 0;
            return valor * 100m;
        }

        protected decimal ResumenTotal2()
        {
            return models.IdCuenteClienteActiva > 0 ? models.ventaCuenta?.total_A_Pagar ?? 0 : models.venta?.total_A_Pagar ?? 0;
        }

        protected string NombreCuentaClienteActiva()
        {
            if (models.IdCuenteClienteActiva <= 0)
            {
                return "Cuenta General";
            }

            return models.ventaCuenta?.nombreCuenta ?? $"Cuenta #{models.IdCuenteClienteActiva}";
        }

        protected string FormatearMoneda(object valor)
        {
            decimal numero;
            return decimal.TryParse(Convert.ToString(valor), out numero) ? numero.ToString("C0") : "$ 0";
        }

        protected bool EsCortesiaDetalle(object valor)
        {
            decimal numero;
            return decimal.TryParse(Convert.ToString(valor), out numero) && numero <= 0;
        }

        protected string ClienteDomiciliosJson()
        {
            var lista = models?.clienteDomicilios ?? new List<ClienteDomicilio>();
            return JsonConvert.SerializeObject(lista).Replace("</", "<\\/");
        }

        protected string DomicilioActivoPrintJson()
        {
            if (!CuentaActivaEsDomicilio())
            {
                return "null";
            }

            var cliente = models?.clienteDomicilioActivo ?? new ClienteDomicilio();
            var sede = models?.Sede ?? new Sede();
            var printerWidth = PuntoDePagoPrinterHelper.ResolvePrinterWidth(Session, models);
            var telefonoSede = !string.IsNullOrWhiteSpace(sede.telefono) ? sede.telefono : (sede.celular ?? string.Empty);
            var detalle = (models?.detalleCaja ?? new List<V_DetalleCaja>())
                .Select(x => new
                {
                    producto = x.nombreProducto,
                    cantidad = x.unidad.ToString("0"),
                    valor = FormatearMoneda(x.totalDetalle),
                    nota = string.IsNullOrWhiteSpace(x.adiciones) ? string.Empty : x.adiciones
                })
                .ToList();

            var payload = new
            {
                restaurante = sede.nombreSede ?? string.Empty,
                nitRestaurante = sede.nit ?? string.Empty,
                regimenRestaurante = sede.regimen ?? string.Empty,
                direccionRestaurante = sede.direccion ?? string.Empty,
                telefonoRestaurante = telefonoSede,
                horarioRestaurante = sede.horarios_atencion ?? string.Empty,
                leyendaRestaurante1 = sede.leyenda1 ?? string.Empty,
                leyendaRestaurante2 = sede.leyenda2 ?? string.Empty,
                puntoPago = NombrePuntoDePagoActual(),
                printerWidth = printerWidth <= 58 ? 58 : 80,
                cuenta = models.IdCuentaActiva,
                estado = EstadoDomicilioActualTexto(),
                cliente = cliente?.nombreCliente ?? string.Empty,
                telefono = cliente?.celularCliente ?? string.Empty,
                direccion = cliente?.direccionCliente ?? string.Empty,
                medioPago = MetodoPagoDomicilioActualNombre(),
                pagaCon = MetodoPagoDomicilioEsEfectivo() && MontoBilleteDomicilioActual() > 0 ? FormatearMoneda(MontoBilleteDomicilioActual()) : string.Empty,
                vueltas = MetodoPagoDomicilioEsEfectivo() && MontoBilleteDomicilioActual() > 0 ? CambioSugeridoDomicilio() : string.Empty,
                total = FormatearMoneda(models?.venta?.total_A_Pagar ?? 0m),
                observacion = ObservacionVentaVisibleActual(),
                items = detalle
            };

            return JsonConvert.SerializeObject(payload).Replace("</", "<\\/");
        }
        protected string AdicionesCatalogoJson()
        {
            var lista = models?.adiciones ?? new List<V_CatagoriaAdicion>();
            return JsonConvert.SerializeObject(lista).Replace("</", "<\\/");
        }

        protected string NombrePuntoDePagoActual()
        {
            if (!string.IsNullOrWhiteSpace(models?.PuntoDePagoSeleccionado?.nombrePunto))
            {
                return models.PuntoDePagoSeleccionado.nombrePunto;
            }

            return "Sin punto seleccionado";
        }

        protected bool MostrarAperturarCajon()
        {
            return ajustes != null
                && ajustes.MostrarCierreCaja
                && models?.vendedor?.cajaMovil == 1;
        }

        protected bool MostrarBotonesComandas()
        {
            return ajustes?.ComandasCaja == true;
        }

        protected bool MostrarResumenPropina()
        {
            return ajustes != null && ajustes.Propina > 0;
        }

        protected bool MostrarListadoPreciosDetalle()
        {
            return ajustes != null && ajustes.PrecioLT == 1m;
        }

        protected bool EsProductoGramera(object idPresentacionObj)
        {
            if (!int.TryParse(Convert.ToString(idPresentacionObj), out var idPresentacion) || idPresentacion <= 0)
            {
                return false;
            }

            var producto = (models?.productosLista ?? models?.productos ?? new List<v_productoVenta>())
                .FirstOrDefault(x => x != null && x.idPresentacion == idPresentacion);

            return producto != null && producto.gramera == 1;
        }

        protected string ObtenerGrameraDetalleData(object idPresentacionObj)
        {
            return EsProductoGramera(idPresentacionObj) ? "1" : "0";
        }

        protected string FormatearCantidadDetalleInput(object unidadObj, object idPresentacionObj)
        {
            var cantidad = Convert.ToDecimal(unidadObj ?? 0m);
            var formato = EsProductoGramera(idPresentacionObj) ? "0.###" : "0";
            return cantidad.ToString(formato, System.Globalization.CultureInfo.InvariantCulture);
        }

        protected string ObtenerPreciosDetalleData(object idPresentacionObj)
        {
            if (!MostrarListadoPreciosDetalle())
            {
                return "[]";
            }

            if (!int.TryParse(Convert.ToString(idPresentacionObj), out var idPresentacion) || idPresentacion <= 0)
            {
                return "[]";
            }

            if (!_preciosDetallePorPresentacion.TryGetValue(idPresentacion, out var lista) || lista == null || lista.Count == 0)
            {
                return "[]";
            }

            var payload = lista.Select(x => new
            {
                id = x.id,
                nombrePrecio = x.nombrePrecio ?? string.Empty,
                valorPrecio = x.valorPrecio
            }).ToList();

            return HttpUtility.HtmlAttributeEncode(JsonConvert.SerializeObject(payload).Replace("</", "<\\/"));
        }

        protected async void Page_Load(object sender, EventArgs e)
        {
            await CargarAjustesDbEnContextoAsync();

            if (!IsPostBack)
            {
                if (!await DeserializarModels())
                {
                    AlertModerno.ErrorRedirect(this, "Error", "La sesion expiro o no contiene el contexto de trabajo.", "Default.aspx");
                    return;
                }
                //antes de iniciar verificamos que halla session activa 
                var resp = await VerificarSession();
                if (!resp)
                {
                    //retornamos para la pagina de login
                    AlertModerno.ErrorRedirect(this, "Error", "Aun no hay session activa.", "Default.aspx");
                    return;
                }

                await IniciarPagina();
            }
        }

        private async Task CargarAjustesDbEnContextoAsync()
        {
            var dbJson = Session["DBConexion"] as string;
            if (!string.IsNullOrWhiteSpace(dbJson))
            {
                try
                {
                    ajustes = JsonConvert.DeserializeObject<DBConexion>(dbJson) ?? new DBConexion();
                    return;
                }
                catch
                {
                    ajustes = new DBConexion();
                }
            }

            var modelEnSesion = SessionContextHelper.LoadModels(Session);
            var db = modelEnSesion?.db ?? Convert.ToString(Session[SessionContextHelper.DbKey]);
            if (string.IsNullOrWhiteSpace(db))
            {
                return;
            }

            var ajustesDb = await DBConexionControler.DAsync(db);
            if (ajustesDb == null)
            {
                return;
            }

            ajustes = ajustesDb;
            Session["DBConexion"] = JsonConvert.SerializeObject(ajustesDb);
        }

        private async Task<bool> DeserializarModels()
        {
            var model = SessionContextHelper.LoadModels(Session);
            if (model == null)
            {
                return false;
            }

            models = model;
            await CargarPermisosDetalleCajeroAsync();
            return true;
        }

        private async Task<bool> RecargarAjustesDb()
        {
            var ajustesDb = await DBConexionControler.DAsync(models.db);
            if (ajustesDb == null)
            {
                return false;
            }

            ajustes = ajustesDb;
            Session["DBConexion"] = JsonConvert.SerializeObject(ajustesDb);
            return true;
        }

        private async Task CargarDATA()
        {
            await CargarPermisosDetalleCajeroAsync();
            await CargarListadoPreciosDetalleAsync();
            Cargar_RP();
            SessionContextHelper.SaveModelsReferenceOnly(Session, models);

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "CajaPostRenderConfig",
                $@"
        window.CajaConfig = window.CajaConfig || {{}};
        window.CajaConfig.autoFocusBusquedaDesktop = {(AutoFocusBusquedaDesktop() ? "true" : "false")};
        window.CajaConfig.desktopMinWidth = {DesktopMinWidth()};
        window.CajaConfig.preservarPosicionEnMobile = true;
        window.CajaConfig.listadoItemVentasUnico = {(MostrarListadoPreciosDetalle() ? "true" : "false")};
        window.CajaConfig.editarPrecioSinAutorizacion = {(PuedeEditarDetalleCaja() ? "true" : "false")};
        window.CajaConfig.eliminarDetalleSinAutorizacion = {(PuedeEliminarDetalleCaja() ? "true" : "false")};

        if (window.CajaViewport) {{
            if (typeof window.CajaViewport.restaurarEstadoScroll === 'function') {{
                window.CajaViewport.restaurarEstadoScroll();
            }}
            if (typeof window.CajaViewport.activarFocusBuscadorSiAplica === 'function') {{
                window.CajaViewport.activarFocusBuscadorSiAplica();
            }}
        }}
        ",
                true
            );
        }
        private void Cargar_RP()
        {
            SincronizarEstadoVisualMesas();
            rpCuentas.DataSource = models.cuentas;
            rpCuentasModal.DataSource = models.cuentas;
            rpCuentasCliente.DataSource = CuentasClienteActivas();
            rpDetalleCaja.DataSource = models.detalleCaja;
            rpZonas.DataSource = models.zonas;
            rpMesas.DataSource = models.Mesas;
            rpCategorias.DataSource = models.categorias;
            rpProductos.DataSource = models.productosLista ?? models.productos;
            DataBind();
        }

        private void SincronizarEstadoVisualMesas()
        {
            if (models?.MesasLista == null || !models.MesasLista.Any())
            {
                return;
            }

            var mesasOcupadas = new HashSet<string>(
                (models?.cuentasMesasVista ?? new List<V_CuentasVenta>())
                    .Where(x => x != null && !x.eliminada && !string.IsNullOrWhiteSpace(x.nombremesa))
                    .Select(x => x.nombremesa.Trim()),
                StringComparer.OrdinalIgnoreCase);

            foreach (var mesa in models.MesasLista)
            {
                if (mesa == null)
                {
                    continue;
                }

                mesa.estadoMesa = mesasOcupadas.Contains((mesa.nombreMesa ?? string.Empty).Trim()) ? 1 : 0;
            }

            if (models.Mesas != null)
            {
                foreach (var mesa in models.Mesas)
                {
                    if (mesa == null)
                    {
                        continue;
                    }

                    mesa.estadoMesa = mesasOcupadas.Contains((mesa.nombreMesa ?? string.Empty).Trim()) ? 1 : 0;
                }
            }
        }

        private async Task CargarListadoPreciosDetalleAsync()
        {
            _preciosDetallePorPresentacion.Clear();

            if (!MostrarListadoPreciosDetalle())
            {
                return;
            }

            var idsPresentacion = (models?.detalleCaja ?? new List<V_DetalleCaja>())
                .Where(x => x != null && x.idPresentacion > 0)
                .Select(x => x.idPresentacion)
                .Distinct()
                .ToList();

            if (!idsPresentacion.Any())
            {
                return;
            }

            var lista = await V_PreciosControler.ListaPorPresentaciones(models.db, idsPresentacion);
            foreach (var grupo in (lista ?? new List<V_Precios>()).GroupBy(x => x.idPresentacion))
            {
                _preciosDetallePorPresentacion[grupo.Key] = grupo
                    .OrderBy(x => x.nombrePrecio)
                    .ThenBy(x => x.id)
                    .ToList();
            }
        }

        private Task<bool> VerificarSession()
        {
            return Task.FromResult(models?.vendedor?.id > 0);
        }

        private bool UsuarioActualEsCajero()
        {
            return models?.vendedor?.cajaMovil == 1;
        }

        private async Task CargarPermisosDetalleCajeroAsync()
        {
            _puedeEditarDetalleVentaCajero = false;
            _puedeEliminarDetalleVentaCajero = false;

            if (!UsuarioActualEsCajero())
            {
                return;
            }

            var db = models?.db;
            var idCajero = models?.vendedor?.id ?? 0;
            if (string.IsNullOrWhiteSpace(db) || idCajero <= 0)
            {
                return;
            }

            if (Session[SessionPuedeEditarDetalleVenta] != null && Session[SessionPuedeEliminarDetalleVenta] != null)
            {
                _puedeEditarDetalleVentaCajero = Convert.ToBoolean(Session[SessionPuedeEditarDetalleVenta]);
                _puedeEliminarDetalleVentaCajero = Convert.ToBoolean(Session[SessionPuedeEliminarDetalleVenta]);
                return;
            }

            var permisos = await V_R_PermisoCajeroControler.Lista(db, idCajero) ?? new List<V_R_PermisoCajero>();
            _puedeEditarDetalleVentaCajero = permisos.Any(x =>
                string.Equals((x?.nombrePermiso ?? string.Empty).Trim(), PermisoEditarDetalleVenta, StringComparison.OrdinalIgnoreCase));
            _puedeEliminarDetalleVentaCajero = permisos.Any(x =>
                string.Equals((x?.nombrePermiso ?? string.Empty).Trim(), PermisoEliminarDetalleVenta, StringComparison.OrdinalIgnoreCase));

            Session[SessionPuedeEditarDetalleVenta] = _puedeEditarDetalleVentaCajero;
            Session[SessionPuedeEliminarDetalleVenta] = _puedeEliminarDetalleVentaCajero;
        }
        private async Task IniciarPagina()
        {

            // Obtener cuentas del vendedor
            var cuentas = new List<V_CuentasVenta>();
            cuentas = await CargarCuentas();

            int idVenta;
            if (!cuentas.Any())
            {
                idVenta = await TablaVentas_f.NuevaVenta(models.db, models.Sede.porcentaje_propina);
                if (idVenta <= 0)
                {
                    AlertModerno.Error(this, "Error", "No fue posible crear una nueva cuenta.", true);
                    return;
                }

                var relacionado = await R_VentaVendedor_f.Relacionar_Vendedor_Venta(models.db, idVenta, models.vendedor.id);
                if (!relacionado)
                {
                    AlertModerno.Error(this, "Error", "No fue posible crear la relaci\u00f3n del vendedor con la venta.", true);
                    // a\u00fan as\u00ed intentamos recargar cuentas para que la UI no quede rota
                }

                // recargar cuentas
                cuentas = await V_CuentasVentaControler.Lista_IdVendedor(models.db, models.vendedor.id) ?? new List<V_CuentasVenta>();
            }
            else
            {
                idVenta = cuentas.First().id;
            }

            // Cargar colecciones base
            var zonasTask = ObtenerZonasAsync();
            var categoriasTask = ObtenerCategoriasAsync();
            var mesasTask = ObtenerMesasAsync();
            var productosTask = ObtenerProductosAsync();
            var metodosPagoTask = ObtenerMetodosPagoAsync();
            var mediosPagoInternosTask = ObtenerMediosPagoInternosAsync();
            var relMediosPagoInternosTask = ObtenerRelMediosPagoInternosAsync();
            var cuentasClienteTask = V_CuentaClienteCotroler.Lista(models.db, false, idVenta);
            var cuentasVistaTask = CargarCuentasMesasVista();
            var ventaTask = V_TablaVentasControler.Consultar_Id(models.db, idVenta);
            var ventaCuentaTask = V_CuentaClienteCotroler.Consultar(models.db, 0);
            var detalleTask = V_DetalleCajaControler.Lista_IdVenta(models.db, idVenta, 0);
            var adicionesTask = ObtenerAdicionesAsync();
            var clienteDomiciliosTask = ObtenerClienteDomiciliosAsync();
            var cargoDescuentoTask = CargoDescuentoVentasControler.ObtenerPorVenta(models.db, idVenta);

            await Task.WhenAll(
                zonasTask,
                categoriasTask,
                mesasTask,
                productosTask,
                metodosPagoTask,
                mediosPagoInternosTask,
                relMediosPagoInternosTask,
                cuentasClienteTask,
                cuentasVistaTask,
                ventaTask,
                ventaCuentaTask,
                detalleTask,
                adicionesTask,
                clienteDomiciliosTask,
                cargoDescuentoTask);

            var zonas = zonasTask.Result ?? new List<Zonas>();
            if (!zonas.Any())
            {
                divZonas.Attributes["class"] = "d-none";
                divProductos.Attributes["class"] = "col-12 col-lg-12";
            }
            else
            {
                divZonas.Attributes["class"] = "col-12 col-lg-5 d-flex";
                divProductos.Attributes["class"] = "col-12 col-lg-7";
            }
            var categorias = categoriasTask.Result ?? new List<V_Categoria>();
            var mesas = mesasTask.Result ?? new List<Mesas>();
            var productos = productosTask.Result ?? new List<v_productoVenta>();
            var metodosPago = (metodosPagoTask.Result ?? new List<payment_methods>())
                .Where(x => x != null && x.state)
                .ToList();
            var mediosPagoInternos = (mediosPagoInternosTask.Result ?? new List<MediosDePagoInternos>())
                .Where(x => x != null && x.estado == 1)
                .ToList();
            var relMediosPagoInternos = relMediosPagoInternosTask.Result ?? new List<V_R_MediosDePago_MediosDePagoInternos>();
            if (!productos.Any())
            {
                AlertModerno.Error(this, "Error", "No fue posible cargar la lista de productos.", true);
            }
            int idZonaActiva = zonas.FirstOrDefault()?.id ?? 0;
            int idCategoriaActiva = categorias.FirstOrDefault()?.id ?? 0;
            var listacc = cuentasClienteTask.Result ?? new List<V_CuentaCliente>();

            // Construir ViewModel
            models.IdCuentaActiva = idVenta;
            if (zonas != null && zonas.Any())
            {
                models.IdZonaActiva = zonas.First().id;
            }
            else
            {
                models.IdZonaActiva = 0;
            }
            if (mesas != null && mesas.Any())
            {
                models.IdMesaActiva = mesas.First().id;
            }
            else
            {
                models.IdMesaActiva = 0;
            }
            if (categorias != null && categorias.Any())
            {
                models.IdCategoriaActiva = categorias.First().id;
            }
            else
            {
                models.IdCategoriaActiva = 0;
            }
            models.IdCuenteClienteActiva = 0;
            models.cuentasMesasVista = cuentasVistaTask.Result ?? new List<V_CuentasVenta>();
            models.cuentas = FiltrarCuentasActivasPorVendedor(models.cuentasMesasVista);
            models.zonas = zonas;
            models.MesasLista = mesas;
            models.Mesas = ConstruirMesasVisibles(mesas, models.IdZonaActiva);
            models.categorias = categorias;
            models.productosLista = productos;
            models.productos = productos;
            models.venta = ventaTask.Result;
            models.ventaCuenta = ventaCuentaTask.Result;
            models.detalleCaja = detalleTask.Result;
            models.v_CuentaClientes = listacc;
            models.adiciones = adicionesTask.Result;
            models.clienteDomicilios = clienteDomiciliosTask.Result;
            models.clienteDomicilioActivo = await CargarClienteDomicilioActivo(idVenta);
            models.AbrirModalDomicilio = false;
            models.cargoDescuentoVentas = cargoDescuentoTask.Result;
            models.metodosPago = metodosPago;
            models.mediosPagoInternos = mediosPagoInternos;
            models.relMediosPagoInternos = relMediosPagoInternos;
            models.IdCuentaActiva = idVenta;



            await CargarDATA();
        }
        private async Task<List<V_CuentasVenta>> CargarCuentas()
        {
            var cuentas = new List<V_CuentasVenta>();
            if (models.vendedor.cajaMovil == 1 || ajustes?.meserosCompartidos == true)
            {
                cuentas = await V_CuentasVentaControler.Lista_Cajero(models.db) ?? new List<V_CuentasVenta>();
            }
            else
            {
                cuentas = await V_CuentasVentaControler.Lista_IdVendedor(models.db, models.vendedor.id) ?? new List<V_CuentasVenta>();
            }
            return (cuentas ?? new List<V_CuentasVenta>()).Where(x => !x.eliminada).ToList();
        }

        private List<V_CuentasVenta> FiltrarCuentasActivasPorVendedor(IEnumerable<V_CuentasVenta> cuentasVista)
        {
            var visibles = (cuentasVista ?? Enumerable.Empty<V_CuentasVenta>())
                .Where(x => x != null && !x.eliminada)
                .ToList();

            if (models?.vendedor?.cajaMovil == 1 || ajustes?.meserosCompartidos == true)
            {
                return visibles;
            }

            var idVendedor = models?.vendedor?.id ?? 0;
            return visibles.Where(x => x.idvendedor == idVendedor).ToList();
        }

        private List<Mesas> ConstruirMesasVisibles(IEnumerable<Mesas> mesas, int idZonaActiva)
        {
            var lista = (mesas ?? Enumerable.Empty<Mesas>())
                .Where(x => x != null)
                .ToList();

            if (!lista.Any())
            {
                return new List<Mesas>();
            }

            var mesasZonaActiva = lista.Where(x => x.idZona == idZonaActiva).ToList();
            if (!UsuarioActualEsCajero())
            {
                return mesasZonaActiva;
            }

            var ocupadasFueraZona = lista
                .Where(x => x.estadoMesa == 1 && x.idZona != idZonaActiva)
                .OrderBy(x => x.idZona)
                .ThenBy(x => x.nombreMesa)
                .ToList();

            return mesasZonaActiva
                .Concat(ocupadasFueraZona)
                .GroupBy(x => x.id)
                .Select(x => x.First())
                .ToList();
        }

        private async Task<List<V_CuentasVenta>> CargarCuentasMesasVista()
        {
            var cuentas = await V_CuentasVentaControler.Lista_Cajero(models.db) ?? new List<V_CuentasVenta>();
            return (cuentas ?? new List<V_CuentasVenta>()).Where(x => !x.eliminada).ToList();
        }

        private async Task ActualizarColeccionesDeCuentasAsync()
        {
            var cuentasVista = await CargarCuentasMesasVista();
            models.cuentasMesasVista = cuentasVista;
            models.cuentas = FiltrarCuentasActivasPorVendedor(cuentasVista);
        }

        private async Task ActualizarCuentaEnColeccionesAsync(int idVenta)
        {
            if (idVenta <= 0)
            {
                return;
            }

            var cuentaActualizada = await V_CuentasVentaControler.Consultar_Id(models.db, idVenta);
            if (cuentaActualizada == null || cuentaActualizada.eliminada)
            {
                models?.cuentas?.RemoveAll(x => x != null && x.id == idVenta);
                models?.cuentasMesasVista?.RemoveAll(x => x != null && x.id == idVenta);
                return;
            }

            if (models.cuentasMesasVista == null)
            {
                models.cuentasMesasVista = new List<V_CuentasVenta>();
            }

            ActualizarCuentaEnColeccion(models.cuentasMesasVista, cuentaActualizada);

            var visibleParaUsuarioActual = models.vendedor.cajaMovil == 1
                || ajustes?.meserosCompartidos == true
                || cuentaActualizada.idvendedor == models.vendedor.id;

            if (visibleParaUsuarioActual)
            {
                if (models.cuentas == null)
                {
                    models.cuentas = new List<V_CuentasVenta>();
                }

                ActualizarCuentaEnColeccion(models.cuentas, cuentaActualizada);
            }
            else
            {
                models?.cuentas?.RemoveAll(x => x != null && x.id == idVenta);
            }
        }

        private void ActualizarCuentaEnColeccion(List<V_CuentasVenta> cuentas, V_CuentasVenta cuenta)
        {
            if (cuentas == null || cuenta == null || cuenta.id <= 0)
            {
                return;
            }

            cuentas.RemoveAll(x => x != null && x.id == cuenta.id);
            cuentas.Insert(0, cuenta);
        }

        private void MarcarMesaComoOcupadaEnModelos(int idMesa)
        {
            ActualizarEstadoMesaEnModelos(idMesa, 1);
        }

        private void MarcarMesaComoLibreEnModelos(int idMesa)
        {
            ActualizarEstadoMesaEnModelos(idMesa, 0);
        }

        private void ActualizarEstadoMesaEnModelos(int idMesa, int estadoMesa)
        {
            if (idMesa <= 0 || models?.MesasLista == null)
            {
                return;
            }

            var mesaLista = models.MesasLista.FirstOrDefault(x => x.id == idMesa);
            if (mesaLista != null)
            {
                mesaLista.estadoMesa = estadoMesa;
            }

            var mesaVista = models.Mesas?.FirstOrDefault(x => x.id == idMesa);
            if (mesaVista != null)
            {
                mesaVista.estadoMesa = estadoMesa;
            }
        }

        private async Task<List<Zonas>> ObtenerZonasAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCajaZonasKey] is List<Zonas> cache)
            {
                return cache;
            }

            var lista = await ZonasControler.Lista(models.db) ?? new List<Zonas>();
            Session[SessionCajaZonasKey] = lista;
            return lista;
        }

        private async Task<List<V_Categoria>> ObtenerCategoriasAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCajaCategoriasKey] is List<V_Categoria> cache)
            {
                return cache;
            }

            var lista = await V_CategoriaControler.lista(models.db) ?? new List<V_Categoria>();
            Session[SessionCajaCategoriasKey] = lista;
            return lista;
        }

        private async Task<List<Mesas>> ObtenerMesasAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCajaMesasKey] is List<Mesas> cache)
            {
                return cache;
            }

            var lista = await MesasControler.Lista(models.db) ?? new List<Mesas>();
            Session[SessionCajaMesasKey] = lista;
            return lista;
        }

        private async Task<List<v_productoVenta>> ObtenerProductosAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCajaProductosKey] is List<v_productoVenta> cache)
            {
                return cache;
            }

            var lista = await v_productoVentaControler.Lista(models.db) ?? new List<v_productoVenta>();
            Session[SessionCajaProductosKey] = lista;
            return lista;
        }

        private async Task<List<payment_methods>> ObtenerMetodosPagoAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCajaMetodosPagoKey] is List<payment_methods> cache)
            {
                return cache;
            }

            var lista = await payment_methodsControler.ListaMetodosDePago(models.db) ?? new List<payment_methods>();
            Session[SessionCajaMetodosPagoKey] = lista;
            return lista;
        }

        private async Task<List<MediosDePagoInternos>> ObtenerMediosPagoInternosAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCajaMediosPagoInternosKey] is List<MediosDePagoInternos> cache)
            {
                return cache;
            }

            var lista = await MediosDePagoInternos_Controler.Lista(models.db) ?? new List<MediosDePagoInternos>();
            Session[SessionCajaMediosPagoInternosKey] = lista;
            return lista;
        }

        private async Task<List<V_R_MediosDePago_MediosDePagoInternos>> ObtenerRelMediosPagoInternosAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCajaRelMediosPagoInternosKey] is List<V_R_MediosDePago_MediosDePagoInternos> cache)
            {
                return cache;
            }

            var lista = await V_R_MediosDePago_MediosDePagoInternosControler.GetAll(models.db) ?? new List<V_R_MediosDePago_MediosDePagoInternos>();
            Session[SessionCajaRelMediosPagoInternosKey] = lista;
            return lista;
        }

        private async Task<List<V_CatagoriaAdicion>> ObtenerAdicionesAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && Session[SessionCajaAdicionesKey] is List<V_CatagoriaAdicion> cache)
            {
                return cache;
            }

            var lista = await V_CatagoriaAdicionControler.Lista(models.db) ?? new List<V_CatagoriaAdicion>();
            Session[SessionCajaAdicionesKey] = lista;
            return lista;
        }

        private async Task<List<ClienteDomicilio>> ObtenerClienteDomiciliosAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && models?.clienteDomicilios != null && models.clienteDomicilios.Any())
            {
                return models.clienteDomicilios;
            }

            if (!forceRefresh && Session[SessionCajaClienteDomiciliosKey] is List<ClienteDomicilio> cache)
            {
                return cache;
            }

            var lista = await ClienteDomicilioControler.Lista(models.db) ?? new List<ClienteDomicilio>();
            Session[SessionCajaClienteDomiciliosKey] = lista;
            return lista;
        }

        private async Task PrepararServicioRecienCreadoAsync(int idVenta, string nombreMesa = "")
        {
            if (idVenta <= 0)
            {
                return;
            }

            models.IdCuenteClienteActiva = 0;
            models.IdCuentaActiva = idVenta;

            var ventaCreada = await V_TablaVentasControler.Consultar_Id(models.db, idVenta) ?? new V_TablaVentas { id = idVenta };
            models.venta = ventaCreada;
            models.detalleCaja = new List<V_DetalleCaja>();
            models.v_CuentaClientes = new List<V_CuentaCliente>();
            models.ventaCuenta = new V_CuentaCliente();
            models.clienteDomicilioActivo = new ClienteDomicilio();
            models.cargoDescuentoVentas = new List<CargoDescuentoVentas>();
            models.AbrirModalDomicilio = false;

            var cuentaNueva = new V_CuentasVenta
            {
                id = idVenta,
                aliasVenta = !string.IsNullOrWhiteSpace(ventaCreada?.aliasVenta) ? ventaCreada.aliasVenta : idVenta.ToString(),
                efectivoVenta = ventaCreada?.efectivoVenta ?? 0,
                numeroVenta = ventaCreada?.numeroVenta ?? 0,
                eliminada = ventaCreada?.eliminada ?? false,
                total = ventaCreada?.total_A_Pagar ?? 0,
                idbase = ventaCreada?.idBaseCaja ?? 0,
                idvendedor = models?.vendedor?.id ?? 0,
                nombrevendedor = models?.vendedor?.nombreVendedor ?? string.Empty,
                nombremesa = nombreMesa ?? string.Empty,
                nombreCD = string.Empty
            };

            if (models.cuentasMesasVista == null)
            {
                models.cuentasMesasVista = new List<V_CuentasVenta>();
            }

            if (models.cuentas == null)
            {
                models.cuentas = new List<V_CuentasVenta>();
            }

            ActualizarCuentaEnColeccion(models.cuentasMesasVista, cuentaNueva);

            if (models.vendedor.cajaMovil == 1 || ajustes?.meserosCompartidos == true || cuentaNueva.idvendedor == models.vendedor.id)
            {
                ActualizarCuentaEnColeccion(models.cuentas, cuentaNueva);
            }
        }

        protected async void Evento_Click(object sender, EventArgs e)
        {
            if (!await DeserializarModels())
            {
                AlertModerno.ErrorRedirect(this, "Error", "La sesion expiro o no contiene el contexto de trabajo.", "Default.aspx");
                return;
            }

            string accion = hidAccion.Value;
            string eventArgument = hidArgumento.Value;

            switch (accion)
            {
                case "Actualizar":
                    await Actualizar();
                    break;

                case "NuevoServicio":
                    await NuevoServicio();
                    break;

                case "AperturarCajon":
                    await AperturarCajonMonedero();
                    break;

                case "EliminarServicio":
                    await EliminarServicio();
                    break;

                case "SeleccionarCuenta":
                    await SeleccionarCuenta(eventArgument);
                    break;

                case "EditarAliasCuenta":
                    await EditarAliasCuenta(eventArgument);
                    break;

                case "SeleccionarZona":
                    await SeleccionarZona(eventArgument);
                    break;

                case "SeleccionarMesa":
                    await SeleccionarMesa(eventArgument);
                    break;

                case "AccionMesa_CrearServicio":
                    await AccionMesa_CrearServicio();
                    break;

                case "AccionMesa_AmarrarMesa":
                    await AccionMesa_AmarrarMesa();
                    break;

                case "LiberarMesa":
                    await LiberarMesa();
                    break;

                case "AmarrarMesaCuenta":
                    await AmarrarMesaCuenta(eventArgument);
                    break;

                case "SeleccionarCategoria":
                    await SeleccionarCategoria(eventArgument);
                    break;


                case "BuscarCodigoProducto":
                    await BuscarCodigoProducto(eventArgument);
                    break;

                case "AgregarProducto":
                    await AgregarProducto(eventArgument);
                    break;

                case "SeleccionarCuentaCliente":
                    await SeleccionarCuentaCliente(eventArgument);
                    break;

                case "CrearCuentaCliente":
                    await CrearCuentaCliente(eventArgument);
                    break;

                case "ActualizarCantidadDetalle":
                    await ActualizarCantidadDetalle(eventArgument);
                    break;

                case "EliminarDetalle":
                    await EliminarDetalleCaja(eventArgument);
                    break;

                case "SolicitarAutorizacionDetalle":
                    await SolicitarAutorizacionDetalle(eventArgument);
                    break;

                case "GuardarNotaDetalle":
                    await GuardarNotaDetalle(eventArgument);
                    break;

                case "DividirDetalle":
                    await DividirDetalle(eventArgument);
                    break;

                case "AnclarDetalleCuenta":
                    await AnclarDetalleCuenta(eventArgument);
                    break;

                case "EditarValorDetalle":
                    await EditarValorDetalle(eventArgument);
                    break;

                case "EditarNombreDetalle":
                    await EditarNombreDetalle(eventArgument);
                    break;

                case "EditarPropina":
                    await EditarPropina(eventArgument);
                    break;

                case "Domicilio":
                    await Domicilio(eventArgument);
                    break;

                case "CrearActualizarClienteDomicilio":
                    await CrearActualizarClienteDomicilio(eventArgument);
                    break;

                case "SeleccionarClienteDomicilio":
                    await SeleccionarClienteDomicilio(eventArgument);
                    break;

                case "ActualizarEstadoDomicilio":
                    await ActualizarEstadoDomicilio(eventArgument);
                    break;

                case "GuardarCobroDomicilio":
                    await GuardarCobroDomicilio(eventArgument);
                    break;

                case "DespacharDomicilio":
                    await DespacharDomicilio();
                    break;

                case "Comandar":
                    await Comandar();
                    break;

                case "SolicitarCuenta":
                    await SolicitarCuenta();
                    break;

                case "Cobrar":
                    await Cobrar();
                    break;

                case "CerrarCaja":
                    await CerrarCaja();
                    break;

                case "CerrarSesion":
                    await CerrarSesion();
                    break;

                case "Ventas":
                    await Ventas();
                    break;
            }
        }
        private async Task Actualizar()
        {
            if (!await RecargarAjustesDb())
            {
                AlertModerno.Error(this, "Error", "No fue posible recargar la configuración de DBConexion.", true);
                return;
            }

            // El catálogo se conserva en Session para agilizar los postbacks normales.
            // En una actualización solicitada por el cajero se debe omitir ese caché para
            // reflejar inmediatamente productos, precios y categorías modificados en SQL.
            await Task.WhenAll(
                ObtenerProductosAsync(true),
                ObtenerCategoriasAsync(true));

            await IniciarPagina();
            AlertModerno.Success(this, "Ok", "Productos y precios actualizados desde la base de datos.", true);
        }
        private async Task NuevoServicio()
        {
            var respNuevaVenta = await TablaVentas_f.NuevaVentaDetallada(
                models.db,
                models.Sede.porcentaje_propina,
                models.Sede?.id ?? 0,
                SessionContextHelper.ResolveBaseCajaId(Session, models));

            int idVenta = respNuevaVenta != null && respNuevaVenta.estado && respNuevaVenta.data != null
                ? Convert.ToInt32(respNuevaVenta.data)
                : 0;

            if (idVenta <= 0)
            {
                AlertModerno.Error(this, "Error", respNuevaVenta?.mensaje ?? "No se cre\u00f3 el servicio.", true, 2600);
                return;
            }


            // amarro venta con vendedor (uso session idvendedor si existe)
            var rvv = await R_VentaVendedor_f.Relacionar_Vendedor_Venta(models.db, idVenta, models.vendedor.id);
            if (rvv)
            {
                await PrepararServicioRecienCreadoAsync(idVenta);
                await CargarDATA();
                AlertModerno.Success(this, "Listo", $"Servicio #{idVenta} creado con \u00e9xito.", true, 2000);
            }
            else
            {
                AlertModerno.Error(this, "Error", $"Servicio #{idVenta} creado con \u00e9xito, pero no se amarr\u00f3 al vendedor.", true, 2000);
            }

        }

        private async Task AperturarCajonMonedero()
        {
            var respCajon = await AperturarCajonRequestHelper.EnviarAsync(models.db, Session, models);

            if (!respCajon)
            {
                AlertModerno.Error(this, "Error", "No fue posible aperturar el cajón monedero.", true, 1800);
                return;
            }

            AlertModerno.Success(this, "OK", "Cajón abierto correctamente.", true, 1600);
        }

        private async Task EliminarServicio()
        {
            int idVenta = models.IdCuentaActiva;
            if (idVenta <= 0)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "No hay un servicio activo para eliminar.", true, 2200);
                return;
            }

            var detalles = await V_DetalleCajaControler.Lista_IdVenta(models.db, idVenta, models.IdCuenteClienteActiva);
            if (detalles != null && detalles.Count > 0)
            {
                AlertModerno.Error(this, "Error", $"El servicio #{idVenta} a\u00fan tiene items cargados.", true, 2200);
                return;
            }

            var rsp = await TablaVentasControler.Consultar_Id(models.db, idVenta);
            if (!rsp.estado)
            {
                AlertModerno.Error(this, "Error", $"El servicio #{idVenta} no se pudo eliminar.", true, 2200);
                return;
            }

            var venta = rsp.data as TablaVentas;
            if (venta == null)
            {
                venta = JsonConvert.DeserializeObject<TablaVentas>(rsp.data);
            }
            if (venta == null)
            {
                AlertModerno.Error(this, "Error", $"El servicio #{idVenta} no se pudo eliminar.", true, 2200);
                return;
            }

            venta.eliminada = true;
            var rspCrud = await TablaVentasControler.CRUD(models.db, venta, 1);
            if (!rspCrud.estado)
            {
                AlertModerno.Error(this, "Error", $"El servicio #{idVenta} no se pudo eliminar.", true, 2200);
                return;
            }

            var relaciones = await R_VentaMesaControler.ListaRelacion(models.db, idVenta) ?? new List<R_VentaMesa>();
            foreach (var relacion in relaciones)
            {
                var mesa = await MesasControler.Consultar_id(models.db, relacion.idMesa);
                if (mesa != null)
                {
                    mesa.estadoMesa = 0;
                    await MesasControler.CRUD(models.db, mesa, 1);
                }

                await R_VentaMesaControler.CRUD(models.db, relacion, 2);
            }

            await IniciarPagina();
            AlertModerno.Success(this, "OK", $"Servicio #{idVenta} eliminado con \u00e9xito.", true, 1600);
        }

        private async Task EditarAliasCuenta(string parametros)
        {
            try
            {

                var args = new EventArgumentParser(parametros);

                int idCuenta = args.GetInt("ID");
                string alias = args.GetString("ALIAS")?.Trim();

                if (idCuenta <= 0)
                {
                    AlertModerno.Warning(this, "Atenci\u00f3n", "No se recibi\u00f3 un ID v\u00e1lido.", true, 2500);
                    return;
                }

                if (string.IsNullOrWhiteSpace(alias) || alias.Length < 2)
                {
                    AlertModerno.Warning(this, "Atenci\u00f3n", "El nombre debe tener m\u00ednimo 2 caracteres.", true, 2500);
                    return;
                }

                var venta = new TablaVentas();
                var resp = await TablaVentasControler.Consultar_Id(models.db, Convert.ToInt32(idCuenta));
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", $"no se encontro la venta {idCuenta}");
                    return;
                }

                venta = JsonConvert.DeserializeObject<TablaVentas>(resp.data);
                venta.aliasVenta = alias;
                var crud = await TablaVentasControler.CRUD(models.db, venta, 1);
                if (!crud.estado)
                {
                    AlertModerno.Error(this, "Error", $"no se modifico el alias");
                }



                await ActualizarCuentaEnColeccionesAsync(idCuenta);
                await CargarDATA();

                AlertModerno.Success(this, "Ok", "Cuenta actualizada correctamente.", true, 2200);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 3000);
            }
        }

        private async Task SeleccionarCuenta(string parametros)
        {

            var args = new EventArgumentParser(parametros);

            int idCuenta = args.GetInt("ID");

            if (idCuenta > 0) 
            {
                //consultamos una relacion 
                var r = await R_VentaMesaControler.ListaRelacion(models.db,idCuenta);
                if(r.Count>0)
                {
                    int idmesa = r.FirstOrDefault().idMesa;
                    var mesa = models.MesasLista.Where(x => x.id == idmesa).FirstOrDefault();
                    if(mesa != null)
                    {
                        models.IdZonaActiva = mesa.idZona;
                        models.Mesas = ConstruirMesasVisibles(models.MesasLista, mesa.idZona);
                    }
                }
                models.IdCuenteClienteActiva = 0;
                models.IdCuentaActiva = idCuenta;
                var ventaTask = V_TablaVentasControler.Consultar_Id(models.db, idCuenta);
                var ventaCuentaTask = V_CuentaClienteCotroler.Consultar(models.db, 0);
                var detalleTask = V_DetalleCajaControler.Lista_IdVenta(models.db, idCuenta, 0);
                var clienteDomicilioTask = CargarClienteDomicilioActivo(idCuenta);
                await Task.WhenAll(ventaTask, ventaCuentaTask, detalleTask, clienteDomicilioTask);

                models.venta = ventaTask.Result;
                models.ventaCuenta = ventaCuentaTask.Result;
                models.detalleCaja = detalleTask.Result;
                models.clienteDomicilioActivo = clienteDomicilioTask.Result;

                await CargarDATA();
            }


        }

        private async Task SeleccionarZona(string parametros)
        {
            var datos=new EventArgumentParser(parametros);
            int idZona = datos.GetInt("ID"); 

            if(idZona > 0)
            {
                models.IdZonaActiva = idZona;
                models.Mesas = ConstruirMesasVisibles(models.MesasLista, idZona);

                await CargarDATA();
            }
        }

        private async Task SeleccionarMesa(string parametros)
        {
            try
            {
                // ? Siempre trabajar con models actualizado (viene de Session)
                await DeserializarModels();

                var data = new EventArgumentParser(parametros);
                int idMesa = data.GetInt("ID");

                if (idMesa <= 0)
                {
                    AlertModerno.Warning(this, "Atenci\u00f3n", "No se recibi\u00f3 una mesa v\u00e1lida.", true, 2200);
                    return;
                }

                V_CuentasVenta cuentaMesa;
                if (!PuedeGestionarMesa(idMesa, out cuentaMesa))
                {
                    var mesaBloqueada = models.MesasLista?.FirstOrDefault(x => x.id == idMesa);
                    var nombreMesa = mesaBloqueada?.nombreMesa ?? $"Mesa #{idMesa}";
                    var nombreVendedor = string.IsNullOrWhiteSpace(cuentaMesa?.nombrevendedor) ? "otro mesero" : cuentaMesa.nombrevendedor;
                    AlertModerno.Warning(this, "Atención", $"La {nombreMesa} pertenece a {nombreVendedor}. Con meseros compartidos desactivado solo puedes verla, no gestionarla.", true, 2600);
                    return;
                }

                // ? Set activo
                models.IdMesaActiva = idMesa;

                // ? Buscar mesa sin riesgo de null
                var mesa = models.MesasLista?.FirstOrDefault(x => x.id == idMesa);
                lblMesaSeleccionada.InnerText = mesa != null ? mesa.nombreMesa : $"Mesa #{idMesa}";

                // Cargar data dependiente (tu l\u00f3gica)
                await CargarDATA();


                // Abrir modal (despu\u00e9s de cargar/persistir)
                ModalHelper.Open(this, modalAccionesMesa);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 3000);
            }
        }
        private async Task AccionMesa_CrearServicio()
        {
            var respNuevaVenta = await TablaVentas_f.NuevaVentaDetallada(
                models.db,
                models.Sede.porcentaje_propina,
                models.Sede?.id ?? 0,
                SessionContextHelper.ResolveBaseCajaId(Session, models));

            int idVenta = respNuevaVenta != null && respNuevaVenta.estado && respNuevaVenta.data != null
                ? Convert.ToInt32(respNuevaVenta.data)
                : 0;

            if (idVenta <= 0)
            {
                AlertModerno.Error(this, "Error", respNuevaVenta?.mensaje ?? "No se cre\u00f3 el servicio.", true, 2600);
                return;
            }


            // amarro venta con vendedor (uso session idvendedor si existe)
            var rvv = await R_VentaVendedor_f.Relacionar_Vendedor_Venta(models.db, idVenta, models.vendedor.id);
            if (!rvv)
            {
                AlertModerno.Error(this, "Error", $"Servicio #{idVenta} creado con \u00e9xito, pero no se amarr\u00f3 al vendedor.", true, 2000);
                return;
            }

            //amaramos la mesa con la cuenta

            var rv = await R_VentaMesaControler.Consultar_relacion(models.db, idVenta, models.IdMesaActiva);
            if (rv != null)
            {
                                AlertModerno.Warning(this, "Atenci\u00f3n", $"La mesa seleccionada ya est\u00e1 amarrada con la cuenta {idVenta}.", true, 2200);
                return;
            }

            rv = new R_VentaMesa { id = 0, idMesa = models.IdMesaActiva, idVenta = idVenta };
            var resp = await R_VentaMesaControler.CRUD(models.db, rv, 0);
            if (!resp.estado)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "No fue posible terminar el proceso.", true, 2200);
                return;
            }

            //cambiamos el estado de la mesa
            var mesa = await MesasControler.Consultar_id(models.db, models.IdMesaActiva);
            if (mesa == null)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "No se encontr\u00f3 la mesa.", true, 2200);
                return;
            }

            mesa.estadoMesa = 1;
            var respm = await MesasControler.CRUD(models.db, mesa, 1);
            if (!respm.estado)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", $"No se logr\u00f3 cambiar el estado de la mesa: {mesa.nombreMesa}.", true, 2200);
                return;
            }

            await PrepararServicioRecienCreadoAsync(idVenta, mesa.nombreMesa);
            MarcarMesaComoOcupadaEnModelos(models.IdMesaActiva);
            await CargarDATA();

            AlertModerno.Success(this, "Listo", $"La mesa {mesa.nombreMesa} se relacion\u00f3 correctamente con la cuenta {idVenta}", true, 2000);
        }

        private async Task LiberarMesa()
        {
            try
            {
                if (models.IdMesaActiva <= 0)
                {
                    AlertModerno.Warning(this, "Atenci\u00f3n", "No se ha seleccionado una mesa v\u00e1lida.", true, 2200);
                    return;
                }

                var mesa = await MesasControler.Consultar_id(models.db, models.IdMesaActiva);
                if (mesa == null)
                {
                    AlertModerno.Warning(this, "Atenci\u00f3n", "No se encontr\u00f3 la mesa seleccionada.", true, 2200);
                    return;
                }

                mesa.estadoMesa = 0;
                var actualizarMesa = await MesasControler.CRUD(models.db, mesa, 1);
                if (!actualizarMesa.estado)
                {
                    AlertModerno.Error(this, "Error", $"No fue posible liberar la mesa {mesa.nombreMesa}.", true);
                    return;
                }

                var relacionesMesa = await R_VentaMesaControler.ListaPorMesa(models.db, mesa.id) ?? new List<R_VentaMesa>();
                foreach (var relacion in relacionesMesa)
                {
                    if (relacion == null)
                    {
                        continue;
                    }

                    var eliminarRelacion = await R_VentaMesaControler.CRUD(models.db, relacion, 2);
                    if (!eliminarRelacion.estado)
                    {
                        AlertModerno.Error(this, "Error", $"La mesa {mesa.nombreMesa} cambi\u00f3 de estado, pero no se elimin\u00f3 la relaci\u00f3n con la cuenta.", true);
                        return;
                    }
                }

                MarcarMesaComoLibreEnModelos(mesa.id);
                await RecargarVentaActiva(true);

                AlertModerno.Success(this, "OK", $"Mesa {mesa.nombreMesa} liberada correctamente.", true, 1500);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }
        private Task AccionMesa_AmarrarMesa()
        {
            ModalHelper.Open(this,mdlCuentas);
            return Task.CompletedTask;
        }

        private async Task AmarrarMesaCuenta(string parametros)
        {
            var data =new EventArgumentParser(parametros);
            int idCuentaAmarrar=data.GetInt("id");

            if (idCuentaAmarrar <= 0)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "No se recibi\u00f3 una cuenta v\u00e1lida.", true, 2200);
                return;
            }

            var rv = await R_VentaMesaControler.Consultar_relacion(models.db,idCuentaAmarrar,models.IdMesaActiva);
            if (rv != null)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", $"La mesa seleccionada ya est\u00e1 amarrada con la cuenta {idCuentaAmarrar}.", true, 2200);
                return;
            }

            rv=new R_VentaMesa { id=0, idMesa=models.IdMesaActiva, idVenta=idCuentaAmarrar };
            var resp = await R_VentaMesaControler.CRUD(models.db,rv,0);
            if (!resp.estado)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "No fue posible terminar el proceso.", true, 2200);
                return;
            }

            //cambiamos el estado de la mesa
            var mesa = await MesasControler.Consultar_id(models.db,models.IdMesaActiva);
            if (mesa == null)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "No se encontr\u00f3 la mesa.", true, 2200);
                return;
            }

            mesa.estadoMesa = 1;
            var respm = await MesasControler.CRUD(models.db,mesa,1);
            if (!respm.estado)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", $"No se logr\u00f3 cambiar el estado de la mesa: {mesa.nombreMesa}.", true, 2200);
                return;
            }

            MarcarMesaComoOcupadaEnModelos(models.IdMesaActiva);
            await ActualizarColeccionesDeCuentasAsync();
            models.IdCuentaActiva=idCuentaAmarrar;

            await CargarDATA();

            AlertModerno.Success(this,ok,"Mesa amarrada correctamente.");

        }

        private async Task BuscarCodigoProducto(string eventArgument)
        {
            try
            {
                string texto = (eventArgument ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(texto))
                {
                    ScriptManager.RegisterStartupScript(
                        this,
                        GetType(),
                        "CajaFocusBusquedaVacia",
                        "if (window.CajaViewport && typeof window.CajaViewport.activarFocusBuscadorSiAplica === 'function') { window.CajaViewport.activarFocusBuscadorSiAplica(); }",
                        true
                    );
                    return;
                }

                var producto = (models.productosLista ?? models.productos)
                    ?.FirstOrDefault(x => string.Equals(x.codigoProducto?.Trim(), texto, StringComparison.OrdinalIgnoreCase));

                if (producto == null)
                {
                    AlertModerno.Warning(this, "Atención", "Producto no encontrado", true, 2000);

                    ScriptManager.RegisterStartupScript(
                        this,
                        GetType(),
                        "CajaFocusProductoNoEncontrado",
                        "if (window.CajaViewport && typeof window.CajaViewport.activarFocusBuscadorSiAplica === 'function') { window.CajaViewport.activarFocusBuscadorSiAplica(); }",
                        true
                    );
                    return;
                }

                var resp = await DetalleVenta_f.AgregarProducto(models.db, producto, 1, models.IdCuentaActiva);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No fue posible agregar el producto.", true);
                    return;
                }

                if (models.IdCuenteClienteActiva > 0 && resp.data != null)
                {
                    var relacion = new R_CuentaCliente_DetalleVenta
                    {
                        id = 0,
                        fecha = DateTime.Now,
                        idCuentaCliente = models.IdCuenteClienteActiva,
                        idDetalleVenta = Convert.ToInt32(resp.data),
                        eliminada = false
                    };
                    await R_CuentaCliente_DetalleVentaControler.CRUD(models.db, relacion, 0);
                }

                AlertModerno.Success(this, "OK", resp.mensaje ?? "Producto agregado correctamente.", true, 800);
                models.IdCategoriaActiva = producto.idCategoria;
                await RecargarVentaActiva();

                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "LimpiarBuscadorCaja",
                    @"
    if (window.CajaBuscador) { CajaBuscador.clear(false); }
    if (window.CajaViewport) {
        if (typeof window.CajaViewport.restaurarEstadoScroll === 'function') {
            window.CajaViewport.restaurarEstadoScroll();
        }
        if (typeof window.CajaViewport.activarFocusBuscadorSiAplica === 'function') {
            window.CajaViewport.activarFocusBuscadorSiAplica();
        }
    }
    ",
                    true
                );
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task AgregarProducto(string parametros)
        {
            try
            {
                var data = new EventArgumentParser(parametros);
                int idPresentacion = data.GetInt("ID");
                decimal cantidad = Convert.ToDecimal((data.GetString("CANTIDAD") ?? "1").Replace(".", ","));

                if (idPresentacion <= 0)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No se recibiÃƒÂ³ un producto vÃƒÂ¡lido.", true, 2200);
                    return;
                }

                if (cantidad <= 0)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "La cantidad debe ser mayor a cero.", true, 2200);
                    return;
                }

                var producto = (models.productosLista ?? models.productos)
                    ?.FirstOrDefault(x => x.idPresentacion == idPresentacion);

                var resp = producto != null
                    ? await DetalleVenta_f.AgregarProducto(models.db, producto, cantidad, models.IdCuentaActiva)
                    : await DetalleVenta_f.AgregarProducto(models.db, idPresentacion, cantidad, models.IdCuentaActiva);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No fue posible agregar el producto.", true);
                    return;
                }

                if (models.IdCuenteClienteActiva > 0 && resp.data != null)
                {
                    var relacion = new R_CuentaCliente_DetalleVenta
                    {
                        id = 0,
                        fecha = DateTime.Now,
                        idCuentaCliente = models.IdCuenteClienteActiva,
                        idDetalleVenta = Convert.ToInt32(resp.data),
                        eliminada = false
                    };
                    await R_CuentaCliente_DetalleVentaControler.CRUD(models.db, relacion, 0);
                }

                AlertModerno.Success(this, "OK", resp.mensaje ?? "Producto agregado correctamente.", true, 900);
                await RecargarVentaActiva();
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task SeleccionarCuentaCliente(string parametros)
        {
            if (!MostrarBotonesComandas())
            {
                AlertModerno.Warning(this, "Atencion", "La gestion de cuentas por comanda esta desactivada en la configuracion de la base.", true, 2200);
                return;
            }

            var data = new EventArgumentParser(parametros);
            int idCuentaCliente = data.GetInt("ID");

            models.IdCuenteClienteActiva = idCuentaCliente;
            await RecargarVentaActiva();
        }

        private async Task CrearCuentaCliente(string parametros)
        {
            if (!MostrarBotonesComandas())
            {
                AlertModerno.Warning(this, "Atencion", "La gestion de cuentas por comanda esta desactivada en la configuracion de la base.", true, 2200);
                return;
            }

            try
            {
                string nombreCuenta = (parametros ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(nombreCuenta) || nombreCuenta.Length < 2)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "El nombre de la cuenta debe tener mÃƒÂ­nimo 2 caracteres.", true, 2200);
                    return;
                }

                int nuevaCuentaId = await CuentaCliente_f.Crear(models.db, models.IdCuentaActiva, nombreCuenta, Convert.ToInt32(models.Sede.porcentaje_propina));
                if (nuevaCuentaId <= 0)
                {
                    AlertModerno.Error(this, "Error", "No fue posible crear la cuenta.", true, 2200);
                    return;
                }

                models.IdCuenteClienteActiva = nuevaCuentaId;
                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", "Cuenta creada correctamente.", true, 1600);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task ActualizarCantidadDetalle(string parametros)
        {
            try
            {
                var data = new EventArgumentParser(parametros);
                int idDetalle = data.GetInt("ID");
                decimal cantidad = Convert.ToDecimal((data.GetString("CANTIDAD") ?? "0").Replace(".", ","));

                if (idDetalle <= 0 || cantidad <= 0)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No se recibiÃƒÂ³ una cantidad vÃƒÂ¡lida.", true, 1800);
                    return;
                }

                var resp = await DetalleVenta_f.ActualizarCantidadDetalle(models.db, idDetalle, cantidad);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo actualizar la cantidad.", true, 1800);
                    return;
                }

                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", resp.mensaje ?? "Cantidad actualizada correctamente.", true, 800);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task SolicitarAutorizacionDetalle(string parametros)
        {
            var data = new EventArgumentParser(parametros);
            var tipo = (data.GetString("TIPO") ?? string.Empty).Trim().ToUpperInvariant();
            var idDetalle = data.GetInt("ID");
            var valor = data.GetString("VALOR") ?? string.Empty;

            if ((tipo != "ELIMINAR" && tipo != "EDITAR_PRECIO") || idDetalle <= 0)
            {
                AlertModerno.Warning(this, "Atención", "La solicitud de autorización no es válida.", true, 1800);
                return;
            }

            var detalle = (models?.detalleCaja ?? new List<V_DetalleCaja>()).FirstOrDefault(x => x.id == idDetalle);
            if (detalle == null)
            {
                AlertModerno.Warning(this, "Atención", "No se encontró el producto solicitado.", true, 1800);
                return;
            }

            var claveSupervisorFija = (ajustes?.ClaveSupervisorCaja ?? string.Empty).Trim();
            var tieneClaveSupervisorFija = !string.IsNullOrWhiteSpace(claveSupervisorFija)
                && !string.Equals(claveSupervisorFija, "-", StringComparison.OrdinalIgnoreCase);
            var codigo = tieneClaveSupervisorFija
                ? claveSupervisorFija
                : Guid.NewGuid().ToString("N").Substring(0, 4).ToUpperInvariant();
            var accion = tipo == "ELIMINAR"
                ? $"permiso para eliminar el producto {detalle.nombreProducto}"
                : $"permiso para editar el precio del producto {detalle.nombreProducto}";
            var cajero = models?.vendedor?.nombreVendedor ?? "Cajero";

            // La solicitud debe quedar notificada y registrada incluso cuando exista
            // una clave fija. Antes se omitía todo el envío en ese caso y la pantalla
            // saltaba directamente a pedir el código del supervisor.
            var email = new EmailAutorizacionCajaHelper();
            var empresa = Convert.ToString(Session["NombreEmpresa"] ?? models?.Sede?.nombreSede ?? "SERINSIS POS");
            var tipoNotificacion = tipo == "ELIMINAR"
                ? $"Eliminar Detalle Caja - {cajero}"
                : $"Editar Precio Detalle Caja - {cajero}";
            var envio = await email.EnviarAsync(
                models.db,
                codigo,
                accion,
                detalle.nombreProducto,
                cajero,
                empresa,
                tipoNotificacion);
            if (!envio.Success)
            {
                LimpiarAutorizacionTemporal();
                AlertModerno.Error(this, "No se envió la autorización", envio.Message, true, 5000);
                return;
            }

            Session[SessionCodigoAutorizacion] = codigo;
            Session[SessionTipoAutorizacion] = tipo;
            Session[SessionDetalleAutorizacion] = idDetalle;
            Session[SessionVenceAutorizacion] = DateTime.UtcNow.AddMinutes(10);

            var titulo = tipo == "ELIMINAR" ? "Autorizar eliminación" : "Autorizar cambio de precio";
            var accionPostback = tipo == "ELIMINAR" ? "EliminarDetalle" : "EditarValorDetalle";
            var notaJs = tipo == "ELIMINAR" ? ", NOTA: ''" : string.Empty;
            var valorJs = tipo == "EDITAR_PRECIO"
                ? ", VALOR: '" + HttpUtility.JavaScriptStringEncode(valor) + "'"
                : string.Empty;
            var script = $@"
(function() {{
var autorizacionProcesada = false;
var continuarAutorizacion = function(codigo) {{
    if (autorizacionProcesada) return;
    autorizacionProcesada = true;
    EjecutarAccion('{accionPostback}', BuildArgs({{ ID: {idDetalle}{valorJs}{notaJs}, CODIGO: codigo }}), null);
}};
solicitarCodigoSupervisor('{HttpUtility.JavaScriptStringEncode(titulo)}', continuarAutorizacion);
esperarAprobacionSupervisor(continuarAutorizacion);
}})();";
            ScriptManager.RegisterStartupScript(this, GetType(), "AbrirAutorizacionDetalle", script, true);
        }

        private async Task EliminarDetalleCaja(string parametros)
        {
            try
            {
                var data = new EventArgumentParser(parametros);
                int idDetalle = data.GetInt("ID");
                string nota = data.GetString("NOTA") ?? string.Empty;
                string codigoSupervisor = data.GetString("CODIGO") ?? string.Empty;

                if (!PuedeEliminarDetalleCaja() && !CodigoSupervisorValido(codigoSupervisor, "ELIMINAR", idDetalle))
                {
                    AlertModerno.Warning(this, "Autorización requerida", "El código del supervisor no es válido.", true, 2000);
                    return;
                }

                if (idDetalle <= 0)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No se recibiÃƒÂ³ un detalle vÃƒÂ¡lido.", true, 1800);
                    return;
                }

                var resp = await DetalleVenta_f.Eliminar(models.db, idDetalle, nota);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo eliminar el producto.", true, 1800);
                    return;
                }

                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", resp.mensaje ?? "Producto eliminado correctamente.", true, 800);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task GuardarNotaDetalle(string parametros)
        {
            try
            {
                var data = new EventArgumentParser(parametros);
                int idDetalle = data.GetInt("ID");
                string nota = data.GetString("NOTA") ?? string.Empty;

                if (idDetalle <= 0)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No se recibiÃƒÂ³ un detalle vÃƒÂ¡lido.", true, 1800);
                    return;
                }

                var resp = await DetalleVenta_f.NotasDetalle(models.db, idDetalle, nota);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo actualizar la nota.", true, 1800);
                    return;
                }

                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", resp.mensaje ?? "Nota actualizada correctamente.", true, 900);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task DividirDetalle(string parametros)
        {
            try
            {
                var data = new EventArgumentParser(parametros);
                int idDetalle = data.GetInt("ID");
                int cantidadActual = data.GetInt("ACTUAL");
                int cantidadDividir = data.GetInt("DIVIDIR");

                if (idDetalle <= 0 || cantidadActual <= 1 || cantidadDividir <= 0 || cantidadDividir >= cantidadActual)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "La cantidad a dividir no es vÃƒÂ¡lida.", true, 1800);
                    return;
                }

                var resp = await DetalleVenta_f.Dividir(models.db, idDetalle, cantidadActual, cantidadDividir, models.IdCuentaActiva);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo dividir el detalle.", true, 1800);
                    return;
                }

                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", resp.mensaje ?? "Detalle dividido correctamente.", true, 900);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task AnclarDetalleCuenta(string parametros)
        {
            try
            {
                var data = new EventArgumentParser(parametros);
                int idDetalle = data.GetInt("ID");
                int idCuentaCliente = data.GetInt("CUENTA");

                if (idDetalle <= 0 || idCuentaCliente <= 0)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "Seleccione una cuenta vÃƒÂ¡lida para anclar el detalle.", true, 1800);
                    return;
                }

                var resp = await R_CuentaCliente_DetalleVenta_f.Insert(models.db, idCuentaCliente, idDetalle);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo anclar el detalle a la cuenta.", true, 1800);
                    return;
                }

                models.IdCuenteClienteActiva = idCuentaCliente;
                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", resp.mensaje ?? "Detalle anclado correctamente.", true, 900);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task EditarValorDetalle(string parametros)
        {
            try
            {
                var data = new EventArgumentParser(parametros);
                int idDetalle = data.GetInt("ID");
                decimal valor = Convert.ToDecimal((data.GetString("VALOR") ?? "0").Replace(".", ","));
                string codigoSupervisor = data.GetString("CODIGO") ?? string.Empty;

                if (idDetalle <= 0 || valor < 0)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "Ingrese un valor vÃƒÂ¡lido mayor o igual a cero.", true, 1800);
                    return;
                }

                var detalle = await DetalleVentaControler.ConsultarId(models.db, idDetalle);
                if (detalle == null)
                {
                    AlertModerno.Error(this, "Error", "No se encontrÃƒÂ³ el detalle a modificar.", true, 1800);
                    return;
                }

                var preciosDisponibles = MostrarListadoPreciosDetalle()
                    ? await V_PreciosControler.ListaPorPresentacion(models.db, detalle.idPresentacion)
                    : new List<V_Precios>();
                var tieneListaPrecios = preciosDisponibles != null && preciosDisponibles.Count > 0;

                if (tieneListaPrecios)
                {
                    var valorValido = preciosDisponibles.Any(x => x.valorPrecio == valor);
                    if (!valorValido)
                    {
                        AlertModerno.Warning(this, "Atención", "El valor seleccionado no pertenece a la lista de precios configurada.", true, 2000);
                        return;
                    }
                }
                else if (!PuedeEditarDetalleCaja() && !CodigoSupervisorValido(codigoSupervisor, "EDITAR_PRECIO", idDetalle))
                {
                    AlertModerno.Warning(this, "Autorización requerida", "El código del supervisor no es válido.", true, 2000);
                    return;
                }

                detalle.precioVenta = valor;
                var resp = await DetalleVentaControler.CRUD(models.db, detalle, 1);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo actualizar el valor.", true, 1800);
                    return;
                }

                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", resp.mensaje ?? "Valor actualizado correctamente.", true, 900);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task EditarNombreDetalle(string parametros)
        {
            try
            {
                if (!PuedeEditarDetalleCaja())
                {
                    AlertModerno.Warning(this, "Atención", "No tiene permiso para editar el producto del detalle.", true, 1800);
                    return;
                }

                var data = new EventArgumentParser(parametros);
                int idDetalle = data.GetInt("ID");
                string nombre = (data.GetString("NOMBRE") ?? string.Empty).Trim();

                if (idDetalle <= 0 || string.IsNullOrWhiteSpace(nombre))
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "Ingrese una descripciÃƒÂ³n vÃƒÂ¡lida.", true, 1800);
                    return;
                }

                var detalle = await DetalleVentaControler.ConsultarId(models.db, idDetalle);
                if (detalle == null)
                {
                    AlertModerno.Error(this, "Error", "No se encontrÃƒÂ³ el detalle a modificar.", true, 1800);
                    return;
                }

                detalle.nombreProducto = nombre;
                var resp = await DetalleVentaControler.CRUD(models.db, detalle, 1);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo actualizar el producto.", true, 1800);
                    return;
                }

                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", resp.mensaje ?? "Producto actualizado correctamente.", true, 900);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private class EditarPropinaDto
        {
            public decimal porcentaje { get; set; }
            public int propina { get; set; }
            public int idventa { get; set; }
            public int idcuenta { get; set; }
        }

        private async Task EditarPropina(string parametros)
        {
            try
            {
                if (!MostrarResumenPropina())
                {
                    AlertModerno.Warning(this, "Atencion", "La propina esta desactivada en la configuracion de la base.", true, 2200);
                    return;
                }

                if (string.IsNullOrWhiteSpace(parametros))
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No se recibiÃƒÂ³ informaciÃƒÂ³n de propina.", true, 1800);
                    return;
                }

                var dto = JsonConvert.DeserializeObject<EditarPropinaDto>(parametros);
                if (dto == null)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No se pudo interpretar la informaciÃƒÂ³n de propina.", true, 1800);
                    return;
                }

                decimal porPropina = dto.porcentaje / 100m;
                if (dto.idcuenta > 0)
                {
                    var cuenta = await CuentaClienteControler.CuentaCliente(models.db, dto.idcuenta);
                    if (cuenta == null)
                    {
                        AlertModerno.Error(this, "Error", "No se encontrÃƒÂ³ la cuenta cliente para actualizar la propina.", true, 1800);
                        return;
                    }

                    cuenta.por_propina = porPropina;
                    cuenta.propina = dto.propina;
                    var respCuenta = await CuentaClienteControler.CRUD(models.db, cuenta, 1);
                    if (!respCuenta.estado)
                    {
                        AlertModerno.Error(this, "Error", respCuenta.mensaje ?? "No se pudo actualizar la propina de la cuenta.", true, 1800);
                        return;
                    }
                }
                else
                {
                    var respuesta = await TablaVentasControler.Consultar_Id(models.db, dto.idventa);
                    if (!respuesta.estado)
                    {
                        AlertModerno.Error(this, "Error", "No se encontrÃƒÂ³ la venta para actualizar la propina.", true, 1800);
                        return;
                    }

                    var venta = respuesta.data as TablaVentas;
                    if (venta == null)
                    {
                        venta = JsonConvert.DeserializeObject<TablaVentas>(respuesta.data);
                    }
                    if (venta == null)
                    {
                        AlertModerno.Error(this, "Error", "No se encontrÃƒÂ³ la venta para actualizar la propina.", true, 1800);
                        return;
                    }

                    venta.porpropina = porPropina;
                    venta.propina = dto.propina;
                    var respVenta = await TablaVentasControler.CRUD(models.db, venta, 1);
                    if (!respVenta.estado)
                    {
                        AlertModerno.Error(this, "Error", respVenta.mensaje ?? "No se pudo actualizar la propina del servicio.", true, 1800);
                        return;
                    }
                }

                models.IdCuenteClienteActiva = dto.idcuenta;
                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", "Propina actualizada correctamente.", true, 1000);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }
        private async Task Comandar()
        {
            if (!MostrarBotonesComandas())
            {
                AlertModerno.Warning(this, "Atencion", "El servicio de comandas esta desactivado en la configuracion de la base.", true, 2200);
                return;
            }

            if (models.detalleCaja == null || !models.detalleCaja.Any())
            {
                AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No hay productos cargados para comandar.", true, 1800);
                return;
            }

            if (models.detalleCaja.Where(x => x.itemComandado == 0).ToList().Count == 0)
            {
                AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No hay items pendientes por comandar.", true, 1800);
                return;
            }

            var comanda = new ImprecionComandaAdd
            {
                id = 0,
                idVenta = models.IdCuentaActiva,
                idMesa = Convert.ToString(models.IdMesaActiva),
                idMesero = Convert.ToString(models.vendedor.id),
                estado = 1
            };

            var resp = await ImprecionComandaAddControler.CRUD(models.db, comanda, 0);
            if (resp.estado)
            {
                if (CuentaActivaEsDomicilio())
                {
                    var venta = await TablaVentasControler.ConsultarIdVenta(models.db, models.IdCuentaActiva);
                    if (venta != null)
                    {
                        venta.observacionVenta = DomicilioEstadoVentaHelper.AplicarEstado(venta.observacionVenta, "EN_PREPARACION");
                        await TablaVentasControler.CRUD(models.db, venta, 1);
                        await RecargarVentaActiva();
                    }
                }

                AlertModerno.Success(this, "OK", "Comanda enviada correctamente.", true, 1500);
            }
            else
            {
                AlertModerno.Error(this, "Error", "Comanda no enviada correctamente.", true, 1800);
            }
        }

        private async Task SolicitarCuenta()
        {
            if (!MostrarBotonesComandas())
            {
                AlertModerno.Warning(this, "Atencion", "El servicio de comandas esta desactivado en la configuracion de la base.", true, 2200);
                return;
            }

            if (models.IdCuentaActiva <= 0)
            {
                AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No hay un servicio activo para imprimir la cuenta.", true, 1800);
                return;
            }

            var cuenta = new ImprimirCuenta
            {
                id = 0,
                idVenta = models.IdCuentaActiva
            };
            PuntoDePagoPrinterHelper.Apply(cuenta, Session, models);

            var resp = await ImprimirCuentaControler.CRUD(models.db, cuenta, 0);
            if (resp.estado)
            {
                AlertModerno.Success(this, "OK", "Cuenta enviada correctamente.", true, 1500);
            }
            else
            {
                AlertModerno.Error(this, "Error", "Cuenta no enviada correctamente.", true, 1800);
            }
        }

        private async Task Cobrar()
        {
            SessionContextHelper.ApplyOperationalContext(Session, models);
            Response.Redirect("~/Cobrar.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
            await Task.CompletedTask;
        }

        private async Task CerrarCaja()
        {
            SessionContextHelper.ApplyOperationalContext(Session, models);
            Response.Redirect("~/CerrarCaja.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
            await Task.CompletedTask;
        }

        private async Task CerrarSesion()
        {
            SessionContextHelper.ApplyOperationalContext(Session, models);
            Response.Redirect("~/Salir.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
            await Task.CompletedTask;
        }

        private async Task Ventas()
        {
            SessionContextHelper.ApplyOperationalContext(Session, models);
            Response.Redirect("~/HVentas.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
            await Task.CompletedTask;
        }

        private async Task Domicilio(string eventArgument)
        {
            int idServicio = models.IdCuentaActiva;
            int idMesa = models.IdMesaActiva;

            if (!string.IsNullOrWhiteSpace(eventArgument))
            {
                var parts = eventArgument.Split('|');
                if (parts.Length > 0)
                {
                    int.TryParse(parts[0], out idMesa);
                }

                if (parts.Length > 1)
                {
                    int.TryParse(parts[1], out idServicio);
                }
            }

            if (idServicio <= 0)
            {
                AlertModerno.Error(this, "Error", "No hay un servicio activo.", true);
                return;
            }

            models.IdMesaActiva = idMesa > 0 ? idMesa : models.IdMesaActiva;
            models.IdCuentaActiva = idServicio;
            models.IdCuenteClienteActiva = 0;
            var ventaTask = V_TablaVentasControler.Consultar_Id(models.db, idServicio);
            var detalleTask = V_DetalleCajaControler.Lista_IdVenta(models.db, idServicio, 0);
            var cuentasClienteTask = V_CuentaClienteCotroler.Lista(models.db, false, idServicio);
            var ventaCuentaTask = V_CuentaClienteCotroler.Consultar(models.db, 0);
            await Task.WhenAll(ventaTask, detalleTask, cuentasClienteTask, ventaCuentaTask);

            models.venta = ventaTask.Result;
            models.detalleCaja = detalleTask.Result;
            models.v_CuentaClientes = cuentasClienteTask.Result;
            models.ventaCuenta = ventaCuentaTask.Result;
            models.clienteDomicilios = await ObtenerClienteDomiciliosAsync();
            models.AbrirModalDomicilio = true;

            if (models.venta == null || models.venta.id == 0)
            {
                AlertModerno.Error(this, "Error", "No se encontro el servicio activo.", true);
                return;
            }

            await CargarDATA();
        }

        private async Task<ClienteDomicilio> CargarClienteDomicilioActivo(int idVenta)
        {
            if (idVenta <= 0)
            {
                return new ClienteDomicilio();
            }

            var relacion = await ClienteDomicilioControler.ConsultarRelacion(models.db, idVenta);
            if (relacion == null || relacion.idClienteDomicilio == Guid.Empty)
            {
                return new ClienteDomicilio();
            }

            var lista = await ObtenerClienteDomiciliosAsync();

            return lista?.FirstOrDefault(x => x.id == relacion.idClienteDomicilio) ?? new ClienteDomicilio();
        }

        private async Task CrearActualizarClienteDomicilio(string eventArgument)
        {
            if (string.IsNullOrWhiteSpace(eventArgument))
            {
                return;
            }

            var parts = eventArgument.Split('|');
            if (parts.Length < 4)
            {
                return;
            }

            var idStr = parts[0];
            var tel = parts[1];
            var nom = parts[2];
            var dir = parts[3];

            Guid id;
            var esNuevo = string.IsNullOrWhiteSpace(idStr);
            if (esNuevo)
            {
                id = Guid.NewGuid();
            }
            else
            {
                id = new Guid(idStr);
            }

            var entidad = new ClienteDomicilio
            {
                id = id,
                celularCliente = tel,
                nombreCliente = nom,
                direccionCliente = dir
            };

            var funcion = esNuevo ? 0 : 1;
            var resp = await ClienteDomicilioControler.CRUD(models.db, entidad, funcion);
            if (!resp.estado)
            {
                AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo guardar el cliente.", true);
                return;
            }

            models.clienteDomicilios = await ObtenerClienteDomiciliosAsync(true);
            models.AbrirModalDomicilio = true;

            AlertModerno.Success(this, "OK", resp.mensaje ?? "Cliente guardado correctamente.", true, 1200);
            await CargarDATA();
        }

        private async Task SeleccionarClienteDomicilio(string eventArgument)
        {
            if (string.IsNullOrWhiteSpace(eventArgument))
            {
                return;
            }

            var parts = eventArgument.Split('|');
            if (parts.Length < 4)
            {
                return;
            }

            var idStr = parts[0];
            if (!Guid.TryParse(idStr, out Guid idCliente))
            {
                AlertModerno.Error(this, "Error", "ID de cliente invÃƒÂ¡lido.", true);
                return;
            }

            var idVenta = models.IdCuentaActiva;
            var consultarRelacion = await ClienteDomicilioControler.ConsultarRelacion(models.db, idVenta);
            var funcion = consultarRelacion == null ? 0 : 1;

            if (consultarRelacion == null)
            {
                consultarRelacion = new R_VentaClienteDomicilio
                {
                    id = 0,
                    idVenta = idVenta,
                    idClienteDomicilio = idCliente
                };
            }
            else
            {
                consultarRelacion.idClienteDomicilio = idCliente;
            }

            var resp = await ClienteDomicilioControler.RelacionarConVenta(models.db, consultarRelacion, funcion);
            if (!resp.estado)
            {
                AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo relacionar el cliente con la venta.", true);
                return;
            }

            var ventaDomicilio = await TablaVentasControler.ConsultarIdVenta(models.db, idVenta);
            if (ventaDomicilio != null && string.IsNullOrWhiteSpace(DomicilioEstadoVentaHelper.ObtenerEstado(ventaDomicilio.observacionVenta)))
            {
                ventaDomicilio.observacionVenta = DomicilioEstadoVentaHelper.AplicarEstado(
                    ventaDomicilio.observacionVenta,
                    DomicilioEstadoVentaHelper.EstadoRecibido);

                await TablaVentasControler.CRUD(models.db, ventaDomicilio, 1);
            }

            models.AbrirModalDomicilio = false;

            AlertModerno.Success(this, "OK", resp.mensaje ?? "Cliente relacionado con la venta.", true, 1200);
            await RecargarVentaActiva(false, true);

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "CerrarModalDomicilioCaja",
                "(function(){var modalEl=document.getElementById('modalDomicilio');if(modalEl&&window.bootstrap){bootstrap.Modal.getOrCreateInstance(modalEl).hide();}})();",
                true
            );
        }

        private async Task ActualizarEstadoDomicilio(string eventArgument)
        {
            var args = new EventArgumentParser(eventArgument);
            var estado = DomicilioEstadoVentaHelper.NormalizarEstado(args.GetString("ESTADO"));
            if (string.IsNullOrWhiteSpace(estado))
            {
                AlertModerno.Warning(this, "Atencion", "No se recibio un estado de domicilio valido.", true, 2200);
                return;
            }

            if (models.IdCuentaActiva <= 0 || !CuentaActivaEsDomicilio())
            {
                AlertModerno.Warning(this, "Atencion", "La cuenta activa no corresponde a un domicilio.", true, 2200);
                return;
            }

            var venta = await TablaVentasControler.ConsultarIdVenta(models.db, models.IdCuentaActiva);
            if (venta == null)
            {
                AlertModerno.Error(this, "Error", "No se encontro la venta activa del domicilio.", true);
                return;
            }

            venta.observacionVenta = DomicilioEstadoVentaHelper.AplicarEstado(venta.observacionVenta, estado);
            var resp = await TablaVentasControler.CRUD(models.db, venta, 1);
            if (!resp.estado)
            {
                AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo actualizar el estado del domicilio.", true);
                return;
            }

            await RecargarVentaActiva();
            AlertModerno.Success(this, "OK", "Estado de domicilio actualizado a " + DomicilioEstadoVentaHelper.EtiquetaEstado(estado) + ".", true, 1400);
        }

        private async Task GuardarCobroDomicilio(string eventArgument)
        {
            var args = new EventArgumentParser(eventArgument);
            var idMetodoPago = args.GetInt("IDMEDIO");
            var montoBilleteTexto = (args.GetString("MONTO") ?? "0").Replace(",", ".");
            decimal.TryParse(montoBilleteTexto, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var montoBillete);

            if (models.IdCuentaActiva <= 0 || !CuentaActivaEsDomicilio())
            {
                AlertModerno.Warning(this, "Atencion", "La cuenta activa no corresponde a un domicilio.", true, 2200);
                return;
            }

            if (idMetodoPago == 0)
            {
                AlertModerno.Warning(this, "Atencion", "Debes seleccionar el medio de pago del domicilio.", true, 2200);
                return;
            }

            var esMedioInterno = idMetodoPago < 0;
            var idMetodoBase = idMetodoPago;
            if (esMedioInterno)
            {
                idMetodoBase = models?.relMediosPagoInternos?.FirstOrDefault(x => x.idMediosDePagoInternos == Math.Abs(idMetodoPago))?.idMedioDePago ?? 0;
            }

            var metodo = idMetodoBase > 0 ? models?.metodosPago?.FirstOrDefault(x => x.id == idMetodoBase) : null;
            var nombreMetodo = metodo?.name ?? string.Empty;
            var esEfectivo = idMetodoBase == 10 || nombreMetodo.IndexOf("efect", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!esEfectivo)
            {
                montoBillete = 0m;
            }

            if (esEfectivo && montoBillete <= 0)
            {
                AlertModerno.Warning(this, "Atencion", "Indica con cuanto paga el cliente para preparar las vueltas.", true, 2200);
                return;
            }

            var venta = await TablaVentasControler.ConsultarIdVenta(models.db, models.IdCuentaActiva);
            if (venta == null)
            {
                AlertModerno.Error(this, "Error", "No se encontro la venta activa del domicilio.", true);
                return;
            }

            if (idMetodoBase > 0)
            {
                venta.idMedioDePago = idMetodoBase;
            }

            venta.observacionVenta = DomicilioEstadoVentaHelper.AplicarCobro(venta.observacionVenta, idMetodoPago, montoBillete);
            var resp = await TablaVentasControler.CRUD(models.db, venta, 1);
            if (!resp.estado)
            {
                AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo guardar la informacion de cobro del domicilio.", true);
                return;
            }

            await RecargarVentaActiva();
            AlertModerno.Success(this, "OK", "Informacion de cobro del domicilio guardada.", true, 1400);
        }

        private async Task DespacharDomicilio()
        {
            const string DomicilioPrintPrefix = "__DOMICILIO__|";

            if (models.IdCuentaActiva <= 0 || !CuentaActivaEsDomicilio())
            {
                AlertModerno.Warning(this, "Atencion", "La cuenta activa no corresponde a un domicilio.", true, 2200);
                return;
            }

            var idMetodoPago = MetodoPagoDomicilioActualId();
            var idMetodoPagoBase = MetodoPagoDomicilioBaseActualId();
            if (idMetodoPago == 0 || idMetodoPagoBase <= 0)
            {
                AlertModerno.Warning(this, "Atencion", "Debes definir como va a pagar el cliente antes de despachar.", true, 2200);
                return;
            }

            if (MetodoPagoDomicilioEsEfectivo() && MontoBilleteDomicilioActual() <= 0)
            {
                AlertModerno.Warning(this, "Atencion", "Debes indicar con que billete paga el cliente antes de despachar.", true, 2200);
                return;
            }

            var venta = await TablaVentasControler.ConsultarIdVenta(models.db, models.IdCuentaActiva);
            if (venta == null)
            {
                AlertModerno.Error(this, "Error", "No se encontro la venta activa del domicilio.", true);
                return;
            }

            venta.observacionVenta = DomicilioEstadoVentaHelper.AplicarEstado(venta.observacionVenta, "EN_CAMINO");
            var respVenta = await TablaVentasControler.CRUD(models.db, venta, 1);
            if (!respVenta.estado)
            {
                AlertModerno.Error(this, "Error", respVenta.mensaje ?? "No se pudo actualizar el despacho del domicilio.", true);
                return;
            }

            await RecargarVentaActiva();

            var impresion = new ImprimirCuenta
            {
                id = 0,
                idVenta = models.IdCuentaActiva
            };
            PuntoDePagoPrinterHelper.Apply(impresion, Session, models);

            if (string.IsNullOrWhiteSpace(impresion.namePrinter))
            {
                AlertModerno.Warning(this, "Atencion", "El domicilio quedo en camino, pero no hay una impresora configurada para enviar el ticket.", true, 2600);
                return;
            }

            impresion.namePrinter = DomicilioPrintPrefix + impresion.namePrinter.Trim();
            var respImpresion = await ImprimirCuentaControler.CRUD(models.db, impresion, 0);
            if (!respImpresion.estado)
            {
                AlertModerno.Warning(this, "Atencion", "El domicilio quedo en camino, pero no se pudo encolar la impresion para el servidor.", true, 2600);
                return;
            }

            var impresionFactura = new ImprimirFactura
            {
                id = 0,
                idventa = models.IdCuentaActiva
            };
            PuntoDePagoPrinterHelper.Apply(impresionFactura, Session, models);

            var respFactura = await ImprimirFacturaControler.CRUD(models.db, impresionFactura, 0);
            if (!respFactura.estado)
            {
                AlertModerno.Warning(this, "Atencion", "El domicilio se despacho y el ticket se envio a impresion, pero no fue posible encolar la factura.", true, 2600);
                return;
            }

            AlertModerno.Success(this, "OK", "Domicilio despachado. Se envio a imprimir el ticket del domicilio y la factura.", true, 1700);
        }

        private async Task RecargarVentaActiva(bool recargarColeccionesCompletas = false, bool recargarClienteDomicilios = false)
        {
            if (recargarColeccionesCompletas)
            {
                await ActualizarColeccionesDeCuentasAsync();
            }
            else
            {
                await ActualizarCuentaEnColeccionesAsync(models.IdCuentaActiva);
            }

            var ventaTask = V_TablaVentasControler.Consultar_Id(models.db, models.IdCuentaActiva);
            var detalleTask = V_DetalleCajaControler.Lista_IdVenta(models.db, models.IdCuentaActiva, models.IdCuenteClienteActiva);
            var cuentasClienteTask = V_CuentaClienteCotroler.Lista(models.db, false, models.IdCuentaActiva);
            var ventaCuentaTask = V_CuentaClienteCotroler.Consultar(models.db, models.IdCuenteClienteActiva);

            await Task.WhenAll(ventaTask, detalleTask, cuentasClienteTask, ventaCuentaTask);

            models.venta = ventaTask.Result;
            models.detalleCaja = detalleTask.Result;
            models.v_CuentaClientes = cuentasClienteTask.Result;
            models.ventaCuenta = ventaCuentaTask.Result;

            if (recargarClienteDomicilios)
            {
                models.clienteDomicilios = await ObtenerClienteDomiciliosAsync(true);
            }

            models.clienteDomicilioActivo = await CargarClienteDomicilioActivo(models.IdCuentaActiva);
            await CargarDATA();
        }

        private async Task SeleccionarCategoria(string parametros)
        {
            var data = new EventArgumentParser(parametros);
            int idCategoria = data.GetInt("id");

            if(idCategoria == 0)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "Id de categor\u00eda no recibido.", true, 2200);
                return;
            }

            models.IdCategoriaActiva = idCategoria;
            models.productos = models.productosLista.Where(x => x.idCategoria == idCategoria).ToList();

            await CargarDATA();
        }
    }
}





























