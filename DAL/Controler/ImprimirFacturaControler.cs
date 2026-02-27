using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class ImprimirFacturaControler
    {
        public static async Task<RespuestaCRUD> CRUD(string db,ImprimirFactura imprimir,int funcion)
        {
            try
            {
                var helper = new CrudSpHelper();
                var resp = await helper.CrudAsync(db, imprimir, funcion);
                var respcrud = new RespuestaCRUD { estado = resp.estado, idAfectado = resp.data, mensaje = resp.mensaje, nuevoId = resp.data };
                return respcrud;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return new RespuestaCRUD { nuevoId="0", mensaje=error, idAfectado="0", estado=false };
            }
        }
    }
}
