using DAL.Model;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class InformeProductoVendidoTurnoControler
    {
        public static async Task<List<InformeProductoVendidoTurnoItem>> Lista(string db, int idBaseCaja)
        {
            var lista = new List<InformeProductoVendidoTurnoItem>();

            if (string.IsNullOrWhiteSpace(db) || idBaseCaja <= 0)
            {
                return lista;
            }

            using (var cn = new Conection_SQL(db))
            {
                var json = await EjecutarConsultaProductosVendidos(cn, idBaseCaja);
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

                    lista.Add(new InformeProductoVendidoTurnoItem
                    {
                        id = row.Value<int?>("id") ?? 0,
                        nombreCategoria = row.Value<string>("nombreCategoria") ?? string.Empty,
                        codigoProducto = row.Value<string>("codigoProducto") ?? string.Empty,
                        nombreProducto = row.Value<string>("nombreProducto") ?? string.Empty,
                        idCategoria = row.Value<int?>("idCategoria") ?? 0,
                        cantidad = row.Value<decimal?>("cantidad") ?? 0m,
                        valor = row.Value<decimal?>("valor") ?? 0m
                    });
                }
            }

            return lista;
        }

        private static async Task<string> EjecutarConsultaProductosVendidos(Conection_SQL cn, int idBaseCaja)
        {
            try
            {
                return await cn.EjecutarConsulta($"exec InformeProductoVendidos {idBaseCaja}", true);
            }
            catch
            {
                return await cn.EjecutarConsulta($"exec InformeProductoVendido {idBaseCaja}", true);
            }
        }
    }
}
