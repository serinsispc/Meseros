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

namespace WebApplication
{
    public partial class _Default : Page
    {
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

                    Session["db"] = db;

                    // consultar sede (para usarla luego si hace falta)
                    var sede = await SedeControler.Consultar(db);
                    if (sede != null)
                    {
                        Session["porpropina"]= sede.porcentaje_propina;
                        Session["sede"] = JsonConvert.SerializeObject(sede);
                        Session["guidSede"] = sede.guidSede;
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
                }
                else
                {
                    AlertModerno.Error(this, "Error", "No se encuentra la base de datos.", true);
                }
            }
        }

        protected async void btnIngresar_Click(object sender, EventArgs e)
        {
            if (txtCelular.Text != string.Empty && txtContrasena.Text != string.Empty)
            {
                string db = Session["db"]?.ToString();
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
                    Session["cajero"] = vendedor.cajaMovil;
                    Session["idvendedor"] = vendedor.id;
                    Session["NombreMesero"] = vendedor.nombreVendedor;
                    Session["vendedor"] = JsonConvert.SerializeObject(vendedor);

                    // si tiene permiso de caja
                    if (vendedor.cajaMovil == 1)
                    {
                        // consultar relación vendedor -> usuario caja
                        var usuarioCaja = await R_VendedorUsuarioControler.ConsultarRelacion(db, vendedor.id);
                        if (usuarioCaja != null)
                        {
                            Session["usuario_caja"] = JsonConvert.SerializeObject(usuarioCaja);

                            // verificar base activa
                            var baseActiva = await BaseCajaControler.VerificarBaseCaja(db, usuarioCaja.id);

                            if (baseActiva != null)
                            {
                                Session["base_caja"] = JsonConvert.SerializeObject(baseActiva);

                                AlertModerno.SuccessGoTo(this, "Ok",
                                    $"Bienvenido {vendedor.nombreVendedor}",
                                    "~/menu.aspx", esToast: false, ms: 1200);
                            }
                            else
                            {
                                // ✅ NO redirigir. Abrir modal para ingresar base
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
                        AlertModerno.SuccessGoTo(this, "Ok",
                            $"Bienvenido {vendedor.nombreVendedor}",
                            "~/menu.aspx", esToast: false, ms: 1200);
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
            string db = Session["db"]?.ToString();
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
            var guidSede = Session["guidSede"] != null ? Session["guidSede"].ToString() : null;

            // ✅ AQUÍ APERTURAS LA BASE
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
                Session["base_caja"] = JsonConvert.SerializeObject(baseNueva);

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
