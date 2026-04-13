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
using System.Web.UI;
using WebApplication.Class;
using WebApplication.Helpers;
using WebApplication.ViewModels;

namespace WebApplication
{
    public partial class _Default : Page
    {
        protected MenuViewModels models = new MenuViewModels();

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
            await Task.WhenAll(vendedoresTask, imagenTask);

            var vendedores = vendedoresTask.Result ?? new List<Vendedor>();
            var imagenes = imagenTask.Result;

            ConfigurarLogoLogin(db, imagenes?.imagenBytes);

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

                    if (await AdminControlAccessHelper.MostrarRecordatorioIngresoSiCorrespondeAsync(this, db, "~/caja.aspx"))
                    {
                        return;
                    }

                    AlertModerno.SuccessGoTo(this, "Ok", $"Bienvenido {vendedor.nombreVendedor}", "~/caja.aspx", false, 1200);
                    return;
                }

                SessionContextHelper.ApplyOperationalContext(Session, models);

                if (await AdminControlAccessHelper.MostrarRecordatorioIngresoSiCorrespondeAsync(this, db, onCloseScript: "openBaseModal();"))
                {
                    return;
                }

                AbrirModalBase();
                return;
            }

            SessionContextHelper.ApplyOperationalContext(Session, models);

            if (await AdminControlAccessHelper.MostrarRecordatorioIngresoSiCorrespondeAsync(this, db, "~/caja.aspx"))
            {
                return;
            }

            AlertModerno.SuccessGoTo(this, "Ok", $"Bienvenido {vendedor.nombreVendedor}", "~/caja.aspx", false, 1200);
        }

        protected async void btnAperturarBase_Click(object sender, EventArgs e)
        {
            DeserializarModels();

            string db = SanitizarDb(models.db);
            if (string.IsNullOrEmpty(db))
            {
                AlertModerno.Error(this, "Error", "No se encuentra la base de datos en sesión.", true);
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
            var respCajon = await AperturarCajonControler.CRUD(Session["db"].ToString(), cajon, 0);

            AlertModerno.SuccessGoTo(this, "Ok", mensaje, "~/caja.aspx", false, 1800);
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

        private void AbrirModalBase()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "openBaseModal", "openBaseModal();", true);
        }
    }
}



