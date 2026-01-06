using DAL;                 // CrudSpHelper, SqlAutoDAL
using DAL.Funciones;       // ComandImpresaa_f
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class DetalleVentaControler
    {
        /// <summary>
        /// CRUD DetalleVenta usando SP CRUD_DetalleVenta (@json, @funcion)
        /// boton/funcion: 0 = INSERT, 1 = UPDATE, 2 = DELETE
        /// </summary>
        public static async Task<Respuesta_DAL> CRUD(string db, DetalleVenta dv, int boton)
        {
            try
            {
                var helper = new CrudSpHelper();

                // Ejecuta: EXEC [dbo].[CRUD_DetalleVenta] @json = N'...', @funcion = {boton}
                var resp = await helper.CrudAsync(db, dv, boton);

                // Si es UPDATE o DELETE, se sigue llamando a LiberarDetalle como antes
                if (boton > 0)
                {
                    // función que se encarga de eliminar ComandImpresaa
                    await ComandImpresaa_f.LiberarDetalle(db, dv.id);
                }

                // Garantizamos una respuesta consistente
                return resp ?? new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Sin respuesta del servidor en CRUD_DetalleVenta."
                };
            }
            catch (Exception e)
            {
                string msg = e.Message;
                return new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Error en CRUD_DetalleVenta: " + msg
                };
            }
        }

        /// <summary>
        /// Consulta un DetalleVenta por id (SELECT TOP 1 * FROM DetalleVenta WHERE id = @id)
        /// </summary>
        public static async Task<DetalleVenta> ConsultarId(string db, int id)
        {
            try
            {
                var auto = new SqlAutoDAL();

                // Genera y ejecuta: SELECT TOP 1 * FROM DetalleVenta WHERE id = {id}
                var detalle = await auto.ConsultarUno<DetalleVenta>(db, x => x.id == id);

                return detalle; // puede ser null si no existe
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return null;
            }
        }
    }
}
