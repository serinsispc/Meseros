using DAL;
using DAL.Controler;
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication.Class;

namespace WebApplication
{
    public partial class SiteMaster : MasterPage
    {


        protected void  Page_Load(object sender, EventArgs e)
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