using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class Mesas
    {
        public int id { get; set; }
        public string nombreMesa { get; set; }
        public int estadoMesa { get; set; }
        public int idZona { get; set; }
        public Guid guidMesa { get; set; }
        public int widthMesa { get; set; }
    }
}
