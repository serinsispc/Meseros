using DAL.Model;
using System;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class NotasCreditoControler
    {
        public static async Task<NotasCredito> ConsultarIdVenta(string db, int idVenta)
        {
            try
            {
                var cn = new SqlAutoDAL();
                return await cn.ConsultarUno<NotasCredito>(db, x => x.idVenta == idVenta);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        public static async Task<NotasCredito> GuardarResultadoAsync(string db, NotasCredito notaCredito)
        {
            if (notaCredito == null || notaCredito.idVenta.GetValueOrDefault() <= 0)
            {
                return null;
            }

            try
            {
                string cufe = EscapeSql(notaCredito.cufe);
                string numeroFactura = EscapeSql(notaCredito.numeroFactura);
                string fechaEmision = EscapeSql(notaCredito.fechaEmision);
                string fechaVencimiento = EscapeSql(notaCredito.fecahVensimiento);
                string dataQr = EscapeSql(notaCredito.dataQR);
                string imagenQr = EscapeSql(notaCredito.imagenQR);

                string sql = $@"
IF OBJECT_ID('dbo.NotasCredito', 'U') IS NULL
BEGIN
    SELECT
        CAST(NULL AS int) AS id,
        CAST(NULL AS int) AS idVenta,
        CAST(NULL AS varchar(200)) AS cufe,
        CAST(NULL AS varchar(100)) AS numeroFactura,
        CAST(NULL AS varchar(100)) AS fechaEmision,
        CAST(NULL AS varchar(100)) AS fecahVensimiento,
        CAST(NULL AS varchar(max)) AS dataQR,
        CAST(NULL AS varchar(max)) AS imagenQR
    WHERE 1 = 0;
    RETURN;
END;

DECLARE @idNotaCredito int;
SELECT TOP 1 @idNotaCredito = id
FROM dbo.NotasCredito
WHERE idVenta = {notaCredito.idVenta.Value}
   OR cufe = '{cufe}';

IF @idNotaCredito IS NULL
BEGIN
    INSERT INTO dbo.NotasCredito
    (
        idVenta,
        cufe,
        numeroFactura,
        fechaEmision,
        fecahVensimiento,
        dataQR,
        imagenQR
    )
    VALUES
    (
        {notaCredito.idVenta.Value},
        '{cufe}',
        '{numeroFactura}',
        '{fechaEmision}',
        '{fechaVencimiento}',
        '{dataQr}',
        '{imagenQr}'
    );

    SET @idNotaCredito = SCOPE_IDENTITY();
END
ELSE
BEGIN
    UPDATE dbo.NotasCredito
    SET cufe = '{cufe}',
        numeroFactura = '{numeroFactura}',
        fechaEmision = '{fechaEmision}',
        fecahVensimiento = '{fechaVencimiento}',
        dataQR = '{dataQr}',
        imagenQR = '{imagenQr}'
    WHERE id = @idNotaCredito;
END;

SELECT TOP 1
    id,
    idVenta,
    ISNULL(cufe, '') AS cufe,
    ISNULL(numeroFactura, '') AS numeroFactura,
    ISNULL(fechaEmision, '') AS fechaEmision,
    ISNULL(fecahVensimiento, '') AS fecahVensimiento,
    ISNULL(dataQR, '') AS dataQR,
    ISNULL(imagenQR, '') AS imagenQR
FROM dbo.NotasCredito
WHERE id = @idNotaCredito;";

                var cn = new SqlAutoDAL();
                return await cn.EjecutarSQLObjeto<NotasCredito>(db, sql);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        private static string EscapeSql(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }
    }
}
