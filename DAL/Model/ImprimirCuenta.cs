using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DAL.Model
{
    public class ImprimirCuenta
    {
        public int id { get; set; }
        public int idVenta { get; set; }
        [JsonProperty("nameprinter")]
        public string namePrinter { get; set; }
        public int ancho { get; set; }
    }
}
