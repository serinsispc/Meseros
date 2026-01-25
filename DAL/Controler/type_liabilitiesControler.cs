using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class type_liabilitiesControler
    {
        public static async Task<List<type_liabilities>> ListaTiposResponsabilidad(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarLista<type_liabilities>(db);
                return resp.ToList();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new List<type_liabilities>();
            }
        }
    }
}
