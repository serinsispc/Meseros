using DAL.Model;
using System;
using System.Threading.Tasks;

namespace DAL.Controler
{
    public static class AdminControlControler
    {
        public static async Task<AdminControl> ConsultarAsync(string db)
        {
            try
            {
                var cn = new SqlAutoDAL();
                return await cn.ConsultarUno<AdminControl>(db, x => x.id_admincontrol > 0);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
