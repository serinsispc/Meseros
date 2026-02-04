using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_CorreosClienteControler
    {
        public static async Task<List<V_CorreosCliente>>Lista(string db,int idCliente)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarLista<V_CorreosCliente>(db,x=>x.idCliente==idCliente);
                return resp;
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return new List<V_CorreosCliente>();
            }
        }
    }
}
