using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class V_CuentasVenta
    {
        public int id { get; set; }
        public string aliasVenta { get; set; }
        public decimal efectivoVenta { get; set; }
        public int numeroVenta { get; set; }
        public bool eliminada { get; set; }
        public decimal total { get; set; }
        public int idbase { get; set; }
        public int idusuario { get; set; }
        public string numbreUnuario { get; set; }
        public int idvendedor { get; set; }
        public string nombrevendedor { get; set; }
        public int idcliente { get; set; }
        public string nombrecliente { get; set; }
        public int idVehiculo { get; set; }
        public string placa { get; set; }
        public string responsable { get; set; }
        public string telefonoResponsable { get; set; }
        public string nombremesa { get; set; }
        public string nombreCD { get; set; }
    }
}
