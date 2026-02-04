using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class MediosDePagoInternos
    {
        public int id { get; set; }
        public string nombreMPI { get; set; }
        public int estado { get; set; }
        public int reporteDIAN { get; set; }
    }
}
