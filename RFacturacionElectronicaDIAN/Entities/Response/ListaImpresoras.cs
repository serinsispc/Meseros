using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Response
{
    public partial class ListaImpresoras
    {
        public class List_Printer
        {
            public int id { get; set; }
            public string name { get; set; }
            public string name_printer { get; set; }
            public int code { get; set; }
            public int copias { get; set; }
            public bool estado { get; set; }
        }
        public List<List_Printer> list_printer { get; set; }
    }
}
