using DAL.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DAL
{
    public class SqlAutoDAL
    {
        public async Task<T> ConsultarUno<T>(string db)
            where T : class, new()
        {
            string sql = SqlAutoBuilder.BuildSelect<T>(null, true);

            using (var cn = new Conection_SQL(db))
            {
                string json = await cn.EjecutarConsulta(sql, false);

                if (string.IsNullOrWhiteSpace(json))
                    return null;

                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        public async Task<T> ConsultarUno<T>(string db, Expression<Func<T, bool>> where)
            where T : class, new()
        {
            if (where == null)
                throw new ArgumentNullException(nameof(where));

            string sql = SqlAutoBuilder.BuildSelect<T>(where, true);

            using (var cn = new Conection_SQL(db))
            {
                string json = await cn.EjecutarConsulta(sql, false);

                if (string.IsNullOrWhiteSpace(json))
                    return null;

                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        public async Task<List<T>> ConsultarLista<T>(string db, Expression<Func<T, bool>> where = null)
            where T : class, new()
        {
            string sql = SqlAutoBuilder.BuildSelect<T>(where, false);

            using (var cn = new Conection_SQL(db))
            {
                string json = await cn.EjecutarConsulta(sql, true);

                if (string.IsNullOrWhiteSpace(json))
                    return new List<T>();

                return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
            }
        }

        public async Task<T> EjecutarSQLObjeto<T>(string db, string sql) where T : class, new()
        {
            using (var cn = new Conection_SQL(db))
            {
                string json = await cn.EjecutarConsulta(sql, false);

                if (string.IsNullOrWhiteSpace(json))
                    return null;

                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        public async Task<T> EjecutarSPObjeto<T>(string db, string nombreSP, string json, int funcion) where T : class, new()
        {
            using (var cn = new Conection_SQL(db))
            {
                json = json.Replace("'", "''");
                string sql = $"EXEC [dbo].[{nombreSP}] N'{json}', {funcion}";

                string respJson = await cn.EjecutarConsulta(sql, false);

                if (string.IsNullOrWhiteSpace(respJson))
                    return null;

                return JsonConvert.DeserializeObject<T>(respJson);
            }
        }
    }
}