using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class tax_detailsControler
    {
        public static async Task<List<tax_details>> ListaDetallesImpuesto(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarLista<tax_details>(db);
                return resp.ToList();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new List<tax_details>();
            }
        }
    }
}
