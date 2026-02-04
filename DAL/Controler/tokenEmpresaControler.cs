using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class tokenEmpresaControler
    {
        public static async Task<tokenEmpresa>ConsultarToken(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarUno<tokenEmpresa>(db,x=>x.id>0);
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
