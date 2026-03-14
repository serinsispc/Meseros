using DAL;
using DAL.Controler;
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
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
    }
}
