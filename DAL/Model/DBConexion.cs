using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class DBConexion
    {
        public Guid Id { get; set; }
        public string Servidor { get; set; }
        public string DBBase { get; set; }
        public string UsuarioDB { get; set; }
        public string ClaveDB { get; set; }
        public string NombreConexion { get; set; }
        public string RutaServidor { get; set; }
        public bool ComandasCaja { get; set; }
        public bool Pantalla_Tactil { get; set; }
        public bool Comandera { get; set; }
        public int IdTipoImpuesto { get; set; }
        public string TipoImpuesto { get; set; }
        public decimal ValorImpuesto { get; set; }
        public decimal Propina { get; set; }
        public decimal PorcentajePropina { get; set; }
        public bool adiciones { get; set; }
        public int idAnviente { get; set; }
        public bool FE { get; set; }
        public bool DecuentoVendedorJSON { get; set; }
        public string RutaComanda { get; set; }
        public bool ServidorImpresora { get; set; }
        public bool QRComanda { get; set; }
        public bool ConsecutivoCaja { get; set; }
        public string WhatsAppSupervisorCajas { get; set; }
        public bool EliminarDetalleCaja { get; set; }
        public bool MostrarCierreCaja { get; set; }
        public int CopiasComandaCocina { get; set; }
        public int CopiasComandaBarra { get; set; }
        public bool ImprimirDirecto { get; set; }
        public bool MostrarLogoPOS { get; set; }
        public string ClaveSupervisorCaja { get; set; }
        public bool MultiCajas { get; set; }
        public bool MantenerBuscardor { get; set; }
        public string NombreCaja { get; set; }
        public bool MultiTienda { get; set; }
        public string VentanaBuscadorCaja { get; set; }
        public bool ObservacionFactura { get; set; }
        public bool FacturaElectronicaPOS { get; set; }
        public string NombreMenuVendedor { get; set; }
        public string NombreMenuMesas { get; set; }
        public string RutaAdminCajas { get; set; }
        public bool PCAdmin { get; set; }
        public string PuertoCOM { get; set; }
        public bool ImprimirCierre { get; set; }
        public bool ImprimirProductosVendidos { get; set; }
        public bool BuscarTalla { get; set; }
        public bool CajonMonedero { get; set; }
        public decimal PrecioLT { get; set; }
        public string TipoImpresora { get; set; }
        public string TipoImpresora_Factura { get; set; }
        public string CodigoImpresora { get; set; }
        public string RutaImagen { get; set; }
        public bool DocumentoPOSElectronico { get; set; }
        public bool botontaller { get; set; }
        public bool FEH { get; set; }
        public bool FEAutomatico { get; set; }
        public string pNombreRecargo { get; set; }
        public bool Borrador_FE { get; set; }
        public bool ValorAntesDeImpuesto { get; set; }
        public bool BuscarProductoPategoria { get; set; }
        public bool CuentaSinImpusto { get; set; }
        public bool MostrarCantidadProductoEnCaja { get; set; }
        public bool meserosCompartidos { get; set; }
        public bool RutaImg { get; set; }
        public bool ListadoItenVentasUnico { get; set; }
        public bool PermisoAdmin { get; set; }
        public string URL_api { get; set; }
        public string IdNumeroWhatsApp { get; set; }
        public bool DBLocal { get; set; }
        public bool ServidorPrinterAPP { get; set; }
    }
}
