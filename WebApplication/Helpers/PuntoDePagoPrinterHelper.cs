using DAL.Controler;
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.SessionState;
using WebApplication.ViewModels;

namespace WebApplication.Helpers
{
    public static class PuntoDePagoPrinterHelper
    {
        private const int DefaultPrinterWidth = 58;

        private static int NormalizePrinterWidth(int width)
        {
            return width >= 80 ? 80 : 58;
        }

        private static MenuViewModels ResolveModel(HttpSessionState session, MenuViewModels model = null)
        {
            return model ?? SessionContextHelper.LoadModels(session) ?? session?[SessionContextHelper.ModelsKey] as MenuViewModels ?? new MenuViewModels();
        }

        private static void EnsureSelectedPuntoDePago(HttpSessionState session, MenuViewModels resolvedModel)
        {
            if (resolvedModel == null)
            {
                return;
            }

            var puntoSesion = session?[SessionContextHelper.PuntoDePagoKey] as string;
            if ((resolvedModel.PuntoDePagoSeleccionado == null || resolvedModel.PuntoDePagoSeleccionado.id <= 0) && !string.IsNullOrWhiteSpace(puntoSesion))
            {
                try
                {
                    resolvedModel.PuntoDePagoSeleccionado = JsonConvert.DeserializeObject<PuntosDePago>(puntoSesion) ?? new PuntosDePago();
                }
                catch
                {
                    resolvedModel.PuntoDePagoSeleccionado = resolvedModel.PuntoDePagoSeleccionado ?? new PuntosDePago();
                }
            }

            var idPuntoDePago = resolvedModel.PuntoDePagoSeleccionado?.id ?? 0;
            if (idPuntoDePago <= 0 && session?[SessionContextHelper.IdPuntoDePagoKey] != null)
            {
                int.TryParse(Convert.ToString(session[SessionContextHelper.IdPuntoDePagoKey]), out idPuntoDePago);
            }

            if (idPuntoDePago > 0 && resolvedModel.puntosDePago != null && resolvedModel.puntosDePago.Any())
            {
                var puntoDesdeLista = resolvedModel.puntosDePago.FirstOrDefault(x => x != null && x.id == idPuntoDePago);
                if (puntoDesdeLista != null)
                {
                    if (resolvedModel.PuntoDePagoSeleccionado == null || resolvedModel.PuntoDePagoSeleccionado.id <= 0)
                    {
                        resolvedModel.PuntoDePagoSeleccionado = puntoDesdeLista;
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(resolvedModel.PuntoDePagoSeleccionado.impresoraPredeterminada))
                    {
                        resolvedModel.PuntoDePagoSeleccionado.impresoraPredeterminada = puntoDesdeLista.impresoraPredeterminada;
                    }

                    if (resolvedModel.PuntoDePagoSeleccionado.ancho <= 0)
                    {
                        resolvedModel.PuntoDePagoSeleccionado.ancho = puntoDesdeLista.ancho;
                    }
                }
            }
        }

        public static async Task<PuntosDePago> ResolveSelectedPuntoDePagoAsync(string db, HttpSessionState session, MenuViewModels model = null)
        {
            var resolvedModel = ResolveModel(session, model);
            EnsureSelectedPuntoDePago(session, resolvedModel);

            var selected = resolvedModel?.PuntoDePagoSeleccionado;
            var selectedId = selected?.id ?? 0;
            if (selectedId <= 0 || string.IsNullOrWhiteSpace(db))
            {
                return selected;
            }

            var needsHydration = string.IsNullOrWhiteSpace(selected.impresoraPredeterminada) || selected.ancho <= 0;
            if (!needsHydration)
            {
                return selected;
            }

            var puntoDesdeDb = await PuntosDePagoControler.Consultar(db, selectedId);
            if (puntoDesdeDb == null)
            {
                return selected;
            }

            resolvedModel.PuntoDePagoSeleccionado = puntoDesdeDb;

            if (resolvedModel.puntosDePago != null)
            {
                var index = resolvedModel.puntosDePago.FindIndex(x => x != null && x.id == puntoDesdeDb.id);
                if (index >= 0)
                {
                    resolvedModel.puntosDePago[index] = puntoDesdeDb;
                }
            }

            SessionContextHelper.ApplyOperationalContext(session, resolvedModel);
            return puntoDesdeDb;
        }

        public static string ResolvePrinterName(HttpSessionState session, MenuViewModels model = null)
        {
            var resolvedModel = ResolveModel(session, model);
            EnsureSelectedPuntoDePago(session, resolvedModel);

            var printerName = (resolvedModel.PuntoDePagoSeleccionado?.impresoraPredeterminada ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(printerName))
            {
                return printerName;
            }

            if (!string.IsNullOrWhiteSpace(resolvedModel.Sede?.impresora))
            {
                return resolvedModel.Sede.impresora.Trim();
            }

            if (!string.IsNullOrWhiteSpace(resolvedModel.Sede?.nombre_impresora))
            {
                return resolvedModel.Sede.nombre_impresora.Trim();
            }

            if (!string.IsNullOrWhiteSpace(resolvedModel.Sede?.impresora2))
            {
                return resolvedModel.Sede.impresora2.Trim();
            }

            return string.Empty;
        }

        public static int ResolvePrinterWidth(HttpSessionState session, MenuViewModels model = null)
        {
            var resolvedModel = ResolveModel(session, model);
            EnsureSelectedPuntoDePago(session, resolvedModel);

            if (resolvedModel.PuntoDePagoSeleccionado != null && resolvedModel.PuntoDePagoSeleccionado.ancho > 0)
            {
                return NormalizePrinterWidth(resolvedModel.PuntoDePagoSeleccionado.ancho);
            }

            if (resolvedModel.Sede != null && resolvedModel.Sede.tamanoPapel > 0)
            {
                return NormalizePrinterWidth(resolvedModel.Sede.tamanoPapel);
            }

            return NormalizePrinterWidth(DefaultPrinterWidth);
        }

        public static void Apply(ImprimirCuenta solicitud, HttpSessionState session, MenuViewModels model = null)
        {
            if (solicitud == null)
            {
                return;
            }

            solicitud.namePrinter = ResolvePrinterName(session, model);
            solicitud.ancho = ResolvePrinterWidth(session, model);
        }

        public static void Apply(ImprimirFactura solicitud, HttpSessionState session, MenuViewModels model = null)
        {
            if (solicitud == null)
            {
                return;
            }

            solicitud.nameprinter = ResolvePrinterName(session, model);
            solicitud.ancho = ResolvePrinterWidth(session, model);
        }

        public static void Apply(AperturarCajon solicitud, HttpSessionState session, MenuViewModels model = null)
        {
            if (solicitud == null)
            {
                return;
            }

            solicitud.nameprinter = ResolvePrinterName(session, model);
            solicitud.ancho = ResolvePrinterWidth(session, model);
        }
    }
}
