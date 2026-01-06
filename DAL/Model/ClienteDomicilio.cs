using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class ClienteDomicilio
    {
        public Guid id { get; set; }                  
        public string celularCliente { get; set; }   
        public string nombreCliente { get; set; }     
        public string direccionCliente { get; set; }  
    }
}
