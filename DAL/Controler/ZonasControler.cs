using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class ZonasControler
    {
        public static async Task<List<Zonas>> Lista(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                return await cn.ConsultarLista<Zonas>(db);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
