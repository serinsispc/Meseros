using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_ServicioMesaControler
    {
        public static async Task<List<V_ServicioMesa>> lista(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                return await cn.ConsultarLista<V_ServicioMesa>(db, x => x.estadoServicio == 1);
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
