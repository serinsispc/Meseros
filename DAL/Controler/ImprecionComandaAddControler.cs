using DAL;
using DAL.Model;
using System;
using System.Data.SqlClient;
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
                if (boton == 0)
                {
                    return await InsertPendingComandaAsync(db, imprecion);
                }

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

        private static async Task<Respuesta_DAL> InsertPendingComandaAsync(string db, ImprecionComandaAdd imprecion)
        {
            using (var connection = new SqlConnection(RuntimeSettings.BuildSqlConnectionString(db)))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                const string sql = @"
DECLARE @resultado INT = 0;

IF EXISTS (
    SELECT 1
    FROM ImprecionComandaAdd WITH (UPDLOCK, HOLDLOCK)
    WHERE idVenta = @idVenta AND estado = 1
)
BEGIN
    SET @resultado = 2;
END
ELSE IF EXISTS (
    SELECT 1
    FROM V_DetalleComandas
    WHERE idVenta = @idVenta AND itemComandado = 0
)
BEGIN
    INSERT INTO ImprecionComandaAdd (idVenta, idMesa, idMesero, estado)
    VALUES (@idVenta, @idMesa, @idMesero, 1);

    SET @resultado = 1;
END

SELECT @resultado;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idVenta", imprecion.idVenta);
                    command.Parameters.AddWithValue("@idMesa", (object)imprecion.idMesa ?? DBNull.Value);
                    command.Parameters.AddWithValue("@idMesero", (object)imprecion.idMesero ?? DBNull.Value);

                    int resultado = Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false));

                    if (resultado == 1)
                    {
                        return new Respuesta_DAL
                        {
                            data = imprecion.idVenta,
                            estado = true,
                            mensaje = "Comanda enviada correctamente."
                        };
                    }

                    if (resultado == 2)
                    {
                        return new Respuesta_DAL
                        {
                            data = imprecion.idVenta,
                            estado = true,
                            mensaje = "La comanda ya estaba pendiente en cola de impresion."
                        };
                    }

                    return new Respuesta_DAL
                    {
                        data = 0,
                        estado = false,
                        mensaje = "No hay items pendientes por comandar."
                    };
                }
            }
        }
    }
}
