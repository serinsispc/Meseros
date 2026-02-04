using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class MediosDePagoInternos_Controler
    {
        public static async Task<string>ConsultarReferencia(string db,int idmpi)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarUno<MediosDePagoInternos>(db, x=>x.id==idmpi);
                return resp.nombreMPI;
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return "--";
            }
        }
    }
}
