using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication.Class
{
    public class ClassPropina
    {
        public static int CalcularValoPropina(string porcentaje,string valor)
        {
            try
            {
                decimal por = 0;
                decimal vl = 0;

                if (porcentaje != string.Empty) { por = Convert.ToDecimal(porcentaje); }
                if (valor != string.Empty) { vl = Convert.ToDecimal(valor); }

                decimal resul = vl * por;
                return Convert.ToInt32(resul);
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return 0;
            }
        }
    }
}