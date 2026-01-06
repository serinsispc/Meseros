using DAL.Controler;
using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Funciones
{
    public class R_VentaMesa_f
    {
        public static async Task<bool> Relacionar_Venta_Mesa(string db, int idventa,int idmesa)
        {
            try
            {
                var rvm = new R_VentaMesa();
                rvm.id = 0;
                rvm.idVenta = idventa;
                rvm.idMesa = idmesa;
                var resp =await R_VentaMesaControler.CRUD(db,rvm,0);
                return resp.estado;
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return false;
            }
        }
    }
}
