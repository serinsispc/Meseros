using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class DBConexionControler
    {
        public static async Task<DBConexion> DAsync(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarUno<DBConexion>(db);
                return resp;
            }
            catch (Exception ex)
            {
                string mensaje = ex.Message;
                return null;
            }
        }

        public static async Task<RespuestaCRUD> CrudAsync(string db, DBConexion obj, int funcion)
        {
            try
            {
                var cn = new SqlAutoDAL();
                string json = JsonConvert.SerializeObject(obj);

                var resp = await cn.EjecutarSPObjeto<RespuestaCRUD>(db, "CRUD_DBConexion", json, funcion);
                return resp;
            }
            catch (Exception ex)
            {
                return new RespuestaCRUD
                {
                    estado =false,
                    idAfectado = null,
                    mensaje = ex.Message
                };
            }
        }
    }
}