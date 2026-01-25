using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Response
{
    public class WhatsAppResponse
    {
        public string messaging_product { get; set; }
        public List<Contacts> contacts { get; set; }
        public List<Messages> messages { get; set; }
    }

    public class Contacts
    {
        public string input { get; set; }
        public string wa_id { get; set; }
    }

    public class Messages
    {
        public string id { get; set; }
        public string message_status { get; set; }
    }
}
