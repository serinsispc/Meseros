using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class V_Categoria
    {
        public int id { get; set; }
        public Guid guidCategoria { get; set; }
        public string nombreCategoria { get; set; }
        public int estadoCategoria { get; set; }
        public int visible { get; set; }
        public string nombreVisible { get; set; }
        public string printer { get; set; }
    }
}
