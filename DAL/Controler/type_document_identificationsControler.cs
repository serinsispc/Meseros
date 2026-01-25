using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class type_document_identificationsControler
    {
        public static async Task<List<type_document_identifications>> ListaTiposDocumento(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var resp = await cn.ConsultarLista<type_document_identifications>(db, x => x.scope == "1");
                return resp.ToList();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new List<type_document_identifications>();
            }
        }
    }
}
