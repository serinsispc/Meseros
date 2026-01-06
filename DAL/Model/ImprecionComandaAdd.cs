using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class ImprecionComandaAdd
    {
        public int id { get; set; }
        public int idVenta { get; set; }
        public string idMesa { get; set; }
        public string idMesero { get; set; }
        public int estado { get; set; }
    }
}
