using DAL.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_DetalleCajaControler
    {
        public static async Task<List<V_DetalleCaja>> Lista_IdVenta(string db, int idVenta, int idCuenta)
        {
            try
            {
                var auto = new SqlAutoDAL();
                var filtroCuenta = idCuenta > 0 ? $" AND idCuentaCliente = {idCuenta}" : string.Empty;
                var sql = $@"
SELECT id, guidDetalle, idVenta, idPresentacion, codigoProducto, nombreProducto, impuesto_id, presentacion,
       unidad, descuentoDetalle, preVentaNeto, precioVenta, porImpuesto, baseImpuesto, valorImpuesto,
       subTotalDetalleNeto, subTotalDetalle, totalDetalle, costoUnidad, contenido, costoTotal, observacion,
       opciones, adiciones, estadoDetalle, idCategoria, idCuentaCliente, nombreCuenta, itemComandado
FROM V_DetalleCaja
WHERE idVenta = {idVenta}
  AND estadoDetalle = 1{filtroCuenta}
ORDER BY id;";

                return await auto.EjecutarSQLLista<V_DetalleCaja>(db, sql);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
