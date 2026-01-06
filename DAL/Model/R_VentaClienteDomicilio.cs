using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class R_VentaClienteDomicilio
    {
        public int id { get; set; }               // PK de la relación
        public int idVenta { get; set; }          // Id de la venta
        public Guid idClienteDomicilio { get; set; } // Id del cliente domicilio
    }
}
