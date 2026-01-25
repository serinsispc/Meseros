using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Request
{
    public class NotaDebitoRequest :Acceso
    {
        public object zip_key { get; set; }
        public string testSetID { get; set; }
        public bool sync { get; set; }
        public int number { get; set; }
        public int type_document_id { get; set; }

        //public Discrepancy_response discrepancy_response { get; set; }
        //public Billing_reference billing_reference { get; set; }
        //public Customer_Nota customer { get; set; }
        public RequestedMonetaryTotals requested_monetary_totals { get; set; }
        public List<DeditNoteLines> debit_note_lines { get; set; }

        public class DiscrepancyResponse
        {
            /// <summary>
            /// Tipo de Nota Credito
            /// </summary>
            public int correction_concept_id { get; set; }
        }
        public class BillingReference
        {
            public string number { get; set; }
            public string uuid { get; set; }
            public string issue_date { get; set; }
        }
        public class CustomerDebit
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
            public int type_organization_id { get; set; }
            public int type_document_identification_id { get; set; }
            public string trade_name { get; set; }
        }
        public class RequestedMonetaryTotals
        {
            public string line_extension_amount { get; set; }
            public string tax_exclusive_amount { get; set; }
            public string tax_inclusive_amount { get; set; }
            public string payable_amount { get; set; }
        }
        public class DeditNoteLines
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
        public class Tax_Totals
        {
            public int tax_id { get; set; }
            public string tax_amount { get; set; }
            public string taxable_amount { get; set; }
            public string percent { get; set; }
            public int? unit_measure_id { get; set; }
            public string per_unit_amount { get; set; }
            public string base_unit_measure { get; set; }
        }
    }

}
