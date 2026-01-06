using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class R_CuentaCliente_DetalleVenta
    {
        public int id { get; set; }
        public DateTime fecha { get; set; }
        public int idCuentaCliente { get; set; }
        public int idDetalleVenta { get; set; }
        public bool eliminada { get; set; }
    }
}
