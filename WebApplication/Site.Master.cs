using System;
using System.IO;
using System.Web.UI;

namespace WebApplication
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["db"] != null)
            {
                string db = Session["db"].ToString();
                string origen = Server.MapPath($"~/Recursos/Imagenes/Logo/{db}.png");
                string destino = Server.MapPath($"~/Recursos/Imagenes/Logo/favicon-{db}.png");

                if (File.Exists(origen))
                {
                    File.Copy(origen, destino, true);
                }
            }
        }
    }
}