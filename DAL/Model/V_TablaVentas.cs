using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class V_TablaVentas
    {
        public int id { get; set; } = 0;
        public DateTime fechaVenta { get; set; }= DateTime.Now;
        public string aliasVenta { get; set; } = "";
        public string tipoFactura { get; set; } = string.Empty;
        public string prefijo { get; set; }= string.Empty;
        public int numeroVenta { get; set; }= 0;
        public decimal descuentoVenta { get; set; }= decimal.Zero;
        public int idMedioDePago { get; set; } = 0;
        public int idResolucion { get; set; } = 0;
        public int idFormaDePago { get; set; } = 0;
        public decimal subtotalVenta { get; set; } = decimal.Zero;  
        public decimal basesIva { get; set; } = decimal.Zero;
        public decimal basesIva_5 { get; set; } = decimal.Zero;
        public decimal basesIva_19 { get; set; } = decimal.Zero;
        public decimal IVA { get; set; } = decimal.Zero;
        public decimal IVA_5 { get; set; } = decimal.Zero;
        public decimal IVA_19 { get; set; } = decimal.Zero;
        public decimal INC { get; set; } = decimal.Zero;
        public decimal INCBolsas { get; set; } = decimal.Zero;
        public decimal otrosImpuestos { get; set; } = decimal.Zero;
        public decimal ivaVenta { get; set; } = decimal.Zero;
        public decimal totalVenta { get; set; } = decimal.Zero;
        public decimal total_A_Pagar { get; set; } = decimal.Zero;
        public decimal efectivoVenta { get; set; } = decimal.Zero;
        public decimal cambioVenta { get; set; } = decimal.Zero;
        public string formaDePago { get; set; } = string.Empty;
        public decimal abonoEfectivo { get; set; } = decimal.Zero;
        public decimal abonoTarjeta { get; set; } = decimal.Zero;
        public decimal totalPagadoVenta { get; set; } = decimal.Zero;
        public decimal totalPendienteVenta { get; set; } = decimal.Zero;
        public string estadoVenta { get; set; }=string.Empty;
        public string medioDePago { get; set; } = string.Empty;
        public string numeroReferenciaPago { get; set; } = string.Empty;
        public int diasCredito { get; set; }=int.MaxValue;
        public DateTime fechaVencimiento { get; set; }=DateTime.Now;
        public string observacionVenta { get; set; } = string.Empty;
        public int IdSede { get; set; } = 0;
        public Guid guidVenta { get; set; }=Guid.Empty;
        public decimal costoTotalVenta { get; set; }=decimal.Zero;
        public decimal utilidadTotalVenta { get; set; } = decimal.Zero;
        public int idCliente { get; set; } = 0;
        public string nit { get; set; } = string.Empty;
        public string nombreCliente { get; set; } = string.Empty;
        public decimal propina { get; set; } = decimal.Zero;
        public string cufe { get; set; } = string.Empty;
        public string estadoFE { get; set; } = string.Empty;
        public string imagenQR { get; set; } = string.Empty;
        public int idBaseCaja { get; set; } = 0;
        public string razonDescuento { get; set; } = string.Empty;
        public decimal por_propina { get; set; } = decimal.Zero;
        public bool eliminada { get; set; } = false;
    }
}
