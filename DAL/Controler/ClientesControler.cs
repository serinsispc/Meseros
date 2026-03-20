using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class ClientesControler
    {
        public static async Task<Respuesta_DAL> CRUD(string db, Clientes cliente, int funcion)
        {
            try
            {
                var helper = new CrudSpHelper();
                var resp = await helper.CrudAsync(db, cliente, funcion);
                return resp ?? new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Sin respuesta del servidor en CRUD_Clientes."
                };
            }
            catch (Exception ex)
            {
                return new Respuesta_DAL
                {
                    data = 0,
                    estado = false,
                    mensaje = "Error en CRUD_Clientes: " + ex.Message
                };
            }
        }

        public static async Task<List<Clientes>> ListaClientes(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarLista<Clientes>(db);
                return resp.ToList();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new List<Clientes>();
            }
        }

        public static async Task<Clientes> Consultar_id(string db, int id)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarUno<Clientes>(db, x => x.id == id);
                return resp;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        public static async Task<Clientes> Consultar_nit(string db, string nit)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarUno<Clientes>(db, x => x.identificationNumber == nit);
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
