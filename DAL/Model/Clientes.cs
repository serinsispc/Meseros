using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class Clientes
    {
        public int id { get; set; }

        public int typeDocumentIdentification_id { get; set; }
        public int typeOrganization_id { get; set; }
        public int municipality_id { get; set; }
        public int typeRegime_id { get; set; }
        public int typeLiability_id { get; set; }
        public int typeTaxDetail_id { get; set; }

        public string nameCliente { get; set; }
        public string tradeName { get; set; }
        public string phone { get; set; }
        public string adress { get; set; }
        public string email { get; set; }
        public string merchantRegistration { get; set; }
        public string identificationNumber { get; set; }

        public int idTipoTercero { get; set; }
    }
}
