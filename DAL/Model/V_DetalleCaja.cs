using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class V_DetalleCaja
    {
        public int id { get; set; } = 0;
        public Guid guidDetalle { get; set; } = Guid.Empty;
        public int idVenta { get; set; } = 0;
        public int idPresentacion { get; set; } = 0;
        public string codigoProducto { get; set; } = string.Empty;
        public string nombreProducto { get; set; } = string.Empty;
        public int impuesto_id { get; set; } = 0;
        public string presentacion { get; set; } = string.Empty;
        public decimal unidad { get; set; } = decimal.Zero;
        public decimal descuentoDetalle { get; set; }= decimal.Zero;
        public decimal preVentaNeto { get; set; } = decimal.Zero;
        public decimal precioVenta { get; set; } = decimal.Zero;
        public decimal porImpuesto { get; set; } = decimal.Zero;
        public decimal baseImpuesto { get; set; } = decimal.Zero;
        public decimal valorImpuesto { get; set; } = decimal.Zero;
        public decimal subTotalDetalleNeto { get; set; } = decimal.Zero;
        public decimal subTotalDetalle { get; set; } = decimal.Zero;
        public decimal totalDetalle { get; set; } = decimal.Zero;
        public decimal costoUnidad { get; set; } = decimal.Zero;
        public decimal contenido { get; set; } = decimal.Zero;
        public decimal costoTotal { get; set; } = decimal.Zero;
        public string observacion { get; set; }= string.Empty;
        public string opciones { get; set; }=string.Empty;
        public string adiciones { get; set; }= string.Empty;
        public int estadoDetalle { get; set; }= int.MaxValue;
        public int idCategoria { get; set; }=int.MaxValue;
        public int idCuentaCliente { get; set; } = int.MaxValue;
        public string nombreCuenta { get; set; } = string.Empty;
        public int itemComandado { get; set; } = int.MaxValue;
    }
}
