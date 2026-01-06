using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DAL
{
    public class RespuestaCRUD
    {
        public bool estado { get; set; }
        public string nuevoId { get; set; }
        public string idAfectado { get; set; }
        public string mensaje { get; set; }

        [JsonIgnore] // No se envía al serializar
        public string IdFinal =>
            !string.IsNullOrEmpty(idAfectado) ? idAfectado : nuevoId;
    }
}
