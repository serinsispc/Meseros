using DAL;
using DAL.Model;
using System;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class SedeControler
    {
        public static async Task<Sede> Consultar(string db)
        {
            try
            {
                var auto = new SqlAutoDAL();

                // Ahora sí funciona:
                var resp = await auto.ConsultarUno<Sede>(db,x=>x.id>0);

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
