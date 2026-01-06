using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class BaseCaja
    {
        public int id { get; set; }
        public DateTime fechaApertura { get; set; }
        public int idUsuarioApertura { get; set; }
        public decimal valorBase { get; set; }
        public DateTime? fechaCierre { get; set; }
        public int? idUsuarioCierre { get; set; }
        public string estadoBase { get; set; }
        public int idSedeBAse { get; set; }
    }
}
