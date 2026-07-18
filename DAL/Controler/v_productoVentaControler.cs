using DAL;
using DAL.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class v_productoVentaControler
    {
        /// <summary>
        /// Lista todos los productos de venta activos (estadoProducto = 1).
        /// Equivale a:
        /// SELECT * FROM v_productoVenta WHERE estadoProducto = 1
        /// </summary>
        public static async Task<List<v_productoVenta>> Lista(string db)
        {
            try
            {
                var auto = new SqlAutoDAL();
                var sql = @"
SELECT id, idTipoMedida, idCategoria, impuesto_id, nombreCategoria, visible, guidProducto, codigoProducto,
       nombreProducto, descripcionProducto, letraTipoMedida, nombreTipoPresentacion, costo_mas_impuesto,
       porcentaje_impuesto, precioVenta, inventarioActual, idPresentacion, idTipoPresentacion, insumo,
       contenidoPresentacion, estadoInventario, estadoProducto, estadoPresentacion, gramera, inventarioMinimo,
       inventarioInicial
FROM v_productoVenta
WHERE estadoProducto = 1
ORDER BY idCategoria, nombreProducto, idPresentacion;";
                return await auto.EjecutarSQLLista<v_productoVenta>(db, sql);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Consulta un producto de venta por idPresentacion.
        /// Equivale a:
        /// SELECT TOP 1 * FROM v_productoVenta WHERE idPresentacion = @idpresentacion
        /// </summary>
        public static async Task<v_productoVenta> Consultar_idpresentacion(string db, int idpresentacion)
        {
            try
            {
                var auto = new SqlAutoDAL();

                return await auto.ConsultarUno<v_productoVenta>(
                    db,
                    x => x.idPresentacion == idpresentacion
                );
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
