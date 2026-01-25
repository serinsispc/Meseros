using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Request
{
    public class WhatsAppRequest : AccesoMeta
    {
        public string messaging_product { get; set; }
        public string to { get; set; }
        public string type { get; set; }
        public Template template { get; set; }
    }
    public class Template
    {
        public string name { get; set; }
        public Language language { get; set; }
        public List<Components> components { get; set; }
    }
    public class Language
    {
        public string code { get; set; }
    }
    public class Components
    {
        public string type { get; set; }
        public List<Parameters> parameters { get; set; }
    }
    public class Parameters
    {
        public string type { get; set; }
        public string parameter_name { get; set; }
        public string text { get; set; }
    }
}
