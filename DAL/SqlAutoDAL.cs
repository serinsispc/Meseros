using DAL.Helpers;           // donde está SqlAutoBuilder
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DAL
{
    public class SqlAutoDAL
    {
        // Un solo registro (TOP 1)
        public async Task<T> ConsultarUno<T>(string db, Expression<Func<T, bool>> where)
            where T : class, new()
        {
            if (where == null)
                throw new ArgumentNullException("where");

            // 1. Generar SQL dinámico
            string sql = SqlAutoBuilder.BuildSelect<T>(where, true); // TOP 1

            // 2. Ejecutar usando Conection_SQL
            using (var cn = new Conection_SQL(db))
            {
                string json = await cn.EjecutarConsulta(sql, false); // false = objeto único

                if (string.IsNullOrEmpty(json))
                    return null;

                T obj = JsonConvert.DeserializeObject<T>(json);
                return obj;
            }
        }

        // Lista de registros
        public async Task<List<T>> ConsultarLista<T>(string db, Expression<Func<T, bool>> where = null)
            where T : class, new()
        {
            // 1. Generar SQL dinámico
            string sql = SqlAutoBuilder.BuildSelect<T>(where, false); // sin TOP 1

            // 2. Ejecutar
            using (var cn = new Conection_SQL(db))
            {
                string json = await cn.EjecutarConsulta(sql, true); // true = lista

                if (string.IsNullOrEmpty(json))
                    return new List<T>();

                var lista = JsonConvert.DeserializeObject<List<T>>(json);
                return lista ?? new List<T>();
            }
        }
    }
}
