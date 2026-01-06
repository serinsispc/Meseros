using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_CuentasVentaControler
    {
        public static async Task<List<V_CuentasVenta>> Lista_IdVendedor(string db, int idvendedor)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarLista<V_CuentasVenta>(db, x => x.idvendedor == idvendedor && x.numeroVenta==0);
                return resp;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
