using DAL;          // <-- para SqlAutoDAL
using DAL.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class VendedorControler
    {
        public static async Task<Vendedor> Consultar_usuario_clave(string db, string usuario, string clave)
        {
            try
            {
                var auto = new SqlAutoDAL();
                var vendedor = await auto.ConsultarUno<Vendedor>(
                    db,
                    x => x.telefonoVendedor == usuario && x.calveVendedor == clave
                );

                return vendedor; 
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        public static async Task<List<Vendedor>>ListaVendedor(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarLista<Vendedor>(db);
                return resp;
            }
            catch(Exception ex)
            {
                string error = ex.Message;
                return new List<Vendedor>();
            }
        }
    }
}
