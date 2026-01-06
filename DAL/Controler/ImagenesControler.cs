using DAL.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class ImagenesControler
    {

        public static async Task<Imagenes> Consultar(string db, Guid guid)
        {
            try
            {
                var _autoSql = new SqlAutoDAL();
                var resp=await _autoSql.ConsultarUno<Imagenes>(db, x => x.id == guid);
                return resp;
            }
            catch
            {
                return null;
            }
        }

    }
}
