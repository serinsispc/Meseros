using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Response
{
    public class ConsultarRUT_Response
    {
        public string email { get; set; } = string.Empty;
        public string state { get; set; } = string.Empty;
        public string nit { get; set; } = string.Empty;
        public string dv { get; set; } = string.Empty;
        public string business_name { get; set; } = string.Empty;
        public string first_name { get; set; } = string.Empty;
        public string other_names { get; set; } = string.Empty;
        public string surname { get; set; } = string.Empty;
        public string second_surname { get; set; } = string.Empty;
        public string ciius { get; set; } = string.Empty;
        public string initials { get; set; } = string.Empty;
        public string merchant_registration { get; set; } = string.Empty;
        public string merchant_registration_city { get; set; } = string.Empty;
        public string merchant_registration_date { get; set; } = string.Empty;
        public string merchant_registration_state { get; set; } = string.Empty;
        public string merchant_registration_renewal_date { get; set; } = string.Empty;
        public string merchant_registration_company_type { get; set; } = string.Empty;
        public string merchant_registration_effective_date { get; set; } = string.Empty;
        public string merchant_registration_last_year_renewed { get; set; } = string.Empty;
        public string merchant_registration_date_of_last_update { get; set; } = string.Empty;
        public string merchant_registration_type_of_organization { get; set; } = string.Empty;
        public string merchant_registration_registration_category { get; set; } = string.Empty;
    }
}
