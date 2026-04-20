using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class PuntosDePagoControler
    {
        public static async Task<List<PuntosDePago>> Lista(string db)
        {
            try
            {
                var auto = new SqlAutoDAL();
                var puntos = await auto.ConsultarLista<PuntosDePago>(db);
                return puntos?.OrderBy(x => x.nombrePunto).ToList() ?? new List<PuntosDePago>();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new List<PuntosDePago>();
            }
        }

        public static async Task<PuntosDePago> Consultar(string db, int id)
        {
            try
            {
                var auto = new SqlAutoDAL();
                return await auto.ConsultarUno<PuntosDePago>(db, x => x.id == id);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
