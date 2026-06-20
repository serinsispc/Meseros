using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public static class FacturaElectronicaNotificacionControler
    {
        public static async Task RegistrarAsync(
            string db,
            int idVenta,
            string tipoNotificacion,
            string titulo,
            string mensaje,
            string detalle = null,
            string documento = null,
            string cufe = null,
            int? consecutivoIntentado = null)
        {
            if (string.IsNullOrWhiteSpace(db) || idVenta <= 0)
            {
                return;
            }

            using (var connection = new SqlConnection(RuntimeSettings.BuildSqlConnectionString(db)))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                const string sql = @"
IF OBJECT_ID('dbo.FacturaElectronicaNotificaciones', 'U') IS NULL
BEGIN
    RETURN;
END;

INSERT INTO dbo.FacturaElectronicaNotificaciones
(
    idVenta,
    tipoNotificacion,
    titulo,
    mensaje,
    detalle,
    documento,
    cufe,
    consecutivoIntentado,
    fechaCreacion
)
VALUES
(
    @idVenta,
    @tipoNotificacion,
    @titulo,
    @mensaje,
    @detalle,
    @documento,
    @cufe,
    @consecutivoIntentado,
    GETDATE()
);";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@idVenta", idVenta);
                    command.Parameters.AddWithValue("@tipoNotificacion", (object)(tipoNotificacion ?? string.Empty));
                    command.Parameters.AddWithValue("@titulo", (object)(titulo ?? string.Empty));
                    command.Parameters.AddWithValue("@mensaje", (object)(mensaje ?? string.Empty));
                    command.Parameters.AddWithValue("@detalle", (object)detalle ?? DBNull.Value);
                    command.Parameters.AddWithValue("@documento", (object)documento ?? DBNull.Value);
                    command.Parameters.AddWithValue("@cufe", (object)cufe ?? DBNull.Value);
                    command.Parameters.AddWithValue("@consecutivoIntentado", (object)consecutivoIntentado ?? DBNull.Value);
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
