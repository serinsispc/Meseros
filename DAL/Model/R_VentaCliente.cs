using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class R_VentaCliente
    {
        public int id { get; set; }
        public int idVenta { get; set; }
        public int idCliente { get; set; }
        public int idSede { get; set; }
    }
}
