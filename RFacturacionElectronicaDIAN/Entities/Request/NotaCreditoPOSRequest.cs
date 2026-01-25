using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Request
{
    public class NotaCreditoPOSRequest: Acceso
    {
        public string testSetID { get; set; }
        public bool sync { get; set; }
        public Discrepancy_response_pos_nc discrepancy_response { get; set; }
        public int number { get; set; }
        public int? resolution_id { get; set; }
        public Resolution_nc resolution { get; set; }
        public List<Notas_POS_nc> notes { get; set; }
        public int type_document_id { get; set; }
        public string due_date { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public Customer_POS_nc customer { get; set; }
        public List<PaymentForms_POS_nc> payment_forms { get; set; }
        public List<AllowanceCharge_POS_nc> allowance_charges { get; set; }
        public LegalMonetaryTotals_POS_nc legal_monetary_totals { get; set; }
        public List<Credit_note_lines_POS_nc> credit_note_lines { get; set; }
        public TaxTotal_POS_nc tax_totals { get; set; }
        public Environment_nc environment { get; set; }
        public Billing_reference_POS_nc billing_reference { get; set; }

    }
    public class Discrepancy_response_pos_nc
    {
        public int correction_concept_id { get; set; }
    }
    public class billing_reference_nc
    {
        public string number { get; set; }
        public string uuid { get; set; }
        public string issue_date { get; set; }
    }
    public class Customer_POS_nc
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
        //public string phone { get; set; }
        /// <summary>
        /// ID Municipio Residencia del Cliente
        /// </summary>
        //public int municipality_id { get; set; }
        /// <summary>
        /// Dirección del Cliente
        /// </summary>
        //public string address { get; set; }
        /// <summary>
        /// Correo Electronico del Cliente, Si desea reportar varios emails usar ; como separador
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// Registro mercantil del Cliente por Default No tiene
        /// </summary>
        //public string merchant_registration { get; set; }
        /// <summary>
        /// Código del tipo organización (Identificador de tipo de organización jurídica de la de persona)
        /// </summary>
        //public int type_organization_id { get; set; }
        public int type_document_identification_id { get; set; }
        //public string trade_name { get; set; }
    }
    public class AllowanceCharge_POS_nc
    {
        public bool charge_indicator { get; set; }
        public int discount_id { get; set; }
        public string allowance_charge_reason { get; set; }
        public string amount { get; set; }
        public string base_amount { get; set; }
    }
    public class LegalMonetaryTotals_POS_nc
    {
        public string line_extension_amount { get; set; }
        public string tax_exclusive_amount { get; set; }
        public string tax_inclusive_amount { get; set; }
        public string payable_amount { get; set; }
    }
    public class TaxTotal_POS_nc
    {
        public int tax_id { get; set; }
        public string tax_amount { get; set; }
        public string taxable_amount { get; set; }
        public string percent { get; set; }
        public int? unit_measure_id { get; set; }
        public string per_unit_amount { get; set; }
        public string base_unit_measure { get; set; }
    }
    public class Credit_note_lines_POS_nc
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
    public class Notas_POS_nc
    {
        public string text { get; set; }
    }
    public class PaymentForms_POS_nc
    {
        public int payment_form_id { get; set; }
        public int payment_method_id { get; set; }
        public string payment_due_date { get; set; }
        public int duration_measure { get; set; }
    }
    public class Resolution_nc
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
    public class Environment_nc
    {
        public int type_environment_id { get; set; }
        public string id { get; set; }
        public string pin { get; set; }
    }
    public class Billing_reference_POS_nc
    {
        public string number { get; set; }
        public string uuid { get; set; }
        public string issue_date { get; set; }
    }
}
