using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class V_CuentaCliente
    {
        public int id { get; set; }
        public DateTime fecha { get; set; }
        public int idVenta { get; set; }
        public string nombreCuenta { get; set; }
        public bool preCuenta { get; set; }
        public bool eliminada { get; set; }
        public decimal subtotalVenta { get; set; }
        public decimal ivaVenta { get; set; }
        public decimal totalVenta { get; set; }
        public decimal por_propina { get; set; }
        public decimal propina { get; set; }
        public decimal total_A_Pagar { get; set; }
    }
}
