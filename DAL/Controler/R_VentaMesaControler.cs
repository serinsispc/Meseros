using DAL;          // CrudSpHelper, SqlAutoDAL
using DAL.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace DAL.Controler
{
    public class R_VentaMesaControler
    {
        /// <summary>
        /// CRUD para R_VentaMesa usando SP:
        /// EXEC CRUD_R_VentaMesa @json, @funcion
        /// funcion: 0 = INSERT, 1 = UPDATE, 2 = DELETE
        /// </summary>
        public static async Task<Respuesta_DAL> CRUD(string db, R_VentaMesa rvm, int funcion)
        {
            try
            {
                var helper = new CrudSpHelper();

                // Llama al helper genérico:
                // EXEC [dbo].[CRUD_R_VentaMesa] @json = N'...', @funcion = {funcion}
                var resp = await helper.CrudAsync(db, rvm, funcion);
                if (funcion == 2 && rvm != null)
                {
                    return await ResolverEliminacionRelacion(db, rvm, resp);
                }

                return resp ?? new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Sin respuesta del servidor en CRUD_R_VentaMesa."
                };
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Error en CRUD_R_VentaMesa: " + msg
                };
            }
        }

        private static async Task<Respuesta_DAL> ResolverEliminacionRelacion(string db, R_VentaMesa rvm, Respuesta_DAL respSp)
        {
            var relacionSigueExistiendo = await ExisteRelacionAsync(db, rvm);
            if (respSp != null && respSp.estado && !relacionSigueExistiendo)
            {
                return respSp;
            }

            var auto = new SqlAutoDAL();
            var filtro = rvm.id > 0
                ? $"id = {rvm.id}"
                : $"idVenta = {rvm.idVenta} AND idMesa = {rvm.idMesa}";

            var sql = $@"
DELETE FROM dbo.R_VentaMesa
WHERE {filtro};

SELECT
    CAST(CASE WHEN EXISTS (SELECT 1 FROM dbo.R_VentaMesa WHERE {filtro}) THEN 0 ELSE 1 END AS bit) AS estado,
    CASE WHEN EXISTS (SELECT 1 FROM dbo.R_VentaMesa WHERE {filtro})
        THEN 'No fue posible eliminar la relación en R_VentaMesa.'
        ELSE 'Relación eliminada correctamente.'
    END AS mensaje,
    {(rvm.id > 0 ? rvm.id : rvm.idVenta)} AS data;";

            var respDirecto = await auto.EjecutarSQLObjeto<Respuesta_DAL>(db, sql);
            if (respDirecto != null)
            {
                return respDirecto;
            }

            return respSp ?? new Respuesta_DAL
            {
                data = 0,
                estado = false,
                mensaje = "No fue posible eliminar la relación en R_VentaMesa."
            };
        }

        private static async Task<bool> ExisteRelacionAsync(string db, R_VentaMesa rvm)
        {
            if (rvm == null)
            {
                return false;
            }

            var auto = new SqlAutoDAL();
            if (rvm.id > 0)
            {
                var relacionPorId = await auto.ConsultarUno<R_VentaMesa>(db, x => x.id == rvm.id);
                return relacionPorId != null;
            }

            var relacion = await auto.ConsultarUno<R_VentaMesa>(db, x => x.idVenta == rvm.idVenta && x.idMesa == rvm.idMesa);
            return relacion != null;
        }

        /// <summary>
        /// Consulta la relación por idVenta e idMesa.
        /// Equivale a:
        /// SELECT TOP 1 * FROM R_VentaMesa WHERE idVenta = @idventa AND idMesa = @idmesa
        /// </summary>
        public static async Task<R_VentaMesa> Consultar_relacion(string db, int idventa, int idmesa)
        {
            try
            {
                var auto = new SqlAutoDAL();

                // Genera y ejecuta:
                // SELECT TOP 1 * FROM R_VentaMesa WHERE idVenta = idventa AND idMesa = idmesa
                var relacion = await auto.ConsultarUno<R_VentaMesa>(
                    db,
                    x => x.idVenta == idventa && x.idMesa == idmesa
                );

                return relacion; // puede ser null si no existe
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return null;
            }
        }

        public static async Task<List<R_VentaMesa>> ListaRelacion(string db, int idventa)
        {
            try
            {
                var auto = new SqlAutoDAL();
                var sql = $"SELECT id, idVenta, idMesa FROM R_VentaMesa WHERE idVenta = {idventa} ORDER BY id;";
                var relacion = await auto.EjecutarSQLLista<R_VentaMesa>(db, sql);
                return relacion; // puede ser null si no existe
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return null;
            }
        }

        public static async Task<List<R_VentaMesa>> ListaPorMesa(string db, int idmesa)
        {
            try
            {
                var auto = new SqlAutoDAL();
                var sql = $"SELECT id, idVenta, idMesa FROM R_VentaMesa WHERE idMesa = {idmesa} ORDER BY id;";
                var relaciones = await auto.EjecutarSQLLista<R_VentaMesa>(db, sql);
                return relaciones;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return null;
            }
        }
    }
}
