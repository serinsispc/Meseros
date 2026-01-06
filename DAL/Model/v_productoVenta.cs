using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class v_productoVenta
    {
        public int id { get; set; }
        public int idTipoMedida { get; set; }
        public int idCategoria { get; set; }
        public int impuesto_id { get; set; }
        public string nombreCategoria { get; set; }
        public int visible { get; set; }
        public Guid guidProducto { get; set; }
        public string codigoProducto { get; set; }
        public string nombreProducto { get; set; }
        public string descripcionProducto { get; set; }
        public string letraTipoMedida { get; set; }
        public string nombreTipoPresentacion { get; set; }
        public decimal costo_mas_impuesto { get; set; }
        public decimal porcentaje_impuesto { get; set; }
        public decimal precioVenta { get; set; }
        public decimal inventarioActual { get; set; }
        public int idPresentacion { get; set; }
        public int idTipoPresentacion { get; set; }
        public int insumo { get; set; }
        public decimal contenidoPresentacion { get; set; }
        public string estadoInventario { get; set; }
        public int estadoProducto { get; set; }
        public int estadoPresentacion { get; set; }
        public int gramera { get; set; }
        public decimal inventarioMinimo { get; set; }
        public decimal inventarioInicial { get; set; }
    }
}
