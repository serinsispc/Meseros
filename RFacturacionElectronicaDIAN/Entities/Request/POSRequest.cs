using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RFacturacionElectronicaDIAN.Entities.Request.DocumentoSoportelRequest;

namespace RFacturacionElectronicaDIAN.Entities.Request
{
    public class POSRequest : Acceso
    {
        public string testSetID { get; set; }
        public bool sync { get; set; }
        public int number { get; set; }
        public int? resolution_id { get; set; }
        public ResolutionRequest resolution { get; set; }
        public Software_Manufacturer software_manufacturer { get; set; }
        public Buyer_Benefit buyer_benefit { get; set; }
        public Point_Sale point_sale { get; set; }
        public List<Notas_POS> notes { get; set; }
        public int type_document_id { get; set; }
        public string due_date { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public Customer_POS customer { get; set; }
        public List<PaymentForms_POS> payment_forms { get; set; }
        public List<AllowanceCharge_POS> allowance_charges { get; set; }
        public LegalMonetaryTotals_POS legal_monetary_totals { get; set; }
        public List<InvoiceLine_POS> invoice_lines { get; set; }
        public TaxTotal_POS tax_totals { get; set; }
        public Environment environment { get; set; }
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
        public int points { get; set;}
    }
    public class Point_Sale
    {
        public string box_plate { get; set; }
        public string location_box { get; set; }
        public string cashier { get; set; }
        public string type_box { get; set; }
        public string sale_code { get; set; }
        public int subtotal { get;  set; }
    }
    public class ResolutionRequest
    {
        public string prefix { get; set; }
        public string resolution { get; set; }
        public string resolution_date { get; set; }
        public string technical_key { get; set; }
        public int from { get; set; }
        public int to { get; set; }
        public string date_from { get; set; }
        public string date_to { get; set; }
    }
    public class Environment
    {
        public int type_environment_id { get; set; }
        public string id { get; set; }
        public string pin { get; set; }
    } 
}
