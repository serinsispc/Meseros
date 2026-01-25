using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Response
{
    public class NotaCreditoPOSResponse
    {
        public string message { get; set; }
        public int type_environment_id { get; set; }
        public bool? is_valid { get; set; }
        public object is_restored { get; set; }
        public object algorithm { get; set; }
        public string number { get; set; }
        public object uuid { get; set; }
        public object issue_date { get; set; }
        public object expedition_date { get; set; }
        public object zip_key { get; set; }
        public string status_code { get; set; }
        public string status_description { get; set; }
        public string status_message { get; set; }
        public object mail_sending_message { get; set; }
        public List<string> errors_messages { get; set; }
        public object xml_name { get; set; }
        public object zip_name { get; set; }
        public object signature { get; set; }
        public object qr_code { get; set; }
        public object qr_data { get; set; }
        public object qr_link { get; set; }
        public object pdf_download_link { get; set; }
        public object xml_base64_bytes { get; set; }
        public object application_response_base64_bytes { get; set; }
        public object attached_document_base64_bytes { get; set; }
        public object pdf_base64_bytes { get; set; }
        public object zip_base64_bytes { get; set; }
        public Payload payload { get; set; }
    }
}
