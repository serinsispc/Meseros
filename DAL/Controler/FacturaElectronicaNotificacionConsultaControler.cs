using DAL.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public static class FacturaElectronicaNotificacionConsultaControler
    {
        public static async Task<List<FacturaElectronicaNotificacionInfo>> ConsultarPorVentaAsync(string db, int idVenta)
        {
            const string query = @"
IF OBJECT_ID('dbo.FacturaElectronicaNotificaciones', 'U') IS NULL
BEGIN
    SELECT
        CAST(0 AS int) AS idNotificacion,
        CAST(0 AS int) AS idVenta,
        CAST('' AS varchar(80)) AS tipoNotificacion,
        CAST('' AS varchar(200)) AS titulo,
        CAST('' AS varchar(max)) AS mensaje,
        CAST(NULL AS varchar(max)) AS detalle,
        CAST(NULL AS varchar(80)) AS documento,
        CAST(NULL AS varchar(255)) AS cufe,
        CAST(NULL AS int) AS consecutivoIntentado,
        CAST(NULL AS datetime) AS fechaCreacion,
        CAST(NULL AS datetime) AS fechaActualizacion
    WHERE 1 = 0;
    RETURN;
END;

SELECT
    idNotificacion,
    idVenta,
    ISNULL(tipoNotificacion, '') AS tipoNotificacion,
    ISNULL(titulo, '') AS titulo,
    ISNULL(mensaje, '') AS mensaje,
    detalle,
    documento,
    cufe,
    consecutivoIntentado,
    fechaCreacion,
    fechaActualizacion
FROM dbo.FacturaElectronicaNotificaciones
WHERE idVenta = " + @"{0}
ORDER BY fechaCreacion DESC, idNotificacion DESC;";

            using (var cn = new Conection_SQL(db))
            {
                var json = await cn.EjecutarConsulta(string.Format(query, idVenta), true);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<FacturaElectronicaNotificacionInfo>();
                }

                return JsonConvert.DeserializeObject<List<FacturaElectronicaNotificacionInfo>>(json)
                    ?? new List<FacturaElectronicaNotificacionInfo>();
            }
        }
    }
}
