using System;

namespace DAL.Helpers
{
    public static class DomicilioEstadoVentaHelper
    {
        private const string InicioMarcaEstado = "[[DOMESTADO=";
        private const string InicioMarcaPago = "[[DOMPAGO=";
        private const string InicioMarcaBillete = "[[DOMBILLETE=";
        private const string FinMarca = "]]";
        public const string EstadoRecibido = "RECIBIDO";

        public static string ObtenerEstado(string observacionVenta)
        {
            return ObtenerValorMarca(observacionVenta, InicioMarcaEstado).ToUpperInvariant();
        }

        public static int ObtenerMetodoPagoId(string observacionVenta)
        {
            var valor = ObtenerValorMarca(observacionVenta, InicioMarcaPago);
            return int.TryParse(valor, out var id) ? id : 0;
        }

        public static decimal ObtenerMontoBillete(string observacionVenta)
        {
            var valor = ObtenerValorMarca(observacionVenta, InicioMarcaBillete);
            return decimal.TryParse(valor, out var monto) ? monto : 0m;
        }

        public static string LimpiarObservacionVisible(string observacionVenta)
        {
            var texto = observacionVenta ?? string.Empty;
            var limpio = LimpiarMarca(texto, InicioMarcaEstado);
            limpio = LimpiarMarca(limpio, InicioMarcaPago);
            limpio = LimpiarMarca(limpio, InicioMarcaBillete);
            return NormalizarVacio(limpio);
        }

        public static string AplicarEstado(string observacionVenta, string estado)
        {
            var estadoNormalizado = NormalizarEstado(estado);
            if (string.IsNullOrWhiteSpace(estadoNormalizado))
            {
                return observacionVenta ?? string.Empty;
            }

            return AplicarMarca(observacionVenta, InicioMarcaEstado, estadoNormalizado);
        }

        public static string AplicarCobro(string observacionVenta, int idMetodoPago, decimal montoBillete)
        {
            var actual = observacionVenta ?? string.Empty;
            actual = AplicarMarca(actual, InicioMarcaPago, idMetodoPago != 0 ? idMetodoPago.ToString() : string.Empty);

            if (montoBillete > 0)
            {
                actual = AplicarMarca(actual, InicioMarcaBillete, decimal.Round(montoBillete, 0).ToString("0"));
            }
            else
            {
                actual = AplicarMarca(actual, InicioMarcaBillete, string.Empty);
            }

            return actual;
        }

        public static string NormalizarEstado(string estado)
        {
            var valor = (estado ?? string.Empty).Trim().ToUpperInvariant();
            switch (valor)
            {
                case "RECIBIDO":
                case "EN_PREPARACION":
                case "LISTO_PARA_DESPACHO":
                case "EN_CAMINO":
                case "ENTREGADO":
                case "NOVEDAD":
                    return valor;
                default:
                    return string.Empty;
            }
        }

        public static string EtiquetaEstado(string estado)
        {
            switch (NormalizarEstado(estado))
            {
                case "RECIBIDO":
                    return "Recibido";
                case "EN_PREPARACION":
                    return "En preparacion";
                case "LISTO_PARA_DESPACHO":
                    return "Listo para despacho";
                case "EN_CAMINO":
                    return "En camino";
                case "ENTREGADO":
                    return "Entregado";
                case "NOVEDAD":
                    return "Novedad";
                default:
                    return "Sin estado";
            }
        }

        public static bool EsTextoVacioVisible(string texto)
        {
            var valor = NormalizarVacio(texto);
            return string.IsNullOrWhiteSpace(valor);
        }

        private static string NormalizarVacio(string texto)
        {
            var valor = (texto ?? string.Empty).Trim();
            if (string.Equals(valor, "-", StringComparison.OrdinalIgnoreCase)
                || string.Equals(valor, "--", StringComparison.OrdinalIgnoreCase)
                || string.Equals(valor, "NULL", StringComparison.OrdinalIgnoreCase)
                || string.Equals(valor, "N/A", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return valor;
        }

        private static string AplicarMarca(string observacionVenta, string inicioMarca, string valor)
        {
            var visible = LimpiarMarca(observacionVenta ?? string.Empty, inicioMarca);
            visible = NormalizarVacio(visible);

            if (string.IsNullOrWhiteSpace(valor))
            {
                return visible;
            }

            var marca = inicioMarca + valor.Trim() + FinMarca;
            if (EsTextoVacioVisible(visible))
            {
                return marca;
            }

            return (visible + " " + marca).Trim();
        }

        private static string ObtenerValorMarca(string observacionVenta, string inicioMarca)
        {
            var texto = observacionVenta ?? string.Empty;
            var inicio = texto.IndexOf(inicioMarca, StringComparison.OrdinalIgnoreCase);
            if (inicio < 0)
            {
                return string.Empty;
            }

            inicio += inicioMarca.Length;
            var fin = texto.IndexOf(FinMarca, inicio, StringComparison.OrdinalIgnoreCase);
            if (fin < 0)
            {
                return string.Empty;
            }

            return (texto.Substring(inicio, fin - inicio) ?? string.Empty).Trim();
        }

        private static string LimpiarMarca(string texto, string inicioMarca)
        {
            var actual = texto ?? string.Empty;
            while (true)
            {
                var inicio = actual.IndexOf(inicioMarca, StringComparison.OrdinalIgnoreCase);
                if (inicio < 0)
                {
                    break;
                }

                var fin = actual.IndexOf(FinMarca, inicio, StringComparison.OrdinalIgnoreCase);
                if (fin < 0)
                {
                    break;
                }

                actual = (actual.Substring(0, inicio) + " " + actual.Substring(fin + FinMarca.Length)).Trim();
            }

            return string.Join(" ", actual.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
