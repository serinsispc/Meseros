using DAL;          // CrudSpHelper, SqlAutoDAL
using DAL.Model;
using System;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class TablaVentasControler
    {
        /// <summary>
        /// CRUD para TablaVentas usando SP:
        /// EXEC CRUD_TablaVentas @json, @funcion
        /// boton/funcion: 0 = INSERT, 1 = UPDATE, 2 = DELETE
        /// </summary>
        public static async Task<Respuesta_DAL> CRUD(string db, TablaVentas venta, int boton)
        {
            try
            {
                var helper = new CrudSpHelper();

                // Llama al helper genérico que arma:
                // EXEC [dbo].[CRUD_TablaVentas] @json = N'...', @funcion = {boton}
                var resp = await helper.CrudAsync(db, venta, boton);

                return resp ?? new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Sin respuesta del servidor en CRUD_TablaVentas."
                };
            }
            catch (Exception ex)
            {
                return new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Error en CRUD_TablaVentas: " + ex.GetBaseException().Message
                };
            }
        }

        /// <summary>
        /// Consulta una venta por id.
        /// Equivale a: SELECT TOP 1 * FROM TablaVentas WHERE id = @id
        /// </summary>
        public static async Task<Respuesta_DAL> Consultar_Id(string db, int id)
        {
            try
            {
                var auto = new SqlAutoDAL();

                // SELECT TOP 1 * FROM TablaVentas WHERE id = {id}
                var venta = await auto.ConsultarUno<TablaVentas>(db, x => x.id == id);

                if (venta == null)
                {
                    return new Respuesta_DAL
                    {
                        data = null,
                        estado = false,
                        mensaje = "No se encontró la venta."
                    };
                }

                return new Respuesta_DAL
                {
                    data = venta,
                    estado = true,
                    mensaje = "ok"
                };
            }
            catch (Exception ex)
            {
                return new Respuesta_DAL
                {
                    data = null,
                    estado = false,
                    mensaje = "Error en Consultar_Id: " + ex.Message
                };
            }
        }

        public static async Task<TablaVentas> ConsultarIdVenta(string db, int id)
        {
            try
            {
                var auto = new SqlAutoDAL();

                // SELECT TOP 1 * FROM TablaVentas WHERE id = {id}
                var venta = await auto.ConsultarUno<TablaVentas>(db, x => x.id == id);

                if (venta == null)
                {
                    return null;
                }
                return venta;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
        public static async Task<int> Consecutivo(string db, int idResolucion)
        {
            try
            {
                var auto = new SqlAutoDAL();

                string sql = $@"
            SELECT ISNULL(MAX(numeroVenta), 0) AS consecutivo
            FROM TablaVentas
            WHERE idResolucion = {idResolucion};";

                var r = await auto.EjecutarSQLObjeto<V_ConsecutivoVenta>(db, sql);

                // Si no hay registros, devuelve 0
                if (r == null) return 0;

                return r.consecutivo+1;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return 0;
            }
        }

    }
}
