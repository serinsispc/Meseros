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
        public static async Task<List<Clientes>>ListaClientes(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarLista<Clientes>(db);
                return resp.ToList();
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return new List<Clientes>();
            }
        }
        public static async Task<Clientes> Consultar_id(string db,int id)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarUno<Clientes>(db, x=>x.id==id);
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
