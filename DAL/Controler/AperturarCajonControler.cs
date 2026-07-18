using DAL.Model;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class AperturarCajonControler
    {
        public static async Task<bool> CRUD(string db, AperturarCajon tabla, int funcion)
        {
            try
            {
                var helper = new CrudSpHelper();
                var resp = await helper.CrudAsync(db, tabla, funcion);
                var respcrud = new RespuestaCRUD
                {
                    estado = resp.estado,
                    idAfectado = resp.data,
                    mensaje = resp.mensaje,
                    nuevoId = resp.data
                };

                if (respcrud.estado)
                {
                    await CompletarDatosAperturaCajonAsync(db, tabla, funcion, respcrud);
                }

                return respcrud.estado;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return false;
            }
        }

        private static async Task CompletarDatosAperturaCajonAsync(string db, AperturarCajon tabla, int funcion, RespuestaCRUD respuesta)
        {
            if (funcion != 0 || tabla == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(tabla.nameprinter) && tabla.ancho <= 0)
            {
                return;
            }

            int idRegistro;
            if (!int.TryParse(Convert.ToString(respuesta?.IdFinal), out idRegistro) || idRegistro <= 0)
            {
                return;
            }

            var impresoraSql = string.IsNullOrWhiteSpace(tabla.nameprinter)
                ? "NULL"
                : "N'" + tabla.nameprinter.Trim().Replace("'", "''") + "'";

            var anchoSql = tabla.ancho > 0
                ? tabla.ancho.ToString(CultureInfo.InvariantCulture)
                : "NULL";

            var sql = string.Format(
                "UPDATE dbo.AperturarCajon SET nameprinter = {0}, ancho = {1} WHERE id = {2}",
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
