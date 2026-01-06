using DAL;            // <-- para CrudSpHelper y SqlAutoDAL
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class CuentaClienteControler
    {
        /// <summary>
        /// Ejecuta el SP CRUD_CuentaCliente (@json, @funcion)
        /// boton/funcion: 0 = INSERT, 1 = UPDATE, 2 = DELETE
        /// </summary>
        public static async Task<Respuesta_DAL> CRUD(string db, CuentaCliente cuenta, int boton)
        {
            try
            {
                var helper = new CrudSpHelper();

                // Llamamos al helper genérico que arma:
                // EXEC [dbo].[CRUD_CuentaCliente] @json = N'...', @funcion = X
                var resp = await helper.CrudAsync(db, cuenta, boton);

                // Si fue exitoso, ajustamos el mensaje (si quieres mantener tu estilo)
                if (resp != null && resp.estado)
                {
                    string accion;
                    switch (boton)
                    {
                        case 0: accion = "creada"; break;
                        case 1: accion = "actualizada"; break;
                        case 2: accion = "eliminada"; break;
                        default: accion = "procesada"; break;
                    }

                    // Solo sobreescribimos si el SP no envió mensaje
                    if (string.IsNullOrEmpty(resp.mensaje))
                        resp.mensaje = $"Cuenta {accion} correctamente.";
                }

                return resp ?? new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Sin respuesta del servidor."
                };
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Error en CRUD_CuentaCliente: " + msg
                };
            }
        }

        /// <summary>
        /// Consulta una CuentaCliente por id (SELECT TOP 1 * FROM CuentaCliente WHERE id = @id)
        /// </summary>
        public static async Task<CuentaCliente> CuentaCliente(string db, int id)
        {
            try
            {
                var auto = new SqlAutoDAL();

                // Genera y ejecuta:
                // SELECT TOP 1 * FROM CuentaCliente WHERE id = {id}
                var cuenta = await auto.ConsultarUno<CuentaCliente>(db, x => x.id == id);

                return cuenta; // puede ser null si no existe
            }
            catch (Exception ex)
            {
                string respuesta = ex.Message;
                return null;
            }
        }
    }
}
