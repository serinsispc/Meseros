using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_CuentasVentaControler
    {
        public static async Task<List<V_CuentasVenta>> Lista_IdVendedor(string db, int idvendedor)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var sql = $@"
SELECT id, aliasVenta, efectivoVenta, numeroVenta, eliminada, total, idbase, idusuario, numbreUnuario,
       idvendedor, nombrevendedor, idcliente, nombrecliente, idVehiculo, placa, responsable,
       telefonoResponsable, nombremesa, nombreCD
FROM V_CuentasVenta
WHERE idvendedor = {idvendedor}
  AND numeroVenta = 0
  AND eliminada = 0
ORDER BY id DESC;";
                var resp = await cn.EjecutarSQLLista<V_CuentasVenta>(db, sql);
                return resp;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        public static async Task<List<V_CuentasVenta>> Lista_Cajero(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var sql = @"
SELECT id, aliasVenta, efectivoVenta, numeroVenta, eliminada, total, idbase, idusuario, numbreUnuario,
       idvendedor, nombrevendedor, idcliente, nombrecliente, idVehiculo, placa, responsable,
       telefonoResponsable, nombremesa, nombreCD
FROM V_CuentasVenta
WHERE numeroVenta = 0
  AND eliminada = 0
ORDER BY id DESC;";
                var resp = await cn.EjecutarSQLLista<V_CuentasVenta>(db, sql);
                return resp;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        public static async Task<V_CuentasVenta> Consultar_Id(string db, int idVenta)
        {
            try
            {
                var cn = new SqlAutoDAL();
                return await cn.ConsultarUno<V_CuentasVenta>(db, x => x.id == idVenta);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}

