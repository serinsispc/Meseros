using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class R_VendedorUsuarioControler
    {
        public static async Task<R_VendedorUsuario>ConsultarRelacion(string db, int idvendedor)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarUno<R_VendedorUsuario>(db, x => x.idVendedor == idvendedor);
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
