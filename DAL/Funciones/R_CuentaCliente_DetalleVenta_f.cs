using DAL.Controler;
using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Funciones
{
    public class R_CuentaCliente_DetalleVenta_f
    {
        public static async Task<Respuesta_DAL> Insert(string db,int idcuenta, int iddetalle)
        {
            try
            {
                int boton = 0;
                var relacion =await R_CuentaCliente_DetalleVentaControler.Consultar_idDetalle(db, iddetalle);
                if (relacion == null)
                {
                    relacion = new R_CuentaCliente_DetalleVenta();
                    relacion.id = 0;
                }
                else
                {
                    boton = 1;
                }
                relacion.fecha=DateTime.Now;
                relacion.idCuentaCliente= idcuenta;
                relacion.idDetalleVenta= iddetalle;
                relacion.eliminada = false;
                var crud =await R_CuentaCliente_DetalleVentaControler.CRUD(db, relacion,boton);
                return new Respuesta_DAL { mensaje=$"cuenta {idcuenta} y detalle {iddetalle}, relacionados con éxito.", estado=crud.estado, data=crud.data };
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return new Respuesta_DAL { data=null, estado=false, mensaje=$"No se pudo anclar la cuenta {idcuenta} con el detalle {iddetalle}" };
            }
        }
    }
}
