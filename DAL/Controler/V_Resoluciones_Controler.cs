using DAL.Model;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_Resoluciones_Controler
    {
        public static async Task<V_Resoluciones>ConsulrarResolucion(string db, string nombre)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarUno<V_Resoluciones>(db,x=>x.nombreRosolucion==nombre && x.estado==1);
                if (resp != null) 
                {
                    return resp;
                }
                return null;
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        public static async Task<V_Resoluciones> ConsultarID(string db, int id)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarUno<V_Resoluciones>(db, x => x.idResolucion == id);
                if (resp != null)
                {
                    return resp;
                }
                return null;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
