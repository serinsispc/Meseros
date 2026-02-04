using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class V_DetalleComandas
    {
        public int id { get; set; }
        public int idVenta { get; set; }
        public string nombreProducto { get; set; }
        public int unidad { get; set; }
        public string nota { get; set; }
        public string nombreCuenta {  get; set; }
        public int itemComandado { get; set; }
        public string impresora {  get; set; }
    }
}
