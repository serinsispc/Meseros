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
            string db;
            if (Session["db"] != null)
            {
                db = Session["db"].ToString();
            }
            else
            {
                db = "-";
            }
            // Limpia toda la sesión
            Session.Clear();
            Session.Abandon();

            // Redirige a Default.aspx con el parámetro db
            Response.Redirect($"Default.aspx?db={db}");
        }
    }
}