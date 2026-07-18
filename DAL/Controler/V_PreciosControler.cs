using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_PreciosControler
    {
        public static async Task<List<V_Precios>> ListaPorPresentaciones(string db, IEnumerable<int> idsPresentacion)
        {
            try
            {
                var ids = (idsPresentacion ?? Enumerable.Empty<int>())
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                if (!ids.Any())
                {
                    return new List<V_Precios>();
                }

                var idsSql = string.Join(",", ids);
                var sql = $"SELECT id, idPresentacion, ISNULL(nombrePrecio,'') AS nombrePrecio, ISNULL(valorPrecio,0) AS valorPrecio FROM V_Precios WHERE idPresentacion IN ({idsSql}) ORDER BY idPresentacion, nombrePrecio, id";

                using (var cn = new Conection_SQL(db))
                {
                    var json = await cn.EjecutarConsulta(sql, true);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return new List<V_Precios>();
                    }

                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<V_Precios>>(json) ?? new List<V_Precios>();
                }
            }
            catch
            {
                return new List<V_Precios>();
            }
        }

        public static async Task<List<V_Precios>> ListaPorPresentacion(string db, int idPresentacion)
        {
            return await ListaPorPresentaciones(db, new[] { idPresentacion });
        }
    }
}
