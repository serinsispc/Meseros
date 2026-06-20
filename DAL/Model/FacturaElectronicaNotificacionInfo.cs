using System;

namespace DAL.Model
{
    public class FacturaElectronicaNotificacionInfo
    {
        public int idNotificacion { get; set; }
        public int idVenta { get; set; }
        public string tipoNotificacion { get; set; }
        public string titulo { get; set; }
        public string mensaje { get; set; }
        public string detalle { get; set; }
        public string documento { get; set; }
        public string cufe { get; set; }
        public int? consecutivoIntentado { get; set; }
        public DateTime? fechaCreacion { get; set; }
        public DateTime? fechaActualizacion { get; set; }
    }
}
