using DAL.Controler;
using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Funciones
{
    public class R_VentaVendedor_f
    {
        public static async Task<bool> Relacionar_Vendedor_Venta(string db,int idventa,int idvendedor)
        {
            try
            {
                int idr = 0;
                int funcion = 0;
                //primero consultamos si ya la venta tiene un vendedor
                var rvv =await R_VentaVendedorControler.Consultar_idventa(db,idventa);
                if (rvv!=null)
                {
                    funcion = 1;
                }
                else
                {
                    rvv = new R_VentaVendedor();
                    rvv.id = 0;
                }
                rvv.idVenta = idventa;
                rvv.idVendedor = idvendedor;
                var rrr= await R_VentaVendedorControler.CRUD(db,rvv,funcion);
                return rrr.estado;
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return false;
            }
        }
    }
}
