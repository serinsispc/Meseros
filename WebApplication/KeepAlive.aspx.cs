using System;
using System.Web.UI;
using WebApplication.Helpers;

namespace WebApplication
{
    public partial class KeepAlive : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Clear();
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            if (Session != null)
            {
                Session[SessionContextHelper.AdminControlReminderAtKey] =
                    Session[SessionContextHelper.AdminControlReminderAtKey] ?? DateTime.UtcNow.ToString("O");
            }

            Response.StatusCode = 204;
            Response.SuppressContent = true;
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
