using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_TablaVentasControler
    {
        public static async Task<V_TablaVentas> Consultar_Id(string db,int idventa)
        {
            try
            {
                var cn = new SqlAutoDAL();
                return await cn.ConsultarUno<V_TablaVentas>(db, x=>x.id==idventa);
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
