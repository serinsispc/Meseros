using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class CargoDescuentoVentasControler
    {
        public static async Task<bool> CRUD(string db,CargoDescuentoVentas descuentoVentas, int funcion)
        {
            try
            {
                var helper = new CrudSpHelper();
                var resp=await helper.CrudAsync(db, descuentoVentas, funcion);
                var respcrud = new RespuestaCRUD { estado = resp.estado, idAfectado = resp.data, mensaje = resp.mensaje, nuevoId = resp.data };
                return respcrud.estado;
            }
            catch(Exception ex)
            {
                string error = ex.Message;
                return false;
            }
        }

        public static async Task<RespuestaCRUD> EliminarPorVenta(string db, int idVenta)
        {
            using (var cn = new Conection_SQL(db))
            {
                // OJO: como idVenta es int, no lleva comillas
                string sql = $"EXEC dbo.DELETE_CargoDescuentoVentas {idVenta};";

                // false => objeto único (primer registro del SELECT del SP)
                string json = await cn.EjecutarConsulta(sql, false);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return new RespuestaCRUD
                    {
                        estado = false,
                        idAfectado = idVenta.ToString(),
                        mensaje = "No se recibió respuesta del servidor."
                    };
                }

                return JsonConvert.DeserializeObject<RespuestaCRUD>(json);
            }
        }
        public static async Task<List<CargoDescuentoVentas>> ObtenerPorVenta(string db, int idVenta)
        {
            using (var cn = new Conection_SQL(db))
            {
                // OJO: como idVenta es int, no lleva comillas
                string sql = $"select *from CargoDescuentoVentas where idVenta={idVenta}";
                // true => lista de objetos
                string json = await cn.EjecutarConsulta(sql, true);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<CargoDescuentoVentas>();
                }
                return JsonConvert.DeserializeObject<List<CargoDescuentoVentas>>(json);
            }
        }
    }
}
