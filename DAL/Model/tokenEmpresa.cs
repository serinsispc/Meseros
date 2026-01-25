using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class tokenEmpresa
    {
        public int? id { get; set; }
        public Guid? idEmpresa { get; set; }
        public string token { get; set; }
        public DateTime? fechaCreacion { get; set; }
    }
}
