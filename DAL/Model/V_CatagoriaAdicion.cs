using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class V_CatagoriaAdicion
    {
        public int id { get; set; }
        public int idCategoria { get; set; }
        public int idAdicion { get; set; }
        public string nombreCategoria { get; set; }
        public string nombreAdicion { get; set; }
        public int estado { get; set; }
    }
}
