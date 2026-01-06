using DAL;           // SqlAutoDAL
using DAL.Model;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class V_CuentaClienteCotroler
    {
        /// <summary>
        /// Lista cuentas cliente filtradas por eliminada y opcionalmente por IdVenta.
        /// Si IdVenta > 0:
        ///   SELECT * FROM V_CuentaCliente WHERE eliminada = @eliminada AND idVenta = @IdVenta
        /// Si IdVenta <= 0:
        ///   SELECT * FROM V_CuentaCliente WHERE eliminada = @eliminada
        /// </summary>
        public static async Task<List<V_CuentaCliente>> Lista(string db, bool eliminada, [Optional] int IdVenta)
        {
            try
            {
                var cn = new SqlAutoDAL();

                if (IdVenta > 0)
                {
                    // filtrando por eliminada + idVenta
                    return await cn.ConsultarLista<V_CuentaCliente>(
                        db,
                        x => x.eliminada == eliminada && x.idVenta == IdVenta
                    );
                }
                else
                {
                    // solo por eliminada
                    return await cn.ConsultarLista<V_CuentaCliente>(
                        db,
                        x => x.eliminada == eliminada
                    );
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Consulta una cuenta cliente por id.
        /// Equivale a: SELECT TOP 1 * FROM V_CuentaCliente WHERE id = @id
        /// </summary>
        public static async Task<V_CuentaCliente> Consultar(string db, int id)
        {
            try
            {
                var cn = new SqlAutoDAL();

                // SELECT TOP 1 * FROM V_CuentaCliente WHERE id = {id}
                var cuenta = await cn.ConsultarUno<V_CuentaCliente>(
                    db,
                    x => x.id == id
                );

                return cuenta; // puede ser null si no existe
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
