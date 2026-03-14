using DAL.Model;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class InformePagoInternoTurnoControler
    {
        public static async Task<List<InformePagoInternoTurnoItem>> Lista(string db, int idBaseCaja)
        {
            var lista = new List<InformePagoInternoTurnoItem>();

            if (string.IsNullOrWhiteSpace(db) || idBaseCaja <= 0)
            {
                return lista;
            }

            using (var cn = new Conection_SQL(db))
            {
                var sql = $"exec InformePagoInterno_Turno {idBaseCaja}";
                var json = await cn.EjecutarConsulta(sql, true);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return lista;
                }

                var array = JArray.Parse(json);
                foreach (var token in array)
                {
                    if (!(token is JObject row))
                    {
                        continue;
                    }

                    lista.Add(new InformePagoInternoTurnoItem
                    {
                        id = row.Value<int?>("id") ?? 0,
                        nombreMPI = row.Value<string>("nombreMPI") ?? string.Empty,
                        estado = row.Value<int?>("estado") ?? 0,
                        reporteDIAN = row.Value<int?>("reporteDIAN") ?? 0,
                        total = row.Value<decimal?>("total") ?? 0m
                    });
                }
            }

            return lista;
        }
    }
}
