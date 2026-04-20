using DAL;
using DAL.Controler;
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using WebApplication.Class;
using WebApplication.Helpers;
using WebApplication.ViewModels;

namespace WebApplication
{
    public partial class PGastos : System.Web.UI.Page
    {
        protected List<GastoVistaItem> ListaGastos = new List<GastoVistaItem>();

        private MenuViewModels models;
        private string db;
        private int idBaseCaja;
        private int idSede;

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!CargarContexto())
            {
                AlertModerno.ErrorRedirect(this, "Error", "La sesión expiró o no contiene el contexto de trabajo.", "Default.aspx");
                return;
            }

            if (!IsPostBack)
            {
                txtFechaGasto.Text = DateTime.Today.ToString("yyyy-MM-dd");
                txtDesde.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
                txtHasta.Text = DateTime.Today.ToString("yyyy-MM-dd");
                await CargarCombos();
                await CargarGastos();
            }
        }

        protected async void btnGuardarGasto_Click(object sender, EventArgs e)
        {
            if (!CargarContexto())
            {
                AlertModerno.ErrorRedirect(this, "Error", "La sesión expiró o no contiene el contexto de trabajo.", "Default.aspx");
                return;
            }

            await CargarCombos();
            RestaurarSeleccionCombo(ddlBolsillo);
            RestaurarSeleccionCombo(ddlTipoGasto);

            if (!DateTime.TryParse(txtFechaGasto.Text, out var fecha))
            {
                AlertModerno.Warning(this, "Atención", "Debe seleccionar una fecha válida.", true, 1800);
                await CargarGastos();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtConcepto.Text))
            {
                AlertModerno.Warning(this, "Atención", "Debe ingresar el concepto del gasto.", true, 1800);
                await CargarGastos();
                return;
            }

            if (!decimal.TryParse((txtValor.Text ?? string.Empty).Replace(".", string.Empty).Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var valor) || valor <= 0)
            {
                AlertModerno.Warning(this, "Atención", "Debe ingresar un valor válido mayor a cero.", true, 1800);
                await CargarGastos();
                return;
            }

            if (!int.TryParse(ddlBolsillo.SelectedValue, out var idBolsillo) || idBolsillo <= 0)
            {
                AlertModerno.Warning(this, "Atención", "Debe seleccionar un bolsillo.", true, 1800);
                await CargarGastos();
                return;
            }

            if (!int.TryParse(ddlTipoGasto.SelectedValue, out var idTipoGasto) || idTipoGasto <= 0)
            {
                AlertModerno.Warning(this, "Atención", "Debe seleccionar un tipo de gasto.", true, 1800);
                await CargarGastos();
                return;
            }

            var gasto = new Gastos
            {
                id = ParseInt(hfIdGasto.Value),
                fecha = fecha,
                concepto = txtConcepto.Text.Trim(),
                valor = valor,
                idBolsillo = idBolsillo,
                idBaseCaja = idBaseCaja,
                idTipoGasto = idTipoGasto,
                idSede = idSede
            };
            var funcion = gasto.id > 0 ? 1 : 0;
            var resp = await EjecutarCrudGastoAsync(gasto, funcion);
            if (!resp.estado)
            {
                AlertModerno.Error(this, "Error", resp.mensaje ?? "No fue posible guardar el gasto.", true, 2200);
                await CargarGastos();
                return;
            }

            var cajon = new AperturarCajon() { estado = true };
            PuntoDePagoPrinterHelper.Apply(cajon, Session, models);
            var respCajon = await AperturarCajonControler.CRUD(db, cajon, 0);

            LimpiarFormularioGasto();
            //wait CargarCombos();
            await CargarGastos();
            AlertModerno.Success(this, "OK", resp.mensaje ?? "Gasto guardado correctamente.", true, 1000);
        }

        protected async void btnEliminarGasto_Click(object sender, EventArgs e)
        {
            if (!CargarContexto())
            {
                AlertModerno.ErrorRedirect(this, "Error", "La sesión expiró o no contiene el contexto de trabajo.", "Default.aspx");
                return;
            }

            var id = ParseInt(hfIdEliminar.Value);
            if (id <= 0)
            {
                AlertModerno.Warning(this, "Atención", "No se recibió un gasto válido para eliminar.", true, 1800);
                await CargarGastos();
                return;
            }
            var resp = await EjecutarCrudGastoAsync(new Gastos { id = id }, 2);
            if (!resp.estado)
            {
                AlertModerno.Error(this, "Error", resp.mensaje ?? "No fue posible eliminar el gasto.", true, 2200);
                await CargarGastos();
                return;
            }

            await CargarCombos();
            await CargarGastos();
            AlertModerno.Success(this, "OK", resp.mensaje ?? "Gasto eliminado correctamente.", true, 1000);
        }

        protected async void btnFiltrar_Click(object sender, EventArgs e)
        {
            if (!CargarContexto())
            {
                AlertModerno.ErrorRedirect(this, "Error", "La sesión expiró o no contiene el contexto de trabajo.", "Default.aspx");
                return;
            }

            await CargarCombos();
            await CargarGastos();
        }

        protected async void btnLimpiar_Click(object sender, EventArgs e)
        {
            if (!CargarContexto())
            {
                AlertModerno.ErrorRedirect(this, "Error", "La sesión expiró o no contiene el contexto de trabajo.", "Default.aspx");
                return;
            }

            txtBuscar.Text = string.Empty;
            txtDesde.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
            txtHasta.Text = DateTime.Today.ToString("yyyy-MM-dd");
            await CargarCombos();
            await CargarGastos();
        }
        private bool CargarContexto()
        {
            models = SessionContextHelper.LoadModels(Session) ?? new MenuViewModels();
            db = Convert.ToString(Session[SessionContextHelper.DbKey] ?? models?.db);
            idBaseCaja = SessionContextHelper.ResolveBaseCajaId(Session, models);
            idSede = models?.Sede?.id ?? models?.BaseCaja?.idSedeBAse ?? 0;
            return !string.IsNullOrWhiteSpace(db) && idBaseCaja > 0;
        }

        private async Task CargarCombos()
        {
            using (var cn = new Conection_SQL(db))
            {
                ddlBolsillo.Items.Clear();
                ddlBolsillo.Items.Add(new System.Web.UI.WebControls.ListItem("Selecciona...", ""));
                var jsonBolsillos = await cn.EjecutarConsulta("select id, nombreBolsillo from Bolsillo order by nombreBolsillo", true);
                var bolsillos = string.IsNullOrWhiteSpace(jsonBolsillos)
                    ? new List<BolsilloItem>()
                    : JsonConvert.DeserializeObject<List<BolsilloItem>>(jsonBolsillos) ?? new List<BolsilloItem>();

                ddlBolsillo.DataSource = bolsillos;
                ddlBolsillo.DataTextField = "nombreBolsillo";
                ddlBolsillo.DataValueField = "id";
                ddlBolsillo.DataBind();

                ddlTipoGasto.Items.Clear();
                ddlTipoGasto.Items.Add(new System.Web.UI.WebControls.ListItem("Selecciona...", ""));
                var jsonTipos = await cn.EjecutarConsulta("select id, nombreTipoGasto from TipoGasto order by nombreTipoGasto", true);
                var tipos = string.IsNullOrWhiteSpace(jsonTipos)
                    ? new List<TipoGastoItem>()
                    : JsonConvert.DeserializeObject<List<TipoGastoItem>>(jsonTipos) ?? new List<TipoGastoItem>();

                ddlTipoGasto.DataSource = tipos;
                ddlTipoGasto.DataTextField = "nombreTipoGasto";
                ddlTipoGasto.DataValueField = "id";
                ddlTipoGasto.DataBind();
            }
        }

        private async Task CargarGastos()
        {
            using (var cn = new Conection_SQL(db))
            {
                var filtro = (txtBuscar.Text ?? string.Empty).Trim().Replace("'", "''");
                var where = " where idBaseGasto = " + idBaseCaja;

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    where += " and (concepto like '%" + filtro + "%' or VnombreBolsillo like '%" + filtro + "%' or nombreTipoGasto like '%" + filtro + "%')";
                }

                if (DateTime.TryParse(txtDesde.Text, out var desde))
                {
                    where += " and cast(fecha as date) >= '" + desde.ToString("yyyy-MM-dd") + "'";
                }

                if (DateTime.TryParse(txtHasta.Text, out var hasta))
                {
                    where += " and cast(fecha as date) <= '" + hasta.ToString("yyyy-MM-dd") + "'";
                }

                var sql = "select idGasto, fecha, concepto, valor, VidBolsillo as idBolsillo, VnombreBolsillo as nombreBolsillo, idTipoGasto, nombreTipoGasto, idBaseGasto from v_Gastos" + where + " order by fecha desc, idGasto desc";
                var json = await cn.EjecutarConsulta(sql, true);
                ListaGastos = string.IsNullOrWhiteSpace(json)
                    ? new List<GastoVistaItem>()
                    : JsonConvert.DeserializeObject<List<GastoVistaItem>>(json) ?? new List<GastoVistaItem>();
            }
        }

        protected string FormatearMoneda(decimal valor)
        {
            return valor.ToString("C0");
        }

        private void LimpiarFormularioGasto()
        {
            hfIdGasto.Value = string.Empty;
            txtFechaGasto.Text = DateTime.Today.ToString("yyyy-MM-dd");
            txtConcepto.Text = string.Empty;
            txtValor.Text = string.Empty;
            ddlBolsillo.SelectedIndex = 0;
            ddlTipoGasto.SelectedIndex = 0;
            hfIdEliminar.Value = string.Empty;
        }

        private void RestaurarSeleccionCombo(System.Web.UI.WebControls.DropDownList ddl)
        {
            if (ddl == null)
            {
                return;
            }

            var postedValue = Request.Form[ddl.UniqueID];
            if (!string.IsNullOrWhiteSpace(postedValue) && ddl.Items.FindByValue(postedValue) != null)
            {
                ddl.SelectedValue = postedValue;
            }
        }
        private int ParseInt(string value)
        {
            return int.TryParse(value, out var id) ? id : 0;
        }

        private async Task<Respuesta_DAL> EjecutarCrudGastoAsync(Gastos gasto, int accion)
        {
            try
            {
                var json = JsonConvert.SerializeObject(gasto ?? new Gastos()).Replace("'", "''");
                var sql = "EXEC [dbo].[CRUD_Gastos] @Accion = " + accion + ", @Json = N'" + json + "'";

                using (var cn = new Conection_SQL(db))
                {
                    var resultadoJson = await cn.EjecutarConsulta(sql, false);
                    if (string.IsNullOrWhiteSpace(resultadoJson))
                    {
                        return new Respuesta_DAL
                        {
                            estado = false,
                            mensaje = "Sin respuesta del procedimiento CRUD_Gastos.",
                            data = 0
                        };
                    }

                    var respCrud = JsonConvert.DeserializeObject<RespuestaCRUD>(resultadoJson) ?? new RespuestaCRUD();
                    var idFinal = string.IsNullOrWhiteSpace(respCrud.IdFinal) ? "0" : respCrud.IdFinal;
                    var esExitoso = respCrud.estado;

                    if (!esExitoso && int.TryParse(idFinal, out var idAfectado) && idAfectado > 0)
                    {
                        esExitoso = true;
                    }

                    if (!esExitoso && !string.IsNullOrWhiteSpace(respCrud.mensaje))
                    {
                        var mensaje = respCrud.mensaje.ToLowerInvariant();
                        esExitoso = mensaje.Contains("correct") || mensaje.Contains("guard") || mensaje.Contains("actualiz") || mensaje.Contains("elimin");
                    }

                    return new Respuesta_DAL
                    {
                        estado = esExitoso,
                        mensaje = string.IsNullOrWhiteSpace(respCrud.mensaje) ? (esExitoso ? "Proceso realizado correctamente." : "No fue posible procesar el gasto.") : respCrud.mensaje,
                        data = idFinal
                    };
                }
            }
            catch (Exception ex)
            {
                return new Respuesta_DAL
                {
                    estado = false,
                    mensaje = "Error en CRUD_Gastos: " + ex.Message,
                    data = 0
                };
            }
        }
        protected class GastoVistaItem
        {
            public int idGasto { get; set; }
            public DateTime fecha { get; set; }
            public string concepto { get; set; }
            public decimal valor { get; set; }
            public int idBolsillo { get; set; }
            public string nombreBolsillo { get; set; }
            public int idTipoGasto { get; set; }
            public string nombreTipoGasto { get; set; }
            public int idBaseGasto { get; set; }
        }

        private class BolsilloItem
        {
            public int id { get; set; }
            public string nombreBolsillo { get; set; }
        }

        private class TipoGastoItem
        {
            public int id { get; set; }
            public string nombreTipoGasto { get; set; }
        }

        private class Gastos
        {
            public int id { get; set; }
            public DateTime? fecha { get; set; }
            public string concepto { get; set; }
            public decimal valor { get; set; }
            public int idBolsillo { get; set; }
            public int idBaseCaja { get; set; }
            public int idTipoGasto { get; set; }
            public int idSede { get; set; }
        }
    }
}
