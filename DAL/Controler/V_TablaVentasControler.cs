using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_TablaVentasControler
    {
        public static async Task<List<V_TablaVentas>> Lista(string db, int idbase)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var sql = $@"
SELECT id, fechaVenta, aliasVenta, tipoFactura, prefijo, numeroVenta, descuentoVenta, idMedioDePago,
       idResolucion, idFormaDePago, subtotalVenta, basesIva, basesIva_5, basesIva_19, IVA, IVA_5, IVA_19,
       INC, INCBolsas, otrosImpuestos, ivaVenta, totalVenta, total_A_Pagar, efectivoVenta, cambioVenta,
       formaDePago, abonoEfectivo, abonoTarjeta, totalPagadoVenta, totalPendienteVenta, estadoVenta,
       medioDePago, numeroReferenciaPago, diasCredito, fechaVencimiento, observacionVenta, IdSede, guidVenta,
       costoTotalVenta, utilidadTotalVenta, idCliente, nit, nombreCliente, propina, cufe, estadoFE, imagenQR,
       idBaseCaja, razonDescuento, por_propina, eliminada
FROM V_TablaVentas
WHERE idBaseCaja = {idbase}
ORDER BY id DESC;";
                return await cn.EjecutarSQLLista<V_TablaVentas>(db, sql);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
        public static async Task<V_TablaVentas> Consultar_Id(string db,int idventa)
        {
            try
            {
                var cn = new SqlAutoDAL();
                return await cn.ConsultarUno<V_TablaVentas>(db, x=>x.id==idventa);
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
