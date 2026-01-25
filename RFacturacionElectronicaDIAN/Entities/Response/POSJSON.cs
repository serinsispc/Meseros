using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Response
{
    public class POSJSON
    {
        public Environment environment { get; set; }
        public  software_manufacturer software_Manufacturer { get; set; }
        public  point_sale point_Sale { get; set; }

        public class Environment
        {
            public int type_environment_id { get; set; }
            public string id { get; set; }
            public string pin { get; set; }
        }
        public class software_manufacturer
        {
            public string names_and_surnames { get; set; }
            public string business_name { get; set; }
            public string software_name { get; set; }
        }

        public class point_sale
        {
            public string box_plate { get; set; }
            public string location_box { get; set; }
            public string type_box { get; set; }
        }
    }


}
