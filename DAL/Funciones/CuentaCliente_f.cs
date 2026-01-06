using DAL.Controler;
using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Funciones
{
    public class CuentaCliente_f
    {
        public static async Task<int> Crear(string db, int idventa,string nombre,int porpro)
        {
            try
            {
                var cuenta = new CuentaCliente { 
                    id=0,
                    fecha=DateTime.Now,
                 idVenta=idventa,
                 nombreCuenta=nombre,
                 preCuenta=false,
                 eliminada=false,
                 por_propina= Convert.ToDecimal(porpro) / 100,
                 propina=0
                };
                var respuesta =await CuentaClienteControler.CRUD(db,cuenta,0);
                return Convert.ToInt32(respuesta.data);
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return 0;
            }
        }
        public static async Task<bool> Editar(string db, int id, string nombre)
        {
            try
            {
                var cuenta =await CuentaClienteControler.CuentaCliente(db, id); 
                if (cuenta == null)
                {
                    return false;
                }
                cuenta.nombreCuenta= nombre;
                var respuesta =await CuentaClienteControler.CRUD(db, cuenta, 1);
                return respuesta.estado;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return false;
            }
        }
    }
}
