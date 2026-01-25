using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Response
{
    public partial class ListaResolucionesDIAN
    {
        public class Lista_Resoculiones
        {
            public int id { get; set; }
            public int type_document_id { get; set; }
            public string prefix { get; set; }
            public string resolution { get; set; }
            public string resolution_date { get; set; }
            public string technical_key { get; set; }
            public int from { get; set; }
            public int to { get; set; }
            public string date_from { get; set; }
            public string date_to { get; set; }
            public int number { get; set; }
            public string next_consecutive { get; set; }
        }
        public List<Lista_Resoculiones> listaresoluciones { get; set; }
    }
}
