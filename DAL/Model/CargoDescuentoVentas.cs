using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class CargoDescuentoVentas
    {
        public int id { get; set; }
        public int idVenta { get; set; }

        /// <summary>
        /// true = Cargo | false = Descuento
        /// </summary>
        public bool tipo { get; set; }

        public int codigo { get; set; }
        public string razon { get; set; }
        public decimal valor { get; set; }
        public decimal baseCD { get; set; }
        public string descripcionCargoDescuento { get; set; }
    }
}
