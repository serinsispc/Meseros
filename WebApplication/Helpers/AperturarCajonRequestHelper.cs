using DAL.Controler;
using DAL.Model;
using System.Threading.Tasks;
using System.Web.SessionState;
using WebApplication.ViewModels;

namespace WebApplication.Helpers
{
    public static class AperturarCajonRequestHelper
    {
        public static Task<bool> EnviarAsync(string db, HttpSessionState session, MenuViewModels model = null)
        {
            var cajon = new AperturarCajon
            {
                estado = true
            };

            PuntoDePagoPrinterHelper.Apply(cajon, session, model);
            return AperturarCajonControler.CRUD(db, cajon, 0);
        }
    }
}
