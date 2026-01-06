using DAL;          // CrudSpHelper, SqlAutoDAL
using DAL.Model;
using System;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class R_VentaVendedorControler
    {
        /// <summary>
        /// CRUD para R_VentaVendedor usando SP:
        /// EXEC CRUD_R_VentaVendedor @json, @funcion
        /// funcion: 0 = INSERT, 1 = UPDATE, 2 = DELETE
        /// </summary>
        public static async Task<Respuesta_DAL> CRUD(string db, R_VentaVendedor rvv, int funcion)
        {
            try
            {
                var helper = new CrudSpHelper();

                // Llama al helper genérico que arma:
                // EXEC [dbo].[CRUD_R_VentaVendedor] @json = N'...', @funcion = {funcion}
                var resp = await helper.CrudAsync(db, rvv, funcion);

                return resp ?? new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Sin respuesta del servidor en CRUD_R_VentaVendedor."
                };
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Error en CRUD_R_VentaVendedor: " + msg
                };
            }
        }

        /// <summary>
        /// Consulta la relación por idVenta.
        /// Equivale a:
        /// SELECT TOP 1 * FROM R_VentaVendedor WHERE idVenta = @idventa
        /// </summary>
        public static async Task<R_VentaVendedor> Consultar_idventa(string db, int idventa)
        {
            try
            {
                var auto = new SqlAutoDAL();

                // OJO: aquí se filtra por idVenta (antes estaba x.id == idventa, que era un bug)
                var relacion = await auto.ConsultarUno<R_VentaVendedor>(
                    db,
                    x => x.idVenta == idventa
                );

                return relacion; // puede ser null si no existe
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
