using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class BaseCajaControler
    {
        public static async Task<BaseCaja> VerificarBaseCaja(string db, int idusuario)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarUno<BaseCaja>(db, x => x.idUsuarioApertura == idusuario && x.estadoBase == "ACTIVA");
                return resp;
            }
            catch(Exception ex)
            {
                string mensaje = ex.Message;
                return null;
            }
        }

        public static async Task<BaseCaja> AperturarBase(string db, BaseCaja baseCaja, int funcion)
        {
            try
            {
                var helper = new CrudSpHelper();
                var resp = await helper.CrudAsync(db, baseCaja, funcion);
                var respcrud= new  RespuestaCRUD { estado = resp.estado, idAfectado = resp.data, mensaje = resp.mensaje, nuevoId = resp.data };
                if (respcrud.estado)
                {
                    // consultamos la nueba base aperturada
                    var cn = new SqlAutoDAL();
                    var baseNueva = await cn.ConsultarUno<BaseCaja>(db, x => x.id == Convert.ToInt32(respcrud.idAfectado));
                    return baseNueva;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                return null;
            }
        }

        public static async Task<bool> CRUD(string db, BaseCaja baseCaja, int funcion)
        {
            try
            {
                var helper = new CrudSpHelper();
                var resp = await helper.CrudAsync(db, baseCaja, funcion);
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
