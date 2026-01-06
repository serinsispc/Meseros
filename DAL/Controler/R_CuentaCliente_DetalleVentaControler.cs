using DAL;
using DAL.Funciones;         // para ComandImpresaa_f
using DAL.Model;
using System;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class R_CuentaCliente_DetalleVentaControler
    {
        /// <summary>
        /// Consulta la relación por idDetalleVenta.
        /// Equivale a:
        /// SELECT TOP 1 * FROM R_CuentaCliente_DetalleVenta WHERE idDetalleVenta = @iddetalle
        /// </summary>
        public static async Task<R_CuentaCliente_DetalleVenta> Consultar_idDetalle(string db, int iddetalle)
        {
            try
            {
                var auto = new SqlAutoDAL();

                // Genera y ejecuta:
                // SELECT TOP 1 * FROM R_CuentaCliente_DetalleVenta WHERE idDetalleVenta = {iddetalle}
                var relacion = await auto.ConsultarUno<R_CuentaCliente_DetalleVenta>(
                    db,
                    x => x.idDetalleVenta == iddetalle
                );

                return relacion;  // puede ser null si no existe
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// CRUD usando el SP:
        /// EXEC CRUD_R_CuentaCliente_DetalleVenta @json, @funcion
        /// boton: 0 = INSERT, 1 = UPDATE, 2 = DELETE
        /// </summary>
        public static async Task<Respuesta_DAL> CRUD(string db, R_CuentaCliente_DetalleVenta relacion, int boton)
        {
            try
            {
                var helper = new CrudSpHelper();

                // Ejecuta el SP genérico
                var resp = await helper.CrudAsync(db, relacion, boton);

                // Si es UPDATE o DELETE, liberamos el detalle como en tu lógica original
                if (boton > 0)
                {
                    await ComandImpresaa_f.LiberarDetalle(db, relacion.idDetalleVenta);
                }

                return resp ?? new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Sin respuesta del servidor en CRUD_R_CuentaCliente_DetalleVenta."
                };
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Error en CRUD_R_CuentaCliente_DetalleVenta: " + error
                };
            }
        }
    }
}
