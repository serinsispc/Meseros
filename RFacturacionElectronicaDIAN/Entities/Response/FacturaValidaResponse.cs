using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Response
{
    public class CustomerFacturaValida
    {
        public int identification_number { get; set; }
        public string name { get; set; }
        public int phone { get; set; }
        public int municipality_id { get; set; }
        public string address { get; set; }
        public string email { get; set; }
        public string merchant_registration { get; set; }
    }

    public class AllowanceCharge
    {
        public bool charge_indicator { get; set; }
        public int discount_id { get; set; }
        public string allowance_charge_reason { get; set; }
        public string amount { get; set; }
        public string base_amount { get; set; }
    }

    public class LegalMonetaryTotalsFacturaValida
    {
        public string line_extension_amount { get; set; }
        public string tax_exclusive_amount { get; set; }
        public string tax_inclusive_amount { get; set; }
        public string payable_amount { get; set; }
    }

    public class AllowanceChargeFacturaValida
    {
        public bool charge_indicator { get; set; }
        public string allowance_charge_reason { get; set; }
        public string amount { get; set; }
        public string base_amount { get; set; }
    }

    public class TaxTotalFacturaValida
    {
        public int tax_id { get; set; }
        public string tax_amount { get; set; }
        public string taxable_amount { get; set; }
        public string percent { get; set; }
        public int? unit_measure_id { get; set; }
        public string per_unit_amount { get; set; }
        public string base_unit_measure { get; set; }
    }

    public class InvoiceLineFacturaValida
    {
        public int unit_measure_id { get; set; }
        public string invoiced_quantity { get; set; }
        public string line_extension_amount { get; set; }
        public IList<AllowanceChargeFacturaValida> allowance_charges { get; set; }
        public IList<TaxTotalFacturaValida> tax_totals { get; set; }
        public string description { get; set; }
        public string code { get; set; }
        public int type_item_identification_id { get; set; }
        public string price_amount { get; set; }
        public string base_quantity { get; set; }
        public int? reference_price_id { get; set; }
        public bool? free_of_charge_indicator { get; set; }
    }

    public class FacturaValidaResponse
    {
        public int number { get; set; }
        public bool sync { get; set; }
        public int type_document_id { get; set; }
        public int resolution_id { get; set; }
        public CustomerFacturaValida customer { get; set; }
        public IList<AllowanceChargeFacturaValida> allowance_charges { get; set; }
        public LegalMonetaryTotalsFacturaValida legal_monetary_totals { get; set; }
        public IList<InvoiceLineFacturaValida> invoice_lines { get; set; }
    }

}
