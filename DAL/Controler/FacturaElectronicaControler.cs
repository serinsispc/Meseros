using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class FacturaElectronicaControler
    {
        public static async Task<bool> Crud(string db,FacturaElectronica facturaElectronica, int funcion)
        {
            try 
            {
                var cn =new CrudSpHelper();
                var resp = await cn.CrudAsync(db,facturaElectronica,funcion);
                var respcrud = new RespuestaCRUD { estado = resp.estado, idAfectado = resp.data, mensaje = resp.mensaje, nuevoId = resp.data };
                return respcrud.estado;
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return false;
            }

        }
        public static async Task<FacturaElectronica> ConsultarCUfe(string db,string cufe)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarUno<FacturaElectronica>(db,x=>x.cufe==cufe);
                if(resp==null)return null;
                return resp;
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
