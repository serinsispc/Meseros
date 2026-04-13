using DAL;
using DAL.Controler;
using DAL.Funciones;
using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication.Class;
using WebApplication.Helpers;
using WebApplication.ViewModels;

namespace WebApplication
{
    public partial class Menu1 : System.Web.UI.MasterPage
    {
        protected bool SoloBotonCaja
        {
            get
            {
                var ruta = Page?.AppRelativeVirtualPath ?? string.Empty;
                return string.Equals(ruta, "~/HVentas.aspx", StringComparison.OrdinalIgnoreCase);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["db"] == null)
            {
                //Session["db"] = ClassConexionDinamica.db;
                Response.Redirect("Salir.aspx");
                return;
            }

            Page.RegisterAsyncTask(new PageAsyncTask(() => AdminControlAccessHelper.AplicarControlAsync(Page)));
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            var faviconUrl = BrandingImageHelper.ResolveFaviconUrl(Page);
            faviconLink.Visible = !string.IsNullOrWhiteSpace(faviconUrl);
            if (faviconLink.Visible)
            {
                faviconLink.Href = faviconUrl;
            }
        }


    }
}
