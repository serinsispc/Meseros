using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.SessionState;
using WebApplication.ViewModels;

namespace WebApplication.Helpers
{
    public static class PuntoDePagoPrinterHelper
    {
        public static string ResolvePrinterName(HttpSessionState session, MenuViewModels model = null)
        {
            var resolvedModel = model ?? SessionContextHelper.LoadModels(session) ?? session?[SessionContextHelper.ModelsKey] as MenuViewModels;
            if (resolvedModel == null)
            {
                resolvedModel = new MenuViewModels();
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

            var idPuntoDePago = 0;
            if (resolvedModel.PuntoDePagoSeleccionado != null && resolvedModel.PuntoDePagoSeleccionado.id > 0)
            {
                idPuntoDePago = resolvedModel.PuntoDePagoSeleccionado.id;
            }
            else if (session?[SessionContextHelper.IdPuntoDePagoKey] != null)
            {
                int.TryParse(Convert.ToString(session[SessionContextHelper.IdPuntoDePagoKey]), out idPuntoDePago);
            }

            if (idPuntoDePago > 0 && (resolvedModel.PuntoDePagoSeleccionado == null || string.IsNullOrWhiteSpace(resolvedModel.PuntoDePagoSeleccionado.impresoraPredeterminada)))
            {
                var puntoDesdeLista = resolvedModel.puntosDePago?.FirstOrDefault(x => x != null && x.id == idPuntoDePago);
                if (puntoDesdeLista != null)
                {
                    resolvedModel.PuntoDePagoSeleccionado = puntoDesdeLista;
                }
            }

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

        public static void Apply(ImprimirCuenta solicitud, HttpSessionState session, MenuViewModels model = null)
        {
            if (solicitud == null)
            {
                return;
            }

            solicitud.namePrinter = ResolvePrinterName(session, model);
        }

        public static void Apply(ImprimirFactura solicitud, HttpSessionState session, MenuViewModels model = null)
        {
            if (solicitud == null)
            {
                return;
            }

            solicitud.nameprinter = ResolvePrinterName(session, model);
        }

        public static void Apply(AperturarCajon solicitud, HttpSessionState session, MenuViewModels model = null)
        {
            if (solicitud == null)
            {
                return;
            }

            solicitud.nameprinter = ResolvePrinterName(session, model);
        }
    }
}
