using DAL;          // <-- importante para CrudSpHelper y SqlAutoDAL
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class ComandImpresaaControler
    {
        /// <summary>
        /// Ejecuta el SP CRUD_ComandImpresaa (@json, @funcion)
        /// funcion/boton: 0 = INSERT, 1 = UPDATE, 2 = DELETE
        /// </summary>
        public static async Task<Respuesta_DAL> CRUD(string db, ComandImpresaa comand, int boton)
        {
            try
            {
                var helper = new CrudSpHelper();
                // Usa el helper genérico basado en JSON
                var resp = await helper.CrudAsync(db, comand, boton);
                return resp;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new Respuesta_DAL { data = 0, estado = false, mensaje = "Error en CRUD_ComandImpresaa: " + msg };
            }
        }

        /// <summary>
        /// Consulta el primer registro de ComandImpresaa por idDetalleVenta
        /// </summary>
        public static async Task<ComandImpresaa> CosultarIdDetalle(string db, int idDetalle)
        {
            try
            {
                var auto = new SqlAutoDAL();

                // Genera: SELECT TOP 1 * FROM ComandImpresaa WHERE idDetalleVenta = {idDetalle}
                var resp = await auto.ConsultarUno<ComandImpresaa>(db, x => x.idDetalleVenta == idDetalle);

                return resp;   // puede ser null si no existe
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return null;
            }
        }
    }
}
