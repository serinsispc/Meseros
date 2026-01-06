using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class Vendedor
    {
        public int id { get; set; }
        public string nombreVendedor { get; set; }
        public string telefonoVendedor { get; set; }
        public string calveVendedor { get; set; } // (claveVendedor si luego corriges el nombre en BD)
        public int cajaMovil { get; set; }
    }
}
