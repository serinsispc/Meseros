using DAL.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_R_PermisoCajeroControler
    {
        public static async Task<List<V_R_PermisoCajero>> Lista(string db, int idCajero)
        {
            try
            {
                var cn = new SqlAutoDAL();
                return await cn.ConsultarLista<V_R_PermisoCajero>(db, x => x.idCajero == idCajero);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new List<V_R_PermisoCajero>();
            }
        }

        public static async Task<bool> TienePermiso(string db, int idCajero, int idPermiso)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var permiso = await cn.ConsultarUno<V_R_PermisoCajero>(db, x => x.idCajero == idCajero && x.idPermiso == idPermiso);
                return permiso != null;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return false;
            }
        }
    }
}
