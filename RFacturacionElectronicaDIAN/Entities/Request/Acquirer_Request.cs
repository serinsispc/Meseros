using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Request
{
    public class Acquirer_Request
    {
        public Environment environment { get; set; }
        public int type_document_identification_id { get; set; }
        public int identification_number { get; set; }
        public class Environment
        {
            public int type_environment_id { get; set; }
        }
    }
}
