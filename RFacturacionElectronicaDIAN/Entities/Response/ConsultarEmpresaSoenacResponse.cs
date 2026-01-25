using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Response
{
    public class ConsultarEmpresaSoenacResponse
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public string identification_number { get; set; }
        public int dv { get; set; }
        public int language_id { get; set; }
        public int type_environment_id { get; set; }
        public int type_operation_id { get; set; }
        public int type_document_identification_id { get; set; }
        public int country_id { get; set; }
        public int type_currency_id { get; set; }
        public int type_organization_id { get; set; }
        public int type_regime_id { get; set; }
        public int type_liability_id { get; set; }
        public int tax_detail_id { get; set; }
        public string business_name { get; set; }
        public string trade_name { get; set; }
        public string merchant_registration { get; set; }
        public int municipality_id { get; set; }
        public string postal_zone { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string ciius { get; set; }
        public int decimals { get; set; }
        public string mailer { get; set; }=string.Empty;
        public string group { get; set; } = string.Empty; 
        public int? cut_off_day { get; set; }
        public int? document_limit { get; set; }
        public string message {  get; set; }=string.Empty;
    }
}
