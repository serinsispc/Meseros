using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class Imagenes
    {
        public Guid id { get; set; }
        public byte[] imagenBytes { get; set; }
    }
}
