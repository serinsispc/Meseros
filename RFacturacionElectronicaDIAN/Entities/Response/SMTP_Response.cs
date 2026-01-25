using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Response
{
    public class SMTP_Response
    {
        public string message { get; set; }
        public smtp smtp { get; set; }
    }
    public class smtp
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public string host { get; set; }
        public int port { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string encryption { get; set; }
        public string from_address { get; set; }
        public string from_name { get; set; }
        public string updated_at { get; set; }
        public string created_at { get; set; }
    }
}
