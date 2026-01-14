using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_R_MediosDePago_MediosDePagoInternosControler
    {
        public static async Task<List<Model.V_R_MediosDePago_MediosDePagoInternos>> GetAll(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarLista<Model.V_R_MediosDePago_MediosDePagoInternos>(db);
                return resp;
            }
            catch (Exception ex)
            {
                string mensaje = ex.Message;
                return new List<Model.V_R_MediosDePago_MediosDePagoInternos>();
            }
        }
    }
}
