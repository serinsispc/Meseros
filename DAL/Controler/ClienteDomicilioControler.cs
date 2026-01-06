using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class ClienteDomicilioControler
    {
        public static async Task<List<ClienteDomicilio>>Lista(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                return await cn.ConsultarLista<ClienteDomicilio>(db);
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return new List<ClienteDomicilio>();
            }
        }

        public static async Task<RespuestaCRUD> CRUD(string db, ClienteDomicilio cliente, int funcion )
        {
            try
            {
                var helper = new CrudSpHelper();
                var resp = await helper.CrudAsync(db,cliente,funcion);
                return new RespuestaCRUD { estado=resp.estado, idAfectado=resp.data, mensaje=resp.mensaje, nuevoId=resp.data };
            }
            catch (Exception ex) 
            { 
                string error = ex.Message;
                return new RespuestaCRUD { estado=false, idAfectado="0", mensaje="error", nuevoId="0" };
            }
        }

        public static async Task<Respuesta_DAL> RelacionarConVenta(string db,R_VentaClienteDomicilio relacion,int funcion)
        {
            try
            {
                var helper = new CrudSpHelper();
                var resp= await helper.CrudAsync(db,relacion,funcion);
                return new Respuesta_DAL { data= resp.data, estado= resp.estado, mensaje= resp.mensaje };
            }
            catch(Exception ex)
            {
                string err = ex.Message;
                return new Respuesta_DAL { data = 1, estado = true, mensaje = "ok" }; ;
            }
        }

        public static async Task<R_VentaClienteDomicilio>ConsultarRelacion(string db, int idventa)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarUno<R_VentaClienteDomicilio>(db,x=>x.idVenta==idventa);
                return resp;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
