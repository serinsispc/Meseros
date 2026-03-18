using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.UI;
using WebApplication.Helpers;

namespace WebApplication
{
    public partial class PGastos : System.Web.UI.Page
    {
        protected async void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }

            await CargarBolsillos();
        }

        private async Task CargarBolsillos()
        {
            var models = SessionContextHelper.LoadModels(Session);
            var db = Convert.ToString(Session[SessionContextHelper.DbKey] ?? models?.db);
            if (string.IsNullOrWhiteSpace(db))
            {
                return;
            }

            List<BolsilloItem> bolsillos;
            using (var cn = new Conection_SQL(db))
            {
                var json = await cn.EjecutarConsulta("select id, nombreBolsillo from Bolsillo order by nombreBolsillo", true);
                bolsillos = string.IsNullOrWhiteSpace(json)
                    ? new List<BolsilloItem>()
                    : JsonConvert.DeserializeObject<List<BolsilloItem>>(json) ?? new List<BolsilloItem>();
            }

            ddlBolsillo.DataSource = bolsillos;
            ddlBolsillo.DataTextField = "nombreBolsillo";
            ddlBolsillo.DataValueField = "id";
            ddlBolsillo.DataBind();
        }

        private class BolsilloItem
        {
            public int id { get; set; }
            public string nombreBolsillo { get; set; }
        }
    }
}