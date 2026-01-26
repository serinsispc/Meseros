using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class R_VentaCliente_Controler
    {
        public static async Task<R_VentaCliente>ConsultarRelacion(string db,int idVenta)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarUno<R_VentaCliente>(db,x=>x.idVenta==idVenta);
                if (resp == null) return null;
                return resp;
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        public static async Task<bool> CRUD(string db,R_VentaCliente r_VentaCliente,int funcion)
        {
            try
            {
                var helper = new CrudSpHelper();
                var resp = await helper.CrudAsync(db, r_VentaCliente,funcion);
                var respcrud = new RespuestaCRUD { estado = resp.estado, idAfectado = resp.data, mensaje = resp.mensaje, nuevoId = resp.data };
                return respcrud.estado;
            }
            catch (Exception ex) 
            { 
                string msg = ex.Message;
                return false;
            }
        }
    }
}
