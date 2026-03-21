using System;
using System.Globalization;
using System.Threading;
using System.Web;
using WebApplication.Helpers;

namespace WebApplication
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
        }

        void Session_Start(object sender, EventArgs e)
        {
            SessionContextHelper.ClearOperationalContext(Session);
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var culture = new CultureInfo("es-CO");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}