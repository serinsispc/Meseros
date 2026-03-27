using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class AperturarCajonControler
    {
        public static async Task<bool> CRUD(string db, AperturarCajon tabla,int funcion)
        {
            try
            {
                var helper = new CrudSpHelper();
                var resp = await helper.CrudAsync(db, tabla, funcion);
                var respcrud = new RespuestaCRUD { estado = resp.estado, idAfectado = resp.data, mensaje = resp.mensaje, nuevoId = resp.data };
                return respcrud.estado;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return false;
            }
        }
    }
}
