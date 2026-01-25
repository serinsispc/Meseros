using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class controlador_tokenEmpresa
    {
        public static async Task<string> ConsultarTokenSerinsisPC()
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarLista<tokenEmpresa>("DBSerinsisPC");
                var objeto = resp.FirstOrDefault();
                return objeto.token;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return error;
            }
        }
    }
}
