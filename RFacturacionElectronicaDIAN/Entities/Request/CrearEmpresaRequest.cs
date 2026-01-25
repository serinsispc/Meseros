using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFacturacionElectronicaDIAN.Entities.Request
{
    public class CrearEmpresaRequest : Acceso
    {
        public int language_id { get; set; } = 79; //Spanish; Castilian
        public int type_environment_id { get; set; } = 2; //tipo de hambiente 1-Producción; 2-Pruebas
        public int type_document_identification_id { get; set; }//tipo de documento de identidad
        public int country_id { get; set; } = 46; //Colombia
        public int type_currency_id { get; set; }=35;//Peso colombiano
        public int type_organization_id { get; set; }//1-Persona Jurídica;2-Persona Natural
        public int type_regime_id { get; set; } = 2;//1-Impuesto sobre las ventas – IVA;2-No responsable de IVA
        public int type_liability_id { get; set; } = 29;//no aplica
        public int tax_detail_id { get; set; } = 5;//No aplica
        public string business_name { get; set; }//Nombre o Razón Social
        public string trade_name { get; set; }//Nombre comercial
        public string merchant_registration { get; set; }//Matrícula mercantil
        public int municipality_id { get; set; } = 604;//municipio
        public string address {  get; set; }//Dirección
        public string phone { get; set; }//Teléfono
        public string email { get; set; }//Correo electrónico
        public string ciius { get; set; }//Corresponde al código de actividad económica CIIU 
        public int decimals { get; set; } = 2;//Decimales (0-6)

    }
}
