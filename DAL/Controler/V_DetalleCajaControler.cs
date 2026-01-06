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

                if (idCuenta > 0)
                {
                    // WHERE idVenta = @idVenta AND idCuentaCliente = @idCuenta AND estadoDetalle = 1
                    return await auto.ConsultarLista<V_DetalleCaja>(
                        db,
                        x => x.idVenta == idVenta &&
                             x.idCuentaCliente == idCuenta &&
                             x.estadoDetalle == 1
                    );
                }
                else
                {
                    // WHERE idVenta = @idVenta AND estadoDetalle = 1
                    return await auto.ConsultarLista<V_DetalleCaja>(
                        db,
                        x => x.idVenta == idVenta &&
                             x.estadoDetalle == 1
                    );
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
