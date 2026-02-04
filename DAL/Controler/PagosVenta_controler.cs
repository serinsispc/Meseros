using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class PagosVenta_controler
    {
        public static async Task<bool> CRUD(string db, List<PagosVenta> pagosVentas, int funcion)
        {
            try
            {
                if (pagosVentas == null || pagosVentas.Count == 0)
                    return false;

                var auto = new SqlAutoDAL();

                // ✅ serializar lista
                string json = JsonConvert.SerializeObject(pagosVentas);

                // ✅ escapar comillas
                json = json.Replace("'", "''");

                // ✅ armar EXEC
                string sql = $@"EXEC dbo.CRUD_PagosVenta N'{json}', {funcion}";

                // ✅ ejecutar con TU DAL
                var r = await auto.EjecutarSQLObjeto<RespuestaCRUD>(db, sql);

                if (r == null)
                    return false;

                return r.estado;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return false;
            }
        }


    }
}
