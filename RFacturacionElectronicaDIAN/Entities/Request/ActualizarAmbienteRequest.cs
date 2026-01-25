using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Request
{
    public partial class ActualizarAmbienteRequest
    {
        public int type_environment_id { get; set; }
        public string id { get; set; }
        public string pin { get; set; }
        public string certificate { get; set; }
        public string password { get; set; }
    }
}
