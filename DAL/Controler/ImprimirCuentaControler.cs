using DAL;
using DAL.Model;
using System;
using System.Data;
using System.Data.SqlClient;
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
                if (boton == 0)
                {
                    return await InsertAsync(db, cuenta).ConfigureAwait(false);
                }

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

        private static async Task<Respuesta_DAL> InsertAsync(string db, ImprimirCuenta cuenta)
        {
            using (var connection = new SqlConnection(RuntimeSettings.BuildSqlConnectionString(db)))
            using (var command = new SqlCommand(@"
INSERT INTO dbo.ImprimirCuenta (idVenta, namePrinter, ancho)
VALUES (@idVenta, @namePrinter, @ancho);
SELECT CAST(SCOPE_IDENTITY() AS int);", connection))
            {
                command.Parameters.Add("@idVenta", SqlDbType.Int).Value = cuenta.idVenta;
                command.Parameters.Add("@namePrinter", SqlDbType.NVarChar, 500).Value =
                    string.IsNullOrWhiteSpace(cuenta.namePrinter)
                        ? (object)DBNull.Value
                        : cuenta.namePrinter.Trim();
                command.Parameters.Add("@ancho", SqlDbType.Int).Value = cuenta.ancho;

                await connection.OpenAsync().ConfigureAwait(false);
                var id = Convert.ToInt32(await command.ExecuteScalarAsync().ConfigureAwait(false));

                return new Respuesta_DAL
                {
                    data = id,
                    estado = id > 0,
                    mensaje = id > 0 ? "Cuenta enviada correctamente." : "No se pudo encolar la cuenta."
                };
            }
        }
    }
}
