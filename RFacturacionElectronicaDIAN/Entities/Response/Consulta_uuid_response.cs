using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Response
{
    public class Consulta_uuid_response
    {
        public bool is_valid { get; set; }
        public bool is_restored { get; set; }
        public string algorithm { get; set; }
        public string number { get; set; }
        public string uuid { get; set; }
        public DateTime issue_date { get; set; }
        public DateTime expedition_date { get; set; }
        public string status_code { get; set; }
        public string status_description { get; set; }
        public string status_message { get; set; }
        public List<string> errors_messages { get; set; }
        public string xml_name { get; set; }
        public string zip_name { get; set; }
        public string signature { get; set; }
        public string qr_code { get; set; }
        public string qr_data { get; set; }
        public string qr_link { get; set; }
        public string pdf_download_link { get; set; }
        public string xml_base64_bytes { get; set; }
        public string application_response_base64_bytes { get; set; }
        public string attached_document_base64_bytes { get; set; }
        public string pdf_base64_bytes { get; set; }
        public string zip_base64_bytes { get; set; }
        public int type_environment_id { get; set; }
        public Payload payload { get; set; }
        public class Customer
        {
            public string identification_number { get; set; }
            public string name { get; set; }
            public string phone { get; set; }
            public int municipality_id { get; set; }
            public string address { get; set; }
            public string email { get; set; }
            public string merchant_registration { get; set; }
            public int type_organization_id { get; set; }
            public int type_document_identification_id { get; set; }

        }
        public class Legal_monetary_totals
        {
            public string line_extension_amount { get; set; }
            public string tax_exclusive_amount { get; set; }
            public string tax_inclusive_amount { get; set; }
            public string payable_amount { get; set; }

        }
        public class Payment_forms
        {
            public int payment_form_id { get; set; }
            public int payment_method_id { get; set; }
            public string payment_due_date { get; set; }
            public int duration_measure { get; set; }

        }
        public class Tax_totals
        {
            public int tax_id { get; set; }
            public string tax_amount { get; set; }
            public string taxable_amount { get; set; }
            public string percent { get; set; }

        }
        public class Invoice_lines
        {
            public int unit_measure_id { get; set; }
            public string invoiced_quantity { get; set; }
            public string line_extension_amount { get; set; }
            public List<Tax_totals> tax_totals { get; set; }
            public string description { get; set; }
            public string code { get; set; }
            public int type_item_identification_id { get; set; }
            public string price_amount { get; set; }
            public string base_quantity { get; set; }
            public List<object> reference_price_id { get; set; }
            public List<object> free_of_charge_indicator { get; set; }

        }
        public class Payload
        {
            public string date { get; set; }
            public string time { get; set; }
            public int number { get; set; }
            public bool sync { get; set; }
            public int type_document_id { get; set; }
            public int resolution_id { get; set; }
            public Customer customer { get; set; }
            public Legal_monetary_totals legal_monetary_totals { get; set; }
            public List<Payment_forms> payment_forms { get; set; }
            public List<Invoice_lines> invoice_lines { get; set; }
            public string token { get; set; }

        }
    }
}
