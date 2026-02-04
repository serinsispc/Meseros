using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class V_CorreosCliente
    {
        public int? id { get; set; }
        public int? idCliente { get; set; }
        public string email { get; set; }
    }
}
