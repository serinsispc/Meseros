using System;

namespace DAL.Model
{
    public class V_Precios
    {
        public int id { get; set; }
        public int idPresentacion { get; set; }
        public string nombrePrecio { get; set; } = string.Empty;
        public decimal valorPrecio { get; set; }
    }
}
