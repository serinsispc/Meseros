using DAL;
using DAL.Controler;
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Web.UI;
using WebApplication.Class;
using WebApplication.Helpers;
using WebApplication.ViewModels;

namespace WebApplication
{
    public partial class _Default : Page
    {
        protected MenuViewModels models = new MenuViewModels();
        private bool RequiereBaseApertura
        {
            get { return (bool?)ViewState["RequiereBaseApertura"] ?? false; }
            set { ViewState["RequiereBaseApertura"] = value; }
        }

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                await InicializarLogin();
                return;
            }

            DeserializarModels();
        }

        private async Task InicializarLogin()
        {
            var db = ResolverDbInicial();
            if (string.IsNullOrWhiteSpace(db))
            {
                AlertModerno.Error(this, "Error", "No se encuentra la base de datos.", true);
                return;
            }

            SessionContextHelper.ClearOperationalContext(Session);
            models = new MenuViewModels { db = db };

            var sede = await SedeControler.Consultar(db);
            if (sede == null)
            {
                AlertModerno.Error(this, "Error", "No se encontró la sede configurada para esta base.", true);
                return;
            }

            models.Sede = sede;

            //ahora consultamos la tabla DBConexion y lo gardamos en session para futuras consultas
            var ajustesDb = await DBConexionControler.DAsync(db);
            if (ajustesDb!=null)
            {
                string dbconexion = JsonConvert.SerializeObject(ajustesDb);
                Session["DBConexion"] = dbconexion;
            }

            var vendedoresTask = VendedorControler.ListaVendedor(db);
            var imagenTask = ImagenesControler.Consultar(db, sede.guidSede);
            var puntosDePagoTask = PuntosDePagoControler.Lista(db);
            await Task.WhenAll(vendedoresTask, imagenTask, puntosDePagoTask);

            var vendedores = vendedoresTask.Result ?? new List<Vendedor>();
            var imagenes = imagenTask.Result;
            models.puntosDePago = puntosDePagoTask.Result ?? new List<PuntosDePago>();

            ConfigurarLogoLogin(db, imagenes?.imagenBytes);
            BindPuntosDePago(models.puntosDePago, models.PuntoDePagoSeleccionado?.id);

            rptUsuarios.DataSource = vendedores;
            rptUsuarios.DataBind();

            SessionContextHelper.SaveModels(Session, models);
        }

        private string ResolverDbInicial()
        {
            var db = SanitizarDb(Request.QueryString["db"]);
            if (!string.IsNullOrWhiteSpace(db) && db != "-")
            {
                return db;
            }

            var sessionModel = SessionContextHelper.LoadModels(Session);
            return SanitizarDb(sessionModel?.db);
        }

        private static string SanitizarDb(string db)
        {
            if (string.IsNullOrWhiteSpace(db))
            {
                return string.Empty;
            }

            db = db.Replace("/", string.Empty)
                   .Replace("\\", string.Empty)
                   .Replace("..", string.Empty)
                   .Trim();

            return db;
        }

        private void ConfigurarLogoLogin(string db, byte[] imagen)
        {
            var rutaVirtual = $"~/Recursos/Imagenes/logo/{db}.png";
            var rutaFisica = Server.MapPath(rutaVirtual);
            var carpeta = Path.GetDirectoryName(rutaFisica);

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            if (imagen != null && imagen.Length > 0)
            {
                File.WriteAllBytes(rutaFisica, imagen);
            }

            if (!File.Exists(rutaFisica))
            {
                imgLogoLogin.Visible = false;
                return;
            }

            imgLogoLogin.ImageUrl = ResolveUrl(rutaVirtual) + "?v=" + File.GetLastWriteTimeUtc(rutaFisica).Ticks;
            imgLogoLogin.Visible = true;
        }

        private void DeserializarModels()
        {
            var model = SessionContextHelper.LoadModels(Session);
            if (model != null)
            {
                models = model;
            }

            var selectedId = ObtenerPuntoDePagoSeleccionadoDesdeRequest()
                ?? models?.PuntoDePagoSeleccionado?.id;

            BindPuntosDePago(models?.puntosDePago, selectedId);
            ConfigurarModalInicioTurno();
        }

        protected async void btnIngresar_Click(object sender, EventArgs e)
        {
            DeserializarModels();

            if (string.IsNullOrWhiteSpace(txtCelular.Text) || string.IsNullOrWhiteSpace(txtContrasena.Text))
            {
                AlertModerno.Warning(this, "Atención", "Debes seleccionar un usuario e ingresar la contraseña.", true);
                return;
            }

            string db = SanitizarDb(models.db);
            if (string.IsNullOrEmpty(db))
            {
                AlertModerno.Error(this, "Error", "No se encuentra la base de datos en sesión.", true);
                return;
            }

            if (await AdminControlAccessHelper.BloquearIngresoSiSuspendidoAsync(this, db))
            {
                return;
            }

            string usuario = txtCelular.Text.Trim();
            string clave = txtContrasena.Text.Trim();

            var vendedor = await VendedorControler.Consultar_usuario_clave(db, usuario, clave);
            if (vendedor == null)
            {
                AlertModerno.Error(this, "Error", "Usuario no existe.", true);
                return;
            }

            models.vendedor = vendedor;

            if (vendedor.cajaMovil == 1)
            {
                var usuarioCaja = await R_VendedorUsuarioControler.ConsultarRelacion(db, vendedor.id);
                if (usuarioCaja == null)
                {
                    AlertModerno.Error(this, "Error", "No tiene un usuario de caja relacionado, contacte al administrador.", true);
                    return;
                }

                Session[SessionContextHelper.UsuarioCajaKey] = JsonConvert.SerializeObject(usuarioCaja);

                var tokenEmpresa = await tokenEmpresaControler.ConsultarToken(db);
                models.TokenEmpresa = tokenEmpresa?.token ?? string.Empty;

                var baseActiva = await BaseCajaControler.VerificarBaseCaja(db, usuarioCaja.idUSuario);
                if (baseActiva != null)
                {
                    models.BaseCaja = baseActiva;
                    SessionContextHelper.ApplyOperationalContext(Session, models);

                    if (await AdminControlAccessHelper.MostrarRecordatorioIngresoSiCorrespondeAsync(this, db, onCloseScript: "openBaseModal();"))
                    {
                        return;
                    }

                    AbrirModalBase(false);
                    return;
                }

                SessionContextHelper.ApplyOperationalContext(Session, models);

                if (await AdminControlAccessHelper.MostrarRecordatorioIngresoSiCorrespondeAsync(this, db, onCloseScript: "openBaseModal();"))
                {
                    return;
                }

                AbrirModalBase(true);
                return;
            }

            SessionContextHelper.ApplyOperationalContext(Session, models);

            if (await AdminControlAccessHelper.MostrarRecordatorioIngresoSiCorrespondeAsync(this, db, onCloseScript: "openBaseModal();"))
            {
                return;
            }

            AbrirModalBase(false);
        }

        protected async void btnAperturarBase_Click(object sender, EventArgs e)
        {
            DeserializarModels();
            ConfigurarModalInicioTurno();

            string db = SanitizarDb(models.db);
            if (string.IsNullOrEmpty(db))
            {
                AlertModerno.Error(this, "Error", "No se encuentra la base de datos en sesión.", true);
                return;
            }

            if (!TryResolvePuntoDePagoSeleccionado(out var puntoDePago, out var mensajePuntoDePago))
            {
                AlertModerno.Error(this, "Error", mensajePuntoDePago, true);
                AbrirModalBase(RequiereBaseApertura);
                return;
            }

            models.PuntoDePagoSeleccionado = puntoDePago;

            if (!RequiereBaseApertura)
            {
                SessionContextHelper.ApplyOperationalContext(Session, models);
                AlertModerno.SuccessGoTo(this, "Ok", $"Bienvenido {models.vendedor?.nombreVendedor}", "~/caja.aspx", false, 1200);
                return;
            }

            var usuarioCajaJson = Session[SessionContextHelper.UsuarioCajaKey]?.ToString();
            if (string.IsNullOrEmpty(usuarioCajaJson))
            {
                AlertModerno.Error(this, "Error", "No se encuentra el usuario de caja en sesión.", true);
                return;
            }

            var usuarioCaja = JsonConvert.DeserializeObject<R_VendedorUsuario>(usuarioCajaJson);
            if (usuarioCaja == null)
            {
                AlertModerno.Error(this, "Error", "La relación de usuario de caja es inválida.", true);
                return;
            }

            string txt = txtValorBaseModal.Text?.Trim() ?? string.Empty;
            txt = txt.Replace(".", string.Empty).Replace(",", string.Empty);
            if (!decimal.TryParse(txt, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal valorBase) || valorBase <= 0)
            {
                AlertModerno.Error(this, "Error", "Ingrese un valor de base válido.", true);
                AbrirModalBase();
                return;
            }

            var baseNueva = await BaseCajaControler.AperturarBase(db, new BaseCaja
            {
                id = 0,
                fechaApertura = DateTime.Now,
                idUsuarioApertura = usuarioCaja.idUSuario,
                valorBase = Convert.ToInt32(valorBase),
                fechaCierre = DateTime.Now,
                idUsuarioCierre = usuarioCaja.idUSuario,
                estadoBase = "ACTIVA",
                idSedeBAse = models.Sede?.id ?? 1
            }, 0);

            if (baseNueva == null)
            {
                AlertModerno.Error(this, "Error", "No fue posible aperturar la caja. Verifique e intente nuevamente.", true);
                AbrirModalBase();
                return;
            }

            models.BaseCaja = baseNueva;
            SessionContextHelper.ApplyOperationalContext(Session, models);

            var aperturaData = ConstruirDatosApertura(baseNueva, usuarioCaja);
            var mensaje = "Caja aperturada correctamente.";

            try
            {
                var whatsApp = new WhatsAppMetaAperturaHelper();
                var envioWhatsApp = await whatsApp.EnviarAperturaCajaAsync(db, models, aperturaData);
                if (!string.IsNullOrWhiteSpace(envioWhatsApp?.Message))
                {
                    mensaje += " " + envioWhatsApp.Message;
                }
            }
            catch (Exception ex)
            {
                mensaje += " WhatsApp pendiente: " + ex.Message;
            }

            try
            {
                var email = new EmailAperturaCajaHelper();
                var envioCorreo = await email.EnviarAperturaCajaAsync(db, ConstruirCorreoApertura(aperturaData, baseNueva));
                if (!string.IsNullOrWhiteSpace(envioCorreo?.Message))
                {
                    mensaje += " " + envioCorreo.Message;
                }
            }
            catch (Exception ex)
            {
                mensaje += " Correo pendiente: " + ex.Message;
            }

            //cargamos orden para abrir el cajon
            var cajon = new AperturarCajon() { estado = true };
            PuntoDePagoPrinterHelper.Apply(cajon, Session, models);
            var respCajon = await AperturarCajonControler.CRUD(Session["db"].ToString(), cajon, 0);

            AlertModerno.SuccessGoTo(this, "Ok", mensaje, "~/caja.aspx", false, 1800);
        }

        private void ConfigurarModalInicioTurno()
        {
            var requiereSeleccionPunto = RequiereSeleccionManualPuntoDePago();
            panelPuntoDePago.Visible = requiereSeleccionPunto;
            textoPuntoDePago.InnerText = requiereSeleccionPunto
                ? "Antes de continuar, selecciona el punto de pago en el que vas a trabajar."
                : "Se usará automáticamente el punto de pago predeterminado para este cliente.";

            panelBaseApertura.Attributes["class"] = RequiereBaseApertura
                ? "mb-3 inicio-turno-base"
                : "mb-3 inicio-turno-base oculto";

            btnAperturarBase.Text = RequiereBaseApertura ? "Aperturar caja" : "Ingresar";
        }

        private void BindPuntosDePago(List<PuntosDePago> puntosDePago, int? selectedId = null)
        {
            ddlPuntoDePago.Items.Clear();

            var puntos = puntosDePago ?? new List<PuntosDePago>();
            if (puntos.Count == 0)
            {
                ddlPuntoDePago.Items.Add(new ListItem("Sin puntos de pago configurados", string.Empty));
                return;
            }

            if (puntos.Count > 1)
            {
                ddlPuntoDePago.Items.Add(new ListItem("Selecciona un punto de pago", string.Empty));
            }

            foreach (var punto in puntos)
            {
                var texto = string.IsNullOrWhiteSpace(punto.impresoraPredeterminada)
                    ? punto.nombrePunto
                    : string.Format("{0} - {1}", punto.nombrePunto, punto.impresoraPredeterminada);

                ddlPuntoDePago.Items.Add(new ListItem(texto, punto.id.ToString()));
            }

            if (selectedId.HasValue && selectedId.Value > 0)
            {
                var item = ddlPuntoDePago.Items.FindByValue(selectedId.Value.ToString());
                if (item != null)
                {
                    ddlPuntoDePago.ClearSelection();
                    item.Selected = true;
                }
            }
            else if (puntos.Count == 1)
            {
                ddlPuntoDePago.ClearSelection();
                ddlPuntoDePago.Items[0].Selected = true;
            }
        }

        private int? ObtenerPuntoDePagoSeleccionadoDesdeRequest()
        {
            var rawValue = Request?.Form[ddlPuntoDePago.UniqueID]
                ?? Request?.Form[ddlPuntoDePago.ClientID]
                ?? Request?.Form["ddlPuntoDePago"];

            if (int.TryParse(rawValue, out var id) && id > 0)
            {
                return id;
            }

            return null;
        }

        private bool TryResolvePuntoDePagoSeleccionado(out PuntosDePago puntoDePago, out string mensaje)
        {
            puntoDePago = null;
            mensaje = string.Empty;

            var puntos = models?.puntosDePago ?? new List<PuntosDePago>();
            if (puntos.Count == 0)
            {
                return true;
            }

            if (puntos.Count == 1)
            {
                puntoDePago = puntos[0];
                return true;
            }

            var idPuntoDePago = ObtenerPuntoDePagoSeleccionadoDesdeRequest()
                ?? (int.TryParse(ddlPuntoDePago.SelectedValue, out var selectedId) ? selectedId : 0);

            if (idPuntoDePago <= 0)
            {
                mensaje = "Selecciona un punto de pago para aperturar la caja.";
                return false;
            }

            puntoDePago = puntos.FirstOrDefault(x => x.id == idPuntoDePago);
            if (puntoDePago == null)
            {
                mensaje = "El punto de pago seleccionado no es válido.";
                return false;
            }

            return true;
        }

        private bool RequiereSeleccionManualPuntoDePago()
        {
            return (models?.puntosDePago?.Count ?? 0) > 1;
        }

        private AperturaCajaWhatsAppData ConstruirDatosApertura(BaseCaja baseNueva, R_VendedorUsuario usuarioCaja)
        {
            var fecha = baseNueva.fechaApertura;
            return new AperturaCajaWhatsAppData
            {
                NombreCajero = models?.vendedor?.nombreVendedor ?? "-",
                NombreCliente = models?.Sede?.nombreSede ?? "Cliente",
                FechaApertura = fecha.ToString("dd-MM-yyyy"),
                HoraApertura = fecha.ToString("hh:mm tt", new CultureInfo("es-CO")).Replace("a. m.", "A.M").Replace("p. m.", "P.M"),
                ValorBase = baseNueva.valorBase
            };
        }

        private AperturaCajaEmailData ConstruirCorreoApertura(AperturaCajaWhatsAppData data, BaseCaja baseNueva)
        {
            return new AperturaCajaEmailData
            {
                IdTurno = baseNueva?.id ?? 0,
                NombreCajero = data.NombreCajero,
                NombreCliente = data.NombreCliente,
                FechaApertura = data.FechaApertura,
                HoraApertura = data.HoraApertura,
                ValorBase = data.ValorBase,
                FechaGeneracion = DateTime.Now.ToString("yyyy-MM-dd hh:mm tt")
            };
        }

        private void AbrirModalBase(bool requiereBase = false)
        {
            RequiereBaseApertura = requiereBase;
            ConfigurarModalInicioTurno();
            ScriptManager.RegisterStartupScript(this, GetType(), "openBaseModal", "openBaseModal();", true);
        }
    }
}



