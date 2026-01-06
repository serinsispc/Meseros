using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class DetalleVenta
    {
        public int id { get; set; }
        public int idVenta { get; set; }
        public int idPresentacion { get; set; }
        public string nombreProducto { get; set; }
        public decimal costoUnidad { get; set; }
        public decimal precioVenta { get; set; }
        public int estadoDetalle { get; set; }
        public decimal ivaDetalle { get; set; }
        public decimal cantidadDetalle { get; set; }
        public string codigoProducto { get; set; }
        public string observacion { get; set; }
        public Guid guidDetalle { get; set; }
        public string opciones { get; set; }
        public string adiciones { get; set; }
        public int impuesto_id { get; set; }
    }
}
