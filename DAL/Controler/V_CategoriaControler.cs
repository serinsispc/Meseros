using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_CategoriaControler
    {
        public static async Task<List<V_Categoria>> lista(string db)
        {
            try
            {
                var auto = new SqlAutoDAL();
                var resp = await auto.ConsultarLista<V_Categoria>(db, x=>x.visible==1);
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
