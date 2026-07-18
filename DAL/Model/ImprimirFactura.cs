using Newtonsoft.Json;

namespace DAL.Model
{
    public class ImprimirFactura
    {
        public int id { get; set; }
        public int idventa { get; set; }

        [JsonProperty("nameprinter")]
        public string nameprinter { get; set; }

        public int ancho { get; set; }
    }
}
