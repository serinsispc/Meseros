using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public class ZonasControler
    {
        public static async Task<List<Zonas>> Lista(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                var sql = @"
SELECT *
FROM Zonas
ORDER BY id;";
                return await cn.EjecutarSQLLista<Zonas>(db, sql);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }
    }
}
