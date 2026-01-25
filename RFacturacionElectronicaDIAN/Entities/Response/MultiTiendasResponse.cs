using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Response
{
    public partial class MultiTiendasResponse
    {
        public class ListaTiendas
        {
            public int id { get; set; }
            public string name { get; set; } 
            public string db_tienda { get; set; }
            public string servidor { get; set; }
        }
        public List<ListaTiendas> lista_tiendas { get; set; }
    }
}
