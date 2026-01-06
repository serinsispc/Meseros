using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class V_ServicioMesa
    {
        public int id { get; set; }
        public DateTime fecha { get; set; }
        public string aliasServicio { get; set; }
        public int idMesero { get; set; }
        public string nombreMesero { get; set; }
        public int estadoServicio { get; set; }
        public int estadoPreCuenta { get; set; }
        public decimal porcentajePropina { get; set; }
        public decimal subtotal { get; set; }
        public decimal impuesto { get; set; }
        public decimal totalsinservicio { get; set; }
        public decimal servicio { get; set; }
        public decimal totalconservicio { get; set; }
    }
}
