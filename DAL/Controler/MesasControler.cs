using DAL;
using DAL.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class MesasControler
    {
        /// <summary>
        /// CRUD para Mesas usando SP:
        /// EXEC CRUD_Mesas @json, @funcion
        /// funcion: 0 = INSERT, 1 = UPDATE, 2 = DELETE
        /// </summary>
        public static async Task<Respuesta_DAL> CRUD(string db, Mesas mesas, int funcion)
        {
            try
            {
                var helper = new CrudSpHelper();

                var resp = await helper.CrudAsync(db, mesas, funcion);

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
                    mensaje = "Error en CRUD_Mesas: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Lista completa de mesas usando SqlAutoDAL.
        /// Equivale a: SELECT * FROM Mesas
        /// </summary>
        public static async Task<List<Mesas>> Lista(string db)
        {
            try
            {
                var auto = new SqlAutoDAL();
                return await auto.ConsultarLista<Mesas>(db);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Consulta una mesa por ID.
        /// Equivale a: SELECT TOP 1 * FROM Mesas WHERE id = @id
        /// </summary>
        public static async Task<Mesas> Consultar_id(string db, int id)
        {
            try
            {
                var auto = new SqlAutoDAL();
                return await auto.ConsultarUno<Mesas>(db, x => x.id == id);
            }
            catch
            {
                return null;
            }
        }
    }
}
