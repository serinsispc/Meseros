using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_CatagoriaAdicionControler
    {
        public static async Task<List<V_CatagoriaAdicion>> Lista(string db)
        {
            try
            {
                var auto = new SqlAutoDAL();
                var resp=await auto.ConsultarLista<V_CatagoriaAdicion>(db,x=>x.estado==1);
                return resp;
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
