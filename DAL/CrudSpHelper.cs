using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace DAL
{
    public class CrudSpHelper
    {
        public CrudSpHelper()
        {
            // No necesitamos campos privados, todo se maneja por using
        }

        /// <summary>
        /// Ejecuta un SP de la forma CRUD_{NombreClase}(@json, @funcion)
        /// funcion: 0 = INSERT, 1 = UPDATE, 2 = DELETE
        /// </summary>
        public async Task<Respuesta_DAL> CrudAsync<T>(string db, T entidad, int funcion)
            where T : class
        {
            try
            {
                if (entidad == null)
                    throw new ArgumentNullException("entidad");

                // 1. Nombre del SP según la clase
                string spName = "CRUD_" + typeof(T).Name; // ej: DetalleVenta → CRUD_DetalleVenta

                // 2. Serializar a JSON
                string json = JsonConvert.SerializeObject(entidad);

                // 3. Escapar comillas simples para SQL
                json = EscapeJsonForSql(json);

                // 4. Construir comando EXEC
                string sql = string.Format(
                    "EXEC [dbo].[{0}] @json = N'{1}', @funcion = {2}",
                    spName,
                    json,
                    funcion
                );

                // 5. Ejecutar contra la base (usa el constructor Conection_SQL(string db))
                using (var conexion = new Conection_SQL(db))
                {
                    // false = queremos un solo objeto en la respuesta
                    string resultadoJson = await conexion.EjecutarConsulta(sql, false);

                    if (string.IsNullOrEmpty(resultadoJson))
                    {
                        return new Respuesta_DAL
                        {
                            data = 0,
                            estado = false,
                            mensaje = "Sin respuesta del servidor."
                        };
                    }

                    // 6. Deserializar respuesta del SP
                    var respcrud = JsonConvert.DeserializeObject<RespuestaCRUD>(resultadoJson);
                    var resp = new Respuesta_DAL
                    {
                        data = respcrud.IdFinal,
                        estado = respcrud.estado,
                        mensaje = respcrud.mensaje
                    };
                    if (resp == null)
                    {
                        return new Respuesta_DAL
                        {
                            data = 0,
                            estado = false,
                            mensaje = "No se pudo interpretar la respuesta del servidor."
                        };
                    }

                    return resp;
                }
            }
            catch (Exception ex)
            {
                return new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Error en CrudAsync: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Reemplaza comillas simples para que el JSON sea seguro dentro de un string SQL.
        /// </summary>
        private string EscapeJsonForSql(string json)
        {
            if (string.IsNullOrEmpty(json))
                return json;

            return json.Replace("'", "''");
        }
    }
}
