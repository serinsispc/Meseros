using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class AjustarJson
    {
        public static string Ajustar(string json)
        {
            //ajsutar el josn
            return json.Replace("'", "''");
        }
    }
}
