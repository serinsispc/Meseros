using DAL.Model;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.SessionState;
using WebApplication.ViewModels;

namespace WebApplication.Helpers
{
    public static class AperturarCajonRequestHelper
    {
        public static Task<bool> EnviarAsync(string db, HttpSessionState session, MenuViewModels model = null)
        {
            return EnviarInternalAsync(db, session, model);
        }

        private static async Task<bool> EnviarInternalAsync(string db, HttpSessionState session, MenuViewModels model = null)
        {
            await PuntoDePagoPrinterHelper.ResolveSelectedPuntoDePagoAsync(db, session, model);

            var cajon = new AperturarCajon
            {
                estado = true
            };

            PuntoDePagoPrinterHelper.Apply(cajon, session, model);

            var impresoraSql = string.IsNullOrWhiteSpace(cajon.nameprinter)
                ? "NULL"
                : "N'" + cajon.nameprinter.Trim().Replace("'", "''") + "'";

            var anchoSql = cajon.ancho > 0
                ? cajon.ancho.ToString(CultureInfo.InvariantCulture)
                : "NULL";

            var estadoSql = cajon.estado ? "1" : "0";
            var sql = string.Format(
                "INSERT INTO dbo.AperturarCajon (estado, nameprinter, ancho) VALUES ({0}, {1}, {2}); SELECT CAST(SCOPE_IDENTITY() AS int) AS id;",
                estadoSql,
                impresoraSql,
                anchoSql);

            using (var conexion = new DAL.Conection_SQL(db))
            {
                var resultado = await conexion.EjecutarConsulta(sql, false);
                return !string.IsNullOrWhiteSpace(resultado);
            }
        }
    }
}
