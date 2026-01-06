using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication
{
    public partial class Salir : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var db = Session["db"].ToString();
            // Limpia toda la sesión
            Session.Clear();
            Session.Abandon();

            // Redirige a Default.aspx con el parámetro db
            Response.Redirect($"Default.aspx?db={db}");
        }
    }
}