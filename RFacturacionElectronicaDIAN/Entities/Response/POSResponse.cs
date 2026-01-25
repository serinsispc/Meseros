using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Response
{
    public class POSResponse
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

    public class Customer_POS
    {
        /// <summary>
        /// Documento del Cliente
        /// </summary>
        public string identification_number { get; set; }
        /// <summary>
        /// Nombres y Apellidos del Cliente
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Telefono del Cliente O Razon social del empresa
        /// </summary>
        public string phone { get; set; }
        /// <summary>
        /// ID Municipio Residencia del Cliente
        /// </summary>
        public int municipality_id { get; set; }
        /// <summary>
        /// Dirección del Cliente
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// Correo Electronico del Cliente, Si desea reportar varios emails usar ; como separador
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// Registro mercantil del Cliente por Default No tiene
        /// </summary>
        public string merchant_registration { get; set; }
        /// <summary>
        /// Código del tipo organización (Identificador de tipo de organización jurídica de la de persona)
        /// </summary>
        public int type_organization_id { get; set; }
        public int type_document_identification_id { get; set; }
        public string trade_name { get; set; }
    }
    public class AllowanceCharge_POS
    {
        public bool charge_indicator { get; set; }
        public int discount_id { get; set; }
        public string allowance_charge_reason { get; set; }
        public string amount { get; set; }
        public string base_amount { get; set; }
    }
    public class LegalMonetaryTotals_POS
    {
        public string line_extension_amount { get; set; }
        public string tax_exclusive_amount { get; set; }
        public string tax_inclusive_amount { get; set; }
        public string payable_amount { get; set; }
    }
    public class TaxTotal_POS
    {
        public int tax_id { get; set; }
        public string tax_amount { get; set; }
        public string taxable_amount { get; set; }
        public string percent { get; set; }
        public int? unit_measure_id { get; set; }
        public string per_unit_amount { get; set; }
        public string base_unit_measure { get; set; }
    }
    public class InvoiceLine_POS
    {
        public int unit_measure_id { get; set; }
        public string invoiced_quantity { get; set; }
        public string line_extension_amount { get; set; }
        public List<AllowanceCharge> allowance_charges { get; set; }
        public List<TaxTotal> tax_totals { get; set; }
        public string description { get; set; }
        public string code { get; set; }
        public int type_item_identification_id { get; set; }
        public string price_amount { get; set; }
        public string base_quantity { get; set; }
        public int? reference_price_id { get; set; }
        public bool? free_of_charge_indicator { get; set; }
    }
    public class Notas_POS
    {
        public string text { get; set; }
    }
    public class PaymentForms_POS
    {
        public int payment_form_id { get; set; }
        public int payment_method_id { get; set; }
        public string payment_due_date { get; set; }
        public int duration_measure { get; set; }
    }
    public class Software_Manufacturer
    {
        public string names_and_surnames { get; set; }
        public string business_name { get; set; }
        public string software_name { get; set; }
    }
    public class Buyer_Benefit
    {
        public string code { get; set; }
        public string names_and_surnames { get; set; }
        public int points { get; set; }
    }
    public class Point_Sale
    {
        public string box_plate { get; set; }
        public string location_box { get; set; }
        public string cashier { get; set; }
        public string type_box { get; set; }
        public string sale_code { get; set; }
        public string subtotal { get; set; }
    }
}
