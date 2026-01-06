using DAL;
using DAL.Model;
using System;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class ImprecionComandaAddControler
    {
        /// <summary>
        /// CRUD para ImprecionComandaAdd usando SP:
        /// EXEC CRUD_ImprecionComandaAdd @json, @funcion
        /// boton: 0 = INSERT, 1 = UPDATE, 2 = DELETE
        /// </summary>
        public static async Task<Respuesta_DAL> CRUD(string db, ImprecionComandaAdd imprecion, int boton)
        {
            try
            {
                var helper = new CrudSpHelper();

                // Ejecuta el SP nuevo
                var resp = await helper.CrudAsync(db, imprecion, boton);

                return resp ?? new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Sin respuesta del servidor."
                };
            }
            catch (Exception ex)
            {
                return new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Error en CRUD_ImprecionComandaAdd: " + ex.Message
                };
            }
        }
    }
}
