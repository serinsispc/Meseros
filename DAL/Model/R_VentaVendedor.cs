using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class R_VentaVendedor
    {
        public int id { get; set; }
        public int idVenta { get; set; }
        public int idVendedor { get; set; }
    }
}
