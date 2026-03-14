using DAL;
using DAL.Controler;
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Web.UI;
using WebApplication.Class;
using WebApplication.ViewModels;

namespace WebApplication
{
    public partial class _Default : Page
    {
        protected MenuViewModels models =new MenuViewModels();
        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string db = Request.QueryString["db"];
                if (db == "-") return;
                if (!string.IsNullOrEmpty(db))
                {
                    // Sanitizar db (seguridad)
                    db = db.Replace("/", "").Replace("\\", "").Replace("..", "").Trim();

                    models.db = db;

                    // consultar sede (para usarla luego si hace falta)
                    var sede = await SedeControler.Consultar(db);
                    if (sede != null)
                    {
                        models.Sede =sede;
                    }

                    // logo
                    var imagenes = await ImagenesControler.Consultar(db, sede.guidSede);

                    if (imagenes != null && imagenes.imagenBytes != null && imagenes.imagenBytes.Length > 0)
                    {
                        byte[] imagenbyte = imagenes.imagenBytes;

                        string carpeta = Server.MapPath("~/Recursos/Imagenes/logo/");
                        if (!Directory.Exists(carpeta))
                            Directory.CreateDirectory(carpeta);

                        string rutaCompleta = Path.Combine(carpeta, $"{db}.png");
                        File.WriteAllBytes(rutaCompleta, imagenbyte);
                    }

                    // vendedores
                    var vendedores = await VendedorControler.ListaVendedor(db);
                    if (vendedores != null && vendedores.Count > 0)
                    {
                        rptUsuarios.DataSource = vendedores;
                        rptUsuarios.DataBind();
                    }
                    await SerialisarModels();
                }
                else
                {
                    AlertModerno.Error(this, "Error", "No se encuentra la base de datos.", true);
                }
            }
            else
            {
                //en esta parte Deserialisamos el models
                await DeserializarModels();
            }
        }
                private async Task DeserializarModels()
        {
            if (Session["Models"] is MenuViewModels modelEnSesion)
            {
                models = modelEnSesion;
                return;
            }

            if (Session["ModelsJson"] != null)
            {
                var modelJson = Session["ModelsJson"].ToString();
                models = JsonConvert.DeserializeObject<MenuViewModels>(modelJson);
            }

        }
        private async Task SerialisarModels()
        {
            Session["ModelsJson"]=JsonConvert.SerializeObject(models);
        }

        protected async void btnIngresar_Click(object sender, EventArgs e)
        {
            if (txtCelular.Text != string.Empty && txtContrasena.Text != string.Empty)
            {
                string db = models.db;
                if (string.IsNullOrEmpty(db))
                {
                    AlertModerno.Error(this, "Error", "No se encuentra la base de datos en sesión.", true);
                    return;
                }

                string usuario = txtCelular.Text;
                string clave = txtContrasena.Text;

                var vendedor = await VendedorControler.Consultar_usuario_clave(db, usuario, clave);

                if (vendedor != null)
                {
                    models.vendedor = vendedor;
                    // si tiene permiso de caja
                    if (vendedor.cajaMovil == 1)
                    {
                        // consultar relación vendedor -> usuario caja
                        var usuarioCaja = await R_VendedorUsuarioControler.ConsultarRelacion(db, vendedor.id);
                        if (usuarioCaja != null)
                        {
                            //consultamos el token de facturacion
                            var tokem = await tokenEmpresaControler.ConsultarToken(db);
                            if(tokem != null)
                            {
                                models.TokenEmpresa = tokem.token;
                            }

                            // verificar base activa
                            var baseActiva = await BaseCajaControler.VerificarBaseCaja(db, usuarioCaja.id);
                            if (baseActiva != null)
                            {
                                models.BaseCaja = baseActiva;
                                await SerialisarModels();
                                AlertModerno.SuccessGoTo(this, "Ok",
                                    $"Bienvenido {vendedor.nombreVendedor}",
                                    "~/caja.aspx", esToast: false, ms: 1200);
                            }
                            else
                            {
                                // ? NO redirigir. Abrir modal para ingresar base
                                AbrirModalBase();
                                return;
                            }
                        }
                        else
                        {
                            AlertModerno.Error(this, "Error", "No tiene un usuario de caja relacionado, contacte al administrador.", true);
                            return;
                        }
                    }
                    else
                    {
                        await SerialisarModels();
                        AlertModerno.SuccessGoTo(this, "Ok",
                            $"Bienvenido {vendedor.nombreVendedor}",
                            "~/caja.aspx", esToast: false, ms: 1200);
                    }
                }
                else
                {
                    AlertModerno.Error(this, "Error", "Usuario no existe.", true);
                }
            }
        }

        protected async void btnAperturarBase_Click(object sender, EventArgs e)
        {
            string db = models.db;
            if (string.IsNullOrEmpty(db))
            {
                AlertModerno.Error(this, "Error", "No se encuentra la base de datos en sesión.", true);
                return;
            }

            var usuarioCajaJson = Session["usuario_caja"]?.ToString();
            if (string.IsNullOrEmpty(usuarioCajaJson))
            {
                AlertModerno.Error(this, "Error", "No se encuentra el usuario de caja en sesión.", true);
                return;
            }

            var usuarioCaja = JsonConvert.DeserializeObject<R_VendedorUsuario>(usuarioCajaJson);

            // leer valor base del modal
            string txt = txtValorBaseModal.Text?.Trim() ?? "";

            // permitir "200000" o "200.000"
            txt = txt.Replace(".", "").Replace(",", "");
            if (!decimal.TryParse(txt, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal valorBase) || valorBase <= 0)
            {
                AlertModerno.Error(this, "Error", "Ingrese un valor de base válido.", true);
                AbrirModalBase();
                return;
            }

            // guidSede si lo necesitas
            var guidSede = models.Sede.guidSede;

            // ? AQUÍ APERTURAS LA BASE
            // Ajusta este llamado al método real de tu DAL/Controler:
            // Ejemplo esperado: BaseCajaControler.AperturarBase(db, usuarioCaja.id, valorBase, guidSede)
            var basecaja = new BaseCaja {
             id=0,
             fechaApertura=DateTime.Now,
             idUsuarioApertura=usuarioCaja.id,
             valorBase=Convert.ToInt32(txt),
             fechaCierre=DateTime.Now,
             idUsuarioCierre=usuarioCaja.id,
             estadoBase= "ACTIVA",
             idSedeBAse= 1
            };
            var baseNueva = await BaseCajaControler.AperturarBase(db, basecaja,0);
            // ---------------------------

            if (baseNueva != null)
            {
                models.BaseCaja=baseNueva;
                await SerialisarModels();
                AlertModerno.SuccessGoTo(this, "Ok",
                    "Caja aperturada correctamente.",
                    "~/menu.aspx", esToast: false, ms: 1200);
            }
            else
            {
                AlertModerno.Error(this, "Error", "No fue posible aperturar la caja. Verifique e intente nuevamente.", true);
                AbrirModalBase();
            }
        }

        private void AbrirModalBase()
        {
            ScriptManager.RegisterStartupScript(
                this,
                this.GetType(),
                "openBaseModal",
                "openBaseModal();",
                true
            );
        }
    }
}


