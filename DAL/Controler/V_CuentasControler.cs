using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_CuentasControler
    {
        public static async Task<List<V_Cuentas>> Lista_IdVendedor(string db,int idvendedor)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarLista<V_Cuentas>(db, x => x.idVendedor == idvendedor);
                return resp;
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        public static async Task<List<V_Cuentas>> Lista_Mesa(string db, string mesa)
        {
            try
            {
                var cn = new SqlAutoDAL();

                // SELECT * FROM V_Cuentas WHERE mesa LIKE '%mesa%'
                return await cn.ConsultarLista<V_Cuentas>(
                    db,
                    x => x.mesa.Contains(mesa)
                );
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
