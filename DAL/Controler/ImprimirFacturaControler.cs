using DAL.Model;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class ImprimirFacturaControler
    {
        public static async Task<RespuestaCRUD> CRUD(string db, ImprimirFactura imprimir, int funcion)
        {
            try
            {
                var helper = new CrudSpHelper();
                var resp = await helper.CrudAsync(db, imprimir, funcion);
                var respcrud = new RespuestaCRUD
                {
                    estado = resp.estado,
                    idAfectado = resp.data,
                    mensaje = resp.mensaje,
                    nuevoId = resp.data
                };

                if (respcrud.estado)
                {
                    await CompletarDatosImpresionAsync(db, imprimir, funcion, respcrud);
                }

                return respcrud;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return new RespuestaCRUD { nuevoId = "0", mensaje = error, idAfectado = "0", estado = false };
            }
        }

        private static async Task CompletarDatosImpresionAsync(string db, ImprimirFactura imprimir, int funcion, RespuestaCRUD respuesta)
        {
            if (funcion != 0 || imprimir == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(imprimir.nameprinter) && imprimir.ancho <= 0)
            {
                return;
            }

            int idRegistro;
            if (!int.TryParse(Convert.ToString(respuesta?.IdFinal), out idRegistro) || idRegistro <= 0)
            {
                return;
            }

            var impresoraSql = string.IsNullOrWhiteSpace(imprimir.nameprinter)
                ? "NULL"
                : "N'" + imprimir.nameprinter.Trim().Replace("'", "''") + "'";

            var anchoSql = imprimir.ancho > 0
                ? imprimir.ancho.ToString(CultureInfo.InvariantCulture)
                : "NULL";

            var sql = string.Format(
                "UPDATE dbo.ImprimirFactura SET nameprinter = {0}, ancho = {1} WHERE id = {2}",
                impresoraSql,
                anchoSql,
                idRegistro.ToString(CultureInfo.InvariantCulture));

            using (var conexion = new Conection_SQL(db))
            {
                await conexion.EjecutarConsulta(sql, false);
            }
        }
    }
}
