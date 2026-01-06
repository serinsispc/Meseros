using DAL;          // CrudSpHelper, SqlAutoDAL
using DAL.Model;
using System;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class R_VentaMesaControler
    {
        /// <summary>
        /// CRUD para R_VentaMesa usando SP:
        /// EXEC CRUD_R_VentaMesa @json, @funcion
        /// funcion: 0 = INSERT, 1 = UPDATE, 2 = DELETE
        /// </summary>
        public static async Task<Respuesta_DAL> CRUD(string db, R_VentaMesa rvm, int funcion)
        {
            try
            {
                var helper = new CrudSpHelper();

                // Llama al helper genérico:
                // EXEC [dbo].[CRUD_R_VentaMesa] @json = N'...', @funcion = {funcion}
                var resp = await helper.CrudAsync(db, rvm, funcion);

                return resp ?? new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Sin respuesta del servidor en CRUD_R_VentaMesa."
                };
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Error en CRUD_R_VentaMesa: " + msg
                };
            }
        }

        /// <summary>
        /// Consulta la relación por idVenta e idMesa.
        /// Equivale a:
        /// SELECT TOP 1 * FROM R_VentaMesa WHERE idVenta = @idventa AND idMesa = @idmesa
        /// </summary>
        public static async Task<R_VentaMesa> Consultar_relacion(string db, int idventa, int idmesa)
        {
            try
            {
                var auto = new SqlAutoDAL();

                // Genera y ejecuta:
                // SELECT TOP 1 * FROM R_VentaMesa WHERE idVenta = idventa AND idMesa = idmesa
                var relacion = await auto.ConsultarUno<R_VentaMesa>(
                    db,
                    x => x.idVenta == idventa && x.idMesa == idmesa
                );

                return relacion; // puede ser null si no existe
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return null;
            }
        }
    }
}
