using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class payment_methodsControler
    {
        public static async Task<List<payment_methods>>ListaMetodosDePago(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp= await cn.ConsultarLista<payment_methods>(db);
                return resp.Where(x=>x.state==true).ToList();
            }
            catch(Exception ex)
            {
                string mensaje = ex.Message;
                return new List<payment_methods>();
            }
        }
    }
}
