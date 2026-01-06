using DAL;
using DAL.Model;
using System;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class ImprimirCuentaControler
    {
        /// <summary>
        /// CRUD ImprimirCuenta usando SP:
        /// EXEC CRUD_ImprimirCuenta @json, @funcion
        /// boton: 0 = INSERT, 1 = UPDATE, 2 = DELETE
        /// </summary>
        public static async Task<Respuesta_DAL> CRUD(string db, ImprimirCuenta cuenta, int boton)
        {
            try
            {
                var helper = new CrudSpHelper();

                // Llama al helper genérico que construye:
                // EXEC [dbo].[CRUD_ImprimirCuenta] @json = N'...', @funcion = {boton}
                var resp = await helper.CrudAsync(db, cuenta, boton);

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
                    mensaje = "Error en CRUD_ImprimirCuenta: " + msg
                };
            }
        }
    }
}
