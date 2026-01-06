using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class V_Cuentas
    {
        public int id { get; set; }
        public int idVenta { get; set; }
        public string aliasVenta { get; set; }
        public bool eliminada { get; set; }
        public int idUsuario { get; set; }
        public string mesa { get; set; }
        public int idVendedor { get; set; }
        public int estado { get; set; }
    }
}
