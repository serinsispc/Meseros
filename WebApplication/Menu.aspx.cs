
using DAL.Controler;
using DAL.Funciones;
using DAL.Model;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication.Class;
using WebApplication.ViewModels;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication
{
    public partial class Menu : Page
    {
        #region Constantes de sesión / claves
        private const string SessionVendedorKey = "Vendedor";
        private const string SessionZonaActivaKey = "zonaactiva";
        private const string SessionModelsKey = "Models";
        private const string SessionIdVendedorKey = "idvendedor";
        #endregion

        public MenuViewModels Models { get; private set; }

        // variable pública para usar en el .aspx
        public string CuentasJson { get; set; }


        protected async void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // 1) Recuperar modelo desde sesión
                Models = Session[SessionModelsKey] as MenuViewModels;

                // 2) Si no existe, crear uno nuevo y guardarlo en sesión
                if (Models == null)
                {
                    Models = new MenuViewModels();
                    Models.v_CuentaClientes = new List<V_CuentaCliente>();
                    Session[SessionModelsKey] = Models;
                }

                if (!IsPostBack)
                {
                    if (Session["db"] != null)
                    {
                        await InicializarPagina(Session["db"].ToString());
                    }
                }
                else
                {
                    Models.AbrirModalDomicilio = false;
                    await ProcesarPostBack();
                }

                // 3) Asegurar que la lista no sea null
                if (Models.v_CuentaClientes == null)
                {
                    Models.v_CuentaClientes = new List<V_CuentaCliente>();
                }

                // 4) Filtrar cuentas de la venta activa
                var cuentasActivas = Models.v_CuentaClientes
                    .Where(x => x.idVenta == Models.IdCuentaActiva)
                    .Select(x => new
                    {
                        id = x.id,
                        nombre = x.nombreCuenta ?? x.nombreCuenta,
                        total = Convert.ToDecimal(x.totalVenta)
                    })
                    .ToList();

                var serializer = new JavaScriptSerializer();
                CuentasJson = serializer.Serialize(cuentasActivas);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Page_Load error: " + ex);
                AlertModerno.Error(this, "¡Error!", "Ocurrió un error inesperado al cargar la página.", true);
            }
        }
        protected List<ClienteDomicilio> ClientesDomicilio = new List<ClienteDomicilio>();
        protected string ListaClientesDomicilioJson
        {
            get
            {
                return JsonConvert.SerializeObject(ClientesDomicilio ?? new List<ClienteDomicilio>());
            }
        }
        #region Inicialización (primer load)

        public async Task InicializarPagina(string db)
        {
            var vendedor = ObtenerVendedorDesdeSession();
            if (vendedor == null)
            {
                AlertModerno.Error(this, "¡Error!", "No se encontró la sesión del vendedor.", true);
                return;
            }

            // Obtener cuentas del vendedor
            var cuentas = await V_CuentasVentaControler.Lista_IdVendedor(db, vendedor.id) ?? new List<V_CuentasVenta>();
            int idVenta;

            if (!cuentas.Any())
            {
                idVenta = await TablaVentas_f.NuevaVenta(db, (int)Session["porpropina"]);
                if (idVenta <= 0)
                {
                    AlertModerno.Error(this, "¡Error!", "No fue posible crear una nueva cuenta.", true);
                    return;
                }

                var relacionado = await R_VentaVendedor_f.Relacionar_Vendedor_Venta(db, idVenta, vendedor.id);
                if (!relacionado)
                {
                    AlertModerno.Error(this, "¡Error!", "No fue posible crear la relación del vendedor con la venta.", true);
                    // aún así intentamos recargar cuentas para que la UI no quede rota
                }

                // recargar cuentas
                cuentas = await V_CuentasVentaControler.Lista_IdVendedor(db, vendedor.id) ?? new List<V_CuentasVenta>();
            }
            else
            {
                idVenta = cuentas.First().id;
            }

            // Cargar colecciones base
            var zonas = await ZonasControler.Lista(db) ?? new List<Zonas>();
            var categorias = await V_CategoriaControler.lista(db) ?? new List<V_Categoria>();
            var mesas = await MesasControler.Lista(db) ?? new List<Mesas>();
            var productos = await v_productoVentaControler.Lista(db) ?? new List<v_productoVenta>();

            int idZonaActiva = zonas.FirstOrDefault()?.id ?? 0;
            int idCategoriaActiva = categorias.FirstOrDefault()?.id ?? 0;

            // Guardar zona activa en sesión (si otros módulos la usan)
            Session[SessionZonaActivaKey] = idZonaActiva;
            var listacc = await V_CuentaClienteCotroler.Lista(db, false);

            // Construir ViewModel
            Models = new MenuViewModels();
            if (Session["estadopropina"] == null) Session["estadopropina"] = false;
            Models.estadopropina = (bool)Session["estadopropina"];
            if (Session["porpropina"] == null) Session["porpropina"] = 0;
            Models.porpropina = (int)Session["porpropina"];
            Models.IdMesero = Convert.ToInt32(Session["idvendedor"].ToString());
            Models.NombreMesero = Session["NombreMesero"].ToString();
            Models.IdCuentaActiva = idVenta;
            Models.IdZonaActiva = idZonaActiva;
            Models.IdMesaActiva = 0;
            Models.IdCategoriaActiva = idCategoriaActiva;
            Models.IdCuenteClienteActiva = 0;
            Models.cuentas = cuentas;
            Models.zonas = zonas;
            Models.Mesas = mesas;
            Models.categorias = categorias;
            Models.productos = productos;
            Models.venta = await V_TablaVentasControler.Consultar_Id(db, idVenta);
            Models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(db, 0);
            Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(db, idVenta, 0);
            Models.v_CuentaClientes = listacc;
            Models.adiciones = await V_CatagoriaAdicionControler.Lista(db);
            Models.clienteDomicilios = await ClienteDomicilioControler.Lista(db);
            Models.AbrirModalDomicilio = false;
            Models.cargoDescuentoVentas= await CargoDescuentoVentasControler.ObtenerPorVenta(db,idVenta);
            Models.clientes = await ClientesControler.ListaClientes(db);


            GuardarModelsEnSesion();
            BindProductos();
            DataBind();
        }

        public Vendedor ObtenerVendedorDesdeSession()
        {
            try
            {
                if (Session[SessionVendedorKey] == null) return null;
                var json = Session[SessionVendedorKey].ToString();
                if (string.IsNullOrWhiteSpace(json)) return null;
                return JsonConvert.DeserializeObject<Vendedor>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ObtenerVendedorDesdeSession error: " + ex);
                return null;
            }
        }

        #endregion

        #region PostBack y eventos globales

        private async Task ProcesarPostBack()
        {
            var eventTarget = Request["__EVENTTARGET"];
            var eventArgument = Request["__EVENTARGUMENT"];

            System.Diagnostics.Debug.WriteLine("EventTarget: " + eventTarget);
            System.Diagnostics.Debug.WriteLine("EventArgument: " + eventArgument);

            if (string.IsNullOrEmpty(eventTarget)) return;

            switch (eventTarget)
            {
                case "ActualizarCantidad":
                    await ProcesarActualizarCantidad(eventArgument);
                    break;

                case "EliminarDetalle":
                    await ProcesarEliminarDetalle(eventArgument);
                    break;

                // en el switch donde procesas event targets:
                case "GuardarCuenta":
                    await ProcesarGuardarCuenta(eventArgument);
                    break;

                case "AnclarDetalle":
                    await ProcesarAnclarDetalle(eventArgument);
                    break;

                case "DividirDetalle":
                    await ProcesarDividirDetalle(eventArgument);
                    break;

                case "NotasDetalle":
                    await ProcesarNotasDetalle(eventArgument);
                    break;

                case "btnNuevoServicio":
                    await BTN_NuevoServicio();
                    break;

                case "btnEliminarServicio":
                    await btnEliminarServicio();
                    break;

                case "btnLiberarMesa":
                    await btnLiberarMesa(eventArgument);
                    break;

                case "btnMesa":
                    await rpMesas_ItemCommand(eventArgument);
                    break;

                case "btnCuentaCliente":
                    await btnCuentaCliente(eventArgument);
                    break;

                case "btnEditarPropina":
                    var json = Request.Form["hdnEditarPropina"];
                    await btnEditarPropina(json);
                    break;

                case "btnCuscarProducto":
                    await btnBuscarProducto(eventArgument);
                    break;

                case "btnDomicilio":
                    await btnDomicilio(eventArgument);
                    break;

                case "btnCrearActualizarClienteDomicilio":
                    await btnCrearActualizarClienteDomicilio(eventArgument);
                    break;


                case "btnSeleccionarClienteDomicilio":
                    await btnSeleccionarClienteDomicilio(eventArgument);
                    break;

                case "btnCerrarCaja":
                    await btnCerrarCaja();
                    break;

                case "btnCobrar":
                    await btnCobrar();
                    break;


                default:
                    // otros eventos por nombre...
                    break;
            }
        }
        private async Task btnCobrar()
        {
            Session["modelsJSON"]=JsonConvert.SerializeObject(Models);
            // Guardas lo que necesites antes de ir a Cobrar
            GuardarModelsEnSesion();

            // ⚠️ Estas dos líneas NO son necesarias si vas a redirigir
            // BindProductos();
            // DataBind();

            // Redirigir a la página Cobrar.aspx
            Response.Redirect("~/Cobrar.aspx", false);

            // Importante para métodos async
            Context.ApplicationInstance.CompleteRequest();
        }

        private async Task btnBuscarProducto(string eventArgument)
        {
            // el valor que vino desde el input del buscador
            string textoBuscado = eventArgument?.Trim() ?? string.Empty;
            // consultamos el id de la presentasion en la lista de productos
            var producto = Models.productos.Where(x => x.codigoProducto == textoBuscado).FirstOrDefault();
            if (producto == null)
            {
                AlertModerno.Error(this, "¡Error!", $"el código {textoBuscado} no se encontro", true);
                Models.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), Models.IdCuentaActiva);
                Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), Models.IdCuentaActiva, Models.IdCuenteClienteActiva);
                Models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(Session["db"].ToString(), false);
                Models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(Session["db"].ToString(), Models.IdCuenteClienteActiva);
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
            }
            else
            {
                var resp = await DetalleVenta_f.AgregarProducto(Session["db"].ToString(), producto.idPresentacion, 1, Models.IdCuentaActiva);
                if (resp.estado)
                {
                    //como si se creo el produto ahora verificamos si esta activa una cuenta de cleinte
                    if (Models.IdCuenteClienteActiva > 0)
                    {
                        var ralacion = new R_CuentaCliente_DetalleVenta
                        {
                            id = 0,
                            fecha = DateTime.Now,
                            idCuentaCliente = Models.IdCuenteClienteActiva,
                            idDetalleVenta = (int)resp.data,
                            eliminada = false
                        };
                        var crudrelacion = await R_CuentaCliente_DetalleVentaControler.CRUD(Session["db"].ToString(), ralacion, 0);
                    }
                    AlertModerno.Success(this, "¡OK!", $"{resp.mensaje}", true, 800);
                }
                else
                {
                    AlertModerno.Error(this, "¡Error!", $"{resp.mensaje}", true);
                }

                Models.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), Models.IdCuentaActiva);
                Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), Models.IdCuentaActiva, Models.IdCuenteClienteActiva);
                Models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(Session["db"].ToString(), false);
                Models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(Session["db"].ToString(), Models.IdCuenteClienteActiva);
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
            }

        }
        private async Task btnCuentaCliente(string eventArgument)
        {
            if (string.IsNullOrEmpty(eventArgument))
            {
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
                return;
            }

            // 1️⃣ Separar por "|"
            var parts = eventArgument.Split('|');

            // 2️⃣ Validar que haya al menos dos partes
            if (parts.Length < 1)
            {
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
                return;
            }

            // 3️⃣ Convertir la primera parte a int
            if (!int.TryParse(parts[0], out int idCuenta))
            {
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
                return;
            }


            Models.IdCuenteClienteActiva = idCuenta;
            Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), Models.IdCuentaActiva, idCuenta);
            Models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(Session["db"].ToString(), idCuenta);
            GuardarModelsEnSesion();
            BindProductos();
            DataBind();

        }
        private async Task btnLiberarMesa(string eventArgument)
        {
            if (string.IsNullOrEmpty(eventArgument))
            {
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
                return;
            }


            // 1️⃣ Separar por "|"
            var parts = eventArgument.Split('|');

            // 2️⃣ Validar que haya al menos dos partes
            if (parts.Length < 2)
            {
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
                return;
            }

            // 3️⃣ Convertir la primera parte a int
            if (!int.TryParse(parts[0], out int idMesa))
            {
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
                return;
            }

            // 4️⃣ Tomar la segunda parte (nombre)
            string nombreMesa = parts[1];

            //primero consultamos el id de la mesa
            var mesa = await MesasControler.Consultar_id(Session["db"].ToString(), idMesa);
            if (mesa == null)
            {
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
                return;
            }

            mesa.estadoMesa = 0;
            var crud = await MesasControler.CRUD(Session["db"].ToString(), mesa, 1);
            if (!crud.estado)
            {
                AlertModerno.Error(this, "Error", $"no se pudo modificar el estado de la mesa {nombreMesa}");
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
                return;
            }

            //ahora eliminos la relacion de la mesa con el servicio
            var relacion = await R_VentaMesaControler.Consultar_relacion(Session["db"].ToString(), Models.IdCuentaActiva, idMesa);
            if (relacion == null)
            {
                AlertModerno.Error(this, "Error", $"no se pudo modificar el estado de la mesa {nombreMesa}");
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
            }

            var crud_r = await R_VentaMesaControler.CRUD(Session["db"].ToString(), relacion, 2);
            if (!crud_r.estado)
            {
                AlertModerno.Error(this, "Error", $"no se pudo modificar el estado de la mesa {nombreMesa}");
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
            }

            AlertModerno.Success(this, "Ok", $"mesa {nombreMesa} liberada");
            Models.Mesas = await MesasControler.Lista(Session["db"].ToString());
            Models.cuentas = await V_CuentasVentaControler.Lista_IdVendedor(Session["db"].ToString(), Models.IdMesero);
            GuardarModelsEnSesion();
            BindProductos();
            DataBind();

        }
        private async Task ProcesarNotasDetalle(string eventArgument)
        {
            if (string.IsNullOrWhiteSpace(eventArgument)) return;

            var parts = eventArgument.Split('|');
            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out int detalleId))
                {
                    string notaDetalle = parts[1];

                    var respuestadal = await DetalleVenta_f.NotasDetalle(Session["db"].ToString(), detalleId, notaDetalle);
                    string titulo;
                    if (respuestadal.estado)
                    {
                        titulo = "ok";
                        AlertModerno.Success(this, titulo, respuestadal.mensaje, true, 1000);
                    }
                    else
                    {
                        titulo = "Error";
                        AlertModerno.Error(this, titulo, respuestadal.mensaje, true, 1000);
                    }

                    Models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(Session["db"].ToString(), false);
                    Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), Models.IdCuentaActiva, Models.IdCuenteClienteActiva);

                    GuardarModelsEnSesion();
                    BindProductos();
                    DataBind();
                }
            }

        }
        private async Task ProcesarDividirDetalle(string eventArgument)
        {
            if (string.IsNullOrWhiteSpace(eventArgument)) return;

            var parts = eventArgument.Split('|');
            if (parts.Length == 3)
            {
                if (int.TryParse(parts[0], out int detalleId) &&
                    int.TryParse(parts[1], out int cantidadActual) &&
                    int.TryParse(parts[2], out int cantidadDividir))
                {
                    var respuestadal = await DetalleVenta_f.Dividir(Session["db"].ToString(), detalleId, cantidadActual, cantidadDividir, Models.IdCuentaActiva);
                    string titulo;
                    if (respuestadal.estado)
                    {
                        titulo = "ok";
                        AlertModerno.Success(this, titulo, respuestadal.mensaje, true, 1000);
                    }
                    else
                    {
                        titulo = "Error";
                        AlertModerno.Error(this, titulo, respuestadal.mensaje, true, 1000);
                    }

                    Models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(Session["db"].ToString(), false);
                    Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), Models.IdCuentaActiva, Models.IdCuenteClienteActiva);
                    Models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(Session["db"].ToString(), Models.IdCuenteClienteActiva);
                    GuardarModelsEnSesion();
                    BindProductos();
                    DataBind();
                }
            }

        }
        private async Task ProcesarAnclarDetalle(string eventArgument)
        {
            if (string.IsNullOrWhiteSpace(eventArgument)) return;

            var parts = eventArgument.Split('|');
            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out int detalleId) &&
                    int.TryParse(parts[1], out int cuentaId))
                {
                    // Aquí llamas tu método que ancla el detalle a la cuenta
                    System.Diagnostics.Debug.WriteLine($"detalle ID {detalleId} con cuenta id {cuentaId}");



                    var respuestadal = await R_CuentaCliente_DetalleVenta_f.Insert(Session["db"].ToString(), cuentaId, detalleId);
                    string titulo;
                    if (respuestadal.estado)
                    {
                        titulo = "ok";
                        AlertModerno.Success(this, titulo, respuestadal.mensaje, true, 1000);
                    }
                    else
                    {
                        titulo = "Error";
                        AlertModerno.Error(this, titulo, respuestadal.mensaje, true, 1000);
                    }

                    Models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(Session["db"].ToString(), false);
                    Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), Models.IdCuentaActiva, Models.IdCuenteClienteActiva);
                    Models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(Session["db"].ToString(), Models.IdCuenteClienteActiva);
                    GuardarModelsEnSesion();
                    BindProductos();
                    DataBind();
                }
            }

        }
        private async Task ProcesarActualizarCantidad(string eventArgument)
        {
            if (string.IsNullOrWhiteSpace(eventArgument)) return;

            var partes = eventArgument.Split('|');
            if (partes.Length < 2) return;

            if (!int.TryParse(partes[0], out int id)) return;
            if (!int.TryParse(partes[1], out int cantidad)) return;

            System.Diagnostics.Debug.WriteLine($"Actualizar ID {id} con cantidad {cantidad}");
            await ActualizarCantidadEnBaseDeDatos(id, cantidad);
        }
        private async Task ProcesarEliminarDetalle(string eventArgument)
        {
            if (string.IsNullOrWhiteSpace(eventArgument)) return;

            var partes = eventArgument.Split('|');
            if (partes.Length < 2) return;

            if (!int.TryParse(partes[0], out int id)) return;
            var nota = partes[1];

            await EliminarDetalle(id, nota);
        }

        // ------------------ método a añadir en tu clase ------------------
        private async Task ProcesarGuardarCuenta(string eventArgument)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(eventArgument)) return;

                // formato: mode|id|nombre
                var partes = eventArgument.Split(new char[] { '|' }, 3);
                if (partes.Length < 3) return;

                var mode = partes[0]; // "crear" o "editar"
                var idPart = partes[1];
                var nombre = partes[2]?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(nombre) || nombre.Length < 2)
                {
                    // puedes mostrar alerta al usuario
                    AlertModerno.Error(this, "Error", "Nombre inválido.", true);
                    return;
                }

                if (mode == "crear")
                {
                    await CrearCuenta(nombre);
                }
                else if (mode == "editar")
                {
                    if (!int.TryParse(idPart, out int idCuenta))
                    {
                        AlertModerno.Error(this, "Error", "ID de cuenta inválido.", true);
                        return;
                    }
                    await EditarCuenta(idCuenta, nombre);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ProcesarGuardarCuenta error: " + ex);
                AlertModerno.Error(this, "Error", "Ocurrió un error al guardar la cuenta.", true);
            }
        }

        #endregion

        #region Helpers: carga/bind/sesión

        private async Task CargarModelsDesdeSesion()
        {
            try
            {
                Models = Session[SessionModelsKey] as MenuViewModels;
                if (Models == null)
                {
                    // Reconstruir mínimamente si no hay sesión
                    await ReconstruirModelsBasico();
                }

                // Re-bind productos si existen
                BindProductos();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CargarModelsDesdeSesion error: " + ex);
            }
        }
        private async Task ReconstruirModelsBasico()
        {
            // Intento de reconstrucción conservadora para evitar nulls
            var vendedor = ObtenerVendedorDesdeSession();
            int vendedorId = vendedor?.id ?? (Session[SessionIdVendedorKey] != null ? Convert.ToInt32(Session[SessionIdVendedorKey]) : 0);
            var cuentas = await V_CuentasVentaControler.Lista_IdVendedor(Session["db"].ToString(), vendedorId) ?? new List<V_CuentasVenta>();
            int idCuenta = cuentas.FirstOrDefault()?.id ?? 0;

            Models = new MenuViewModels
            {
                IdCuentaActiva = idCuenta,
                cuentas = cuentas,
                zonas = await ZonasControler.Lista(Session["db"].ToString()) ?? new List<Zonas>(),
                Mesas = await MesasControler.Lista(Session["db"].ToString()) ?? new List<Mesas>(),
                categorias = await V_CategoriaControler.lista(Session["db"].ToString()) ?? new List<V_Categoria>(),
                productos = await v_productoVentaControler.Lista(Session["db"].ToString()) ?? new List<v_productoVenta>(),
                venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), idCuenta),
                detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), idCuenta, Models.IdCuenteClienteActiva)
            };

            GuardarModelsEnSesion();
        }
        private void BindProductos()
        {
            try
            {
                if (Models?.productos != null && Models.productos.Any())
                {
                    rpProductos.DataSource = Models.productos;
                }
                else
                {
                    rpProductos.DataSource = null;
                }
                rpProductos.DataBind();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("BindProductos error: " + ex);
            }
        }
        private void GuardarModelsEnSesion()
        {
            Session[SessionModelsKey] = Models;
        }

        #endregion

        #region Comandos de repeaters / botones

        protected async void rpServicios_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "AbrirServicio") return;

            if (!int.TryParse(Convert.ToString(e.CommandArgument), out int idServicio)) return;

            await CargarModelsDesdeSesion();
            Models.IdCuenteClienteActiva = 0;
            Models.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), idServicio);
            Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), idServicio, Models.IdCuenteClienteActiva);
            Models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(Session["db"].ToString(), false);
            Models.IdCuentaActiva = idServicio;
            Models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(Session["db"].ToString(), Models.IdCuenteClienteActiva);
            GuardarModelsEnSesion();

            BindProductos();
            DataBind();
        }

        protected async void rpZonas_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "CambiarZona") return;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out int idZona)) return;

            Session[SessionZonaActivaKey] = idZona;
            await CargarModelsDesdeSesion();
            Models.IdZonaActiva = idZona;
            GuardarModelsEnSesion();

            BindProductos();
            DataBind();
        }

        private async Task rpMesas_ItemCommand(string eventArgument)
        {
            await CargarModelsDesdeSesion();

            if (string.IsNullOrWhiteSpace(eventArgument)) return;

            var partes = eventArgument.Split('|');

            if (!int.TryParse(partes[0], out int idMesa)) return;

            var mesa = await MesasControler.Consultar_id(Session["db"].ToString(), idMesa);
            if (mesa == null)
            {
                AlertModerno.Error(this, "Error", "No se encontró la mesa.", true);
                return;
            }

            var cuentasMesa = await V_CuentasControler.Lista_Mesa(Session["db"].ToString(), mesa.nombreMesa) ?? new List<V_Cuentas>();
            if (cuentasMesa.Any())
            {
                // La mesa ya está asociada a una cuenta
                if (cuentasMesa.FirstOrDefault().idVendedor != Models.IdMesero)
                {
                    AlertModerno.Error(this, "Error", $"la mesa {mesa.nombreMesa} pertenece a otro mesero.", true);
                    GuardarModelsEnSesion();
                    BindProductos();
                    DataBind();
                    return;
                }
                Models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(Session["db"].ToString(), false);
                Models.IdCuentaActiva = cuentasMesa.First().id;
                Models.IdCuenteClienteActiva = 0;
                Models.IdMesaActiva = idMesa;
                Models.Mesas = await MesasControler.Lista(Session["db"].ToString());
                Models.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), Models.IdCuentaActiva);
                Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), Models.IdCuentaActiva, 0);
                GuardarModelsEnSesion();
                DataBind();
            }
            else
            {
                // Preguntar acción: nuevo servicio o amarrar a existente (usa AlertModerno con callbacks JS)
                string jsConfirm = $"document.getElementById('{btnMesaNuevaCuenta.ClientID}').click();";
                string jsDeny = $@"abrirModalServicios('{System.Web.HttpUtility.JavaScriptStringEncode(mesa.nombreMesa ?? $"MESA {idMesa}")}', '{idMesa}');";

                AlertModerno.ConfirmDual(
                    this,
                    $"Mesa - {mesa.nombreMesa}",
                    "¿Qué deseas hacer con esta mesa?",
                    jsConfirm,
                    jsDeny,
                    textoConfirm: "Nuevo servicio",
                    textoDeny: "Amarrar a existente",
                    textoCancel: "Cancelar"
                );
                Models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(Session["db"].ToString(), false);
                Models.IdMesaActiva = idMesa;
                Models.Mesas = await MesasControler.Lista(Session["db"].ToString());
                GuardarModelsEnSesion();
            }

            BindProductos();
            DataBind();
        }

        protected async void MesaNuevaCuenta(object sender, EventArgs e)
        {
            await CargarModelsDesdeSesion();

            var mesa = await MesasControler.Consultar_id(Session["db"].ToString(), Models.IdMesaActiva);
            if (mesa == null)
            {
                AlertModerno.Error(this, "Error", "No se encontró la mesa.", true, 2000);
                return;
            }

            int idVenta = await TablaVentas_f.NuevaVenta(Session["db"].ToString(), (int)Session["porpropina"]);
            if (idVenta <= 0)
            {
                AlertModerno.Error(this, "Error", "No se creó el servicio.", true, 2000);
                return;
            }

            Models.IdCuentaActiva = idVenta;

            // Relacionar venta - mesa
            var rvm = await R_VentaMesa_f.Relacionar_Venta_Mesa(Session["db"].ToString(), idVenta, Models.IdMesaActiva);
            if (!rvm)
            {
                AlertModerno.Error(this, "Error", $"Servicio #{idVenta} creado pero no se amarró la mesa {mesa.nombreMesa}", true, 2000);
            }
            else
            {
                mesa.estadoMesa = 1;
                await MesasControler.CRUD(Session["db"].ToString(), mesa, 1);

                // amarro venta con vendedor (uso session idvendedor si existe)
                var rvv = await R_VentaVendedor_f.Relacionar_Vendedor_Venta(Session["db"].ToString(), idVenta, ObtenerIdVendedorSeguro());
                if (rvv)
                {
                    AlertModerno.Success(this, "Listo", $"Servicio #{idVenta} creado para la mesa {mesa.nombreMesa}", true, 2000);
                }
                else
                {
                    AlertModerno.Error(this, "Error", $"Servicio #{idVenta} creado con éxito, pero no se amarró al vendedor.", true, 2000);
                }
            }

            // Actualizar modelos y UI
            Models.Mesas = await MesasControler.Lista(Session["db"].ToString());
            Models.cuentas = await V_CuentasVentaControler.Lista_IdVendedor(Session["db"].ToString(), ObtenerIdVendedorSeguro());
            Models.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), Models.IdCuentaActiva);
            Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), Models.IdCuentaActiva, Models.IdCuenteClienteActiva);

            GuardarModelsEnSesion();
            BindProductos();
            DataBind();
        }

        protected async void MesaAmarar(object sender, EventArgs e)
        {
            if (!int.TryParse(hfMesaId?.Value, out int idMesa)) return;
            if (!int.TryParse(hfServicioId?.Value, out int idServicio)) return;

            await CargarModelsDesdeSesion();

            var mesa = await MesasControler.Consultar_id(Session["db"].ToString(), idMesa);
            if (mesa == null)
            {
                AlertModerno.Error(this, "Error", "No se encontró la mesa.", true);
                return;
            }

            mesa.estadoMesa = 1;
            await MesasControler.CRUD(Session["db"].ToString(), mesa, 1);

            bool resp = await R_VentaMesa_f.Relacionar_Venta_Mesa(Session["db"].ToString(), idServicio, idMesa);
            if (resp)
            {
                AlertModerno.Success(this, "Amarrada", $"Mesa {mesa.nombreMesa} amarrada al servicio #{idServicio}.", true, 1200);
            }
            else
            {
                AlertModerno.Error(this, "Error", $"Mesa {mesa.nombreMesa} no fue amarrada al servicio #{idServicio}.", true, 1200);
            }

            Models.IdCuentaActiva = idServicio;
            Models.IdMesaActiva = idMesa;
            Models.cuentas = await V_CuentasVentaControler.Lista_IdVendedor(Session["db"].ToString(), ObtenerIdVendedorSeguro());
            Models.Mesas = await MesasControler.Lista(Session["db"].ToString());
            Models.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), idServicio);
            Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), idServicio, Models.IdCuenteClienteActiva);

            GuardarModelsEnSesion();
            BindProductos();
            DataBind();
        }

        protected async void rpProductos_ItemCommand(object source, RepeaterCommandEventArgs e)
        {

            if (e.CommandName != "AgregarAlCarrito") return;

            if (!int.TryParse(Convert.ToString(e.CommandArgument), out int idPresentacion))
            {
                // CommandArgument inválido
                return;
            }

            var txt = e.Item.FindControl("txtCantidad") as TextBox;
            int cantidad = 0;
            if (txt != null) int.TryParse(txt.Text, out cantidad);

            if (cantidad <= 0)
            {
                AlertModerno.Error(this, "¡Error!", $"la cantidad reportada es ({cantidad})", true);
                BindProductos();
                DataBind();
                return;
            }

            var resp = await DetalleVenta_f.AgregarProducto(Session["db"].ToString(), idPresentacion, cantidad, Models.IdCuentaActiva);
            if (resp.estado)
            {
                //como si se creo el produto ahora verificamos si esta activa una cuenta de cleinte
                if (Models.IdCuenteClienteActiva > 0)
                {
                    var ralacion = new R_CuentaCliente_DetalleVenta
                    {
                        id = 0,
                        fecha = DateTime.Now,
                        idCuentaCliente = Models.IdCuenteClienteActiva,
                        idDetalleVenta = (int)resp.data,
                        eliminada = false
                    };
                    var crudrelacion = await R_CuentaCliente_DetalleVentaControler.CRUD(Session["db"].ToString(), ralacion, 0);
                }
                AlertModerno.Success(this, "¡OK!", $"{resp.mensaje}", true, 800);
            }
            else
            {
                AlertModerno.Error(this, "¡Error!", $"{resp.mensaje}", true);
            }

            if (txt != null) txt.Text = "0";

            Models.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), Models.IdCuentaActiva);
            Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), Models.IdCuentaActiva, Models.IdCuenteClienteActiva);
            Models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(Session["db"].ToString(), false);
            Models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(Session["db"].ToString(), Models.IdCuenteClienteActiva);
            GuardarModelsEnSesion();
            BindProductos();
            DataBind();
        }


        #endregion

        #region Operaciones sobre detalle (cantidad / eliminar)

        private async Task ActualizarCantidadEnBaseDeDatos(int id, int cantidad)
        {
            try
            {
                await CargarModelsDesdeSesion();

                var respdal = await DetalleVenta_f.ActualizarCantidadDetalle(Session["db"].ToString(), id, cantidad);
                if (respdal.estado)
                {
                    AlertModerno.Success(this, "Ok", respdal.mensaje, true, 500);
                }
                else
                {
                    AlertModerno.Error(this, "Error", respdal.mensaje, true, 500);
                }

                Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), Models.IdCuentaActiva, Models.IdCuenteClienteActiva);
                Models.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), Models.IdCuentaActiva);
                Models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(Session["db"].ToString(), false);
                Models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(Session["db"].ToString(), Models.IdCuenteClienteActiva);
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ActualizarCantidadEnBaseDeDatos error: " + ex);
                AlertModerno.Error(this, "Error", "No se pudo actualizar la cantidad.", true);
            }
        }

        private async Task EliminarDetalle(int id, string nota)
        {
            try
            {
                await CargarModelsDesdeSesion();

                var respdal = await DetalleVenta_f.Eliminar(Session["db"].ToString(), id, nota);
                if (respdal.estado)
                {
                    AlertModerno.Success(this, "Ok", respdal.mensaje, true, 500);
                }
                else
                {
                    AlertModerno.Error(this, "Error", respdal.mensaje, true, 500);
                }

                Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), Models.IdCuentaActiva, Models.IdCuenteClienteActiva);
                Models.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), Models.IdCuentaActiva);
                Models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(Session["db"].ToString(), false);
                Models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(Session["db"].ToString(), Models.IdCuenteClienteActiva);
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EliminarDetalle error: " + ex);
                AlertModerno.Error(this, "Error", "No se pudo eliminar el detalle.", true);
            }
        }

        private async Task CrearCuenta(string nombreCuenta)
        {
            int idventa = Models.IdCuentaActiva;
            // TODO: reemplaza estas llamadas por tus funciones DAL reales
            // Ejemplo genérico: crear una nueva venta/tabla de cuentas y asignarle nombre
            int nuevoId = await CuentaCliente_f.Crear(Session["db"].ToString(), idventa, nombreCuenta, (int)Session["porpropina"]); // si ese es el flujo para crear 'cuenta'
            if (nuevoId <= 0)
            {
                AlertModerno.Error(this, "Error", "No fue posible crear la cuenta.", true);
                return;
            }

            // Si tienes un método para asignar nombre a la cuenta, usarlo:
            // bool ok = V_CuentasControler.AsignarNombreCuenta(nuevoId, nombreCuenta);
            // Si no, haz la llamada DAL que corresponda. Aquí dejo un placeholder:
            bool ok = true; // reemplaza

            if (ok)
            {
                // refrescar Models para que se vea en UI
                await CargarModelsDesdeSesion(); // o reconstruir Models
                                                 // Forzar recarga de cuentas desde BD
                Models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(Session["db"].ToString(), false);
                GuardarModelsEnSesion();
                BindProductos();
                AlertModerno.Success(this, "Creado", $"Cuenta creada con nombre '{nombreCuenta}'.", true, 1500);
            }
            else
            {
                AlertModerno.Error(this, "Error", "No fue posible asignar nombre a la cuenta.", true);
            }
        }

        private async Task EditarCuenta(int idCuenta, string nuevoNombre)
        {
            // TODO: reemplaza por la llamada real que actualice campo nombre
            // Ej: bool ok = V_CuentasControler.EditarNombre(idCuenta, nuevoNombre);
            bool ok = await CuentaCliente_f.Editar(Session["db"].ToString(), idCuenta, nuevoNombre); ; // placeholder

            if (ok)
            {
                // refrescar datos
                Models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(Session["db"].ToString(), false);
                GuardarModelsEnSesion();
                BindProductos();
                AlertModerno.Success(this, "Actualizado", $"Nombre de cuenta actualizado.", true, 1200);
            }
            else
            {
                AlertModerno.Error(this, "Error", "No fue posible actualizar la cuenta.", true);
            }
        }

        #endregion

        #region Utilidades

        private int ObtenerIdVendedorSeguro()
        {
            try
            {
                var vendedor = ObtenerVendedorDesdeSession();
                if (vendedor != null) return vendedor.id;
                if (Session[SessionIdVendedorKey] != null) return Convert.ToInt32(Session[SessionIdVendedorKey]);
            }
            catch { /* ignorar */ }
            return 0;
        }
        private async Task BTN_NuevoServicio()
        {
            int idVenta = await TablaVentas_f.NuevaVenta(Session["db"].ToString(), (int)Session["porpropina"]);
            if (idVenta <= 0)
            {
                AlertModerno.Error(this, "Error", "No se creó el servicio.", true, 2000);
                return;
            }
            else
            {
                // procedemos a modificar el alias
                var resp = await TablaVentasControler.Consultar_Id(Session["db"].ToString(), idVenta);
                if (resp.estado)
                {
                    var venta = resp.data as TablaVentas;
                    venta.aliasVenta = Convert.ToString(idVenta);
                    var crud = await TablaVentasControler.CRUD(Session["db"].ToString(), venta, 1);
                }
            }
            Models.IdCuentaActiva = idVenta;

            // amarro venta con vendedor (uso session idvendedor si existe)
            var rvv = await R_VentaVendedor_f.Relacionar_Vendedor_Venta(Session["db"].ToString(), idVenta, ObtenerIdVendedorSeguro());
            if (rvv)
            {
                AlertModerno.Success(this, "Listo", $"Servicio #{idVenta} creado con éxito.", true, 2000);
            }
            else
            {
                AlertModerno.Error(this, "Error", $"Servicio #{idVenta} creado con éxito, pero no se amarró al vendedor.", true, 2000);
            }


            // Actualizar modelos y UI
            Models.IdCuenteClienteActiva = 0;
            Models.cuentas = await V_CuentasVentaControler.Lista_IdVendedor(Session["db"].ToString(), ObtenerIdVendedorSeguro());
            Models.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), Models.IdCuentaActiva);
            Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), Models.IdCuentaActiva, Models.IdCuenteClienteActiva);
            Models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(Session["db"].ToString(), false, Models.IdCuentaActiva);
            Models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(Session["db"].ToString(), Models.IdCuenteClienteActiva);
            GuardarModelsEnSesion();
            BindProductos();
            DataBind();
        }

        #endregion

        private async Task btnEliminarServicio()
        {
            int idventa = Models.IdCuentaActiva;

            //consultamos si la venta tiene detalle cargados
            var detalles = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), idventa, Models.IdCuenteClienteActiva);
            if (detalles != null && detalles.Count > 0)
            {
                AlertModerno.Error(this, "Error", $"El servicio #{idventa} aun tiene items cargados.", true);
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
                return;
            }

            var rsp = await TablaVentasControler.Consultar_Id(Session["db"].ToString(), idventa);
            if (!rsp.estado)
            {
                AlertModerno.Error(this, "Error", $"El servicio #{idventa} no se pudo eliminar.", true);
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
                return;
            }

            var venta = rsp.data as TablaVentas;
            venta.eliminada = true;
            var rsp_crud = await TablaVentasControler.CRUD(Session["db"].ToString(), venta, 1);
            if (!rsp_crud.estado)
            {
                AlertModerno.Error(this, "Error", $"El servicio #{idventa} no se pudo eliminar.", true);
                GuardarModelsEnSesion();
                BindProductos();
                DataBind();
                return;
            }

            AlertModerno.Success(this, "OK", $"Servicio #{idventa} eliminado con éxito.");
            await InicializarPagina(Session["db"].ToString());
        }

        protected async void btnGuardarAlias_Click(object sender, EventArgs e)
        {
            try
            {
                var idCuenta = hfCuentaId.Value;
                var nuevoAlias = txtAlias.Text?.Trim() ?? "";
                var venta = new TablaVentas();
                var resp = await TablaVentasControler.Consultar_Id(Session["db"].ToString(), Convert.ToInt32(idCuenta));
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", $"no se encontro la venta {idCuenta}");
                }
                else
                {
                    venta = resp.data as TablaVentas;
                }
                venta.aliasVenta = nuevoAlias;
                var crud = await TablaVentasControler.CRUD(Session["db"].ToString(), venta, 1);
                if (!crud.estado)
                {
                    AlertModerno.Error(this, "Error", $"no se modifico el alias");
                }
                else
                {
                    AlertModerno.Success(this, "OK", $"alias modificado correctamente.", true);
                }
                Models.IdCuentaActiva = Convert.ToInt32(idCuenta);
                Models.cuentas = await V_CuentasVentaControler.Lista_IdVendedor(Session["db"].ToString(), Convert.ToInt32(Session["idvendedor"]));
                // Recarga datos y rebind
                await CargarModelsDesdeSesion(); // si usas este patrón
                DataBind();
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true);
            }
        }

        protected async void btnCuentaGeneral_Click(object sender, EventArgs e)
        {
            await CargarModelsDesdeSesion();

            Models.IdCuenteClienteActiva = 0;
            Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), Models.IdCuentaActiva, 0);
            Models.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), Models.IdCuentaActiva);

            BindProductos();
            GuardarModelsEnSesion();
            DataBind();
        }

        private class EditarPropinaDto
        {
            public decimal porcentaje { get; set; }
            public int propina { get; set; } // en COP enteros
            public int idventa { get; set; }
            public int idcuenta { get; set; }
        }
        private async Task btnEditarPropina(string json)
        {
            await CargarModelsDesdeSesion();

            if (!string.IsNullOrWhiteSpace(json))
            {
                var dto = JsonConvert.DeserializeObject<EditarPropinaDto>(json);
                decimal por_propina = dto.porcentaje / 100;
                if (dto.idcuenta > 0)
                {
                    //hallamos la cuenta cliente con el id
                    var cc = await CuentaClienteControler.CuentaCliente(Session["db"].ToString(), dto.idcuenta);
                    if (cc != null)
                    {
                        cc.por_propina = por_propina;
                        cc.propina = dto.propina;
                        var respcc = await CuentaClienteControler.CRUD(Session["db"].ToString(), cc, 1);
                    }
                }
                else
                {
                    var respuesta = await TablaVentasControler.Consultar_Id(Session["db"].ToString(), dto.idventa);
                    if (respuesta.estado)
                    {
                        var venta = respuesta.data as TablaVentas;
                        venta.porpropina = por_propina;
                        venta.propina = dto.propina;
                        var respventa = await TablaVentasControler.CRUD(Session["db"].ToString(), venta, 1);
                    }
                }

                Models.IdCuenteClienteActiva = dto.idcuenta;
                Models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(Session["db"].ToString(), Models.IdCuentaActiva, dto.idcuenta);
                Models.venta = await V_TablaVentasControler.Consultar_Id(Session["db"].ToString(), Models.IdCuentaActiva);
                Models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(Session["db"].ToString(), dto.idcuenta);
                Models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(Session["db"].ToString(), false);
                BindProductos();
                GuardarModelsEnSesion();
                DataBind();
            }

            BindProductos();
            GuardarModelsEnSesion();
            DataBind();
        }

        protected async void btnComandar_ServerClick(object sender, EventArgs e)
        {
            await CargarModelsDesdeSesion();
            var comanda = new ImprecionComandaAdd
            {
                id = 0,
                idVenta = Models.IdCuentaActiva,
                idMesa = Convert.ToString(Models.IdMesaActiva),
                idMesero = Convert.ToString(Models.IdMesero),
                estado = 1
            };
            var resp = await ImprecionComandaAddControler.CRUD(Session["db"].ToString(), comanda, 0);
            if (resp.estado)
            {
                AlertModerno.Success(this, "Ok", "Comanda enviada correctamente.", true, 1500);
            }
            else
            {
                AlertModerno.Error(this, "Error", "Comanda no enviada correctamente.", true, 1500);
            }

            GuardarModelsEnSesion();
            BindProductos();
            DataBind();
        }

        protected async void btnCuenta_ServerClick(object sender, EventArgs e)
        {
            await CargarModelsDesdeSesion();
            var cuenta = new ImprimirCuenta
            {
                id = 0,
                idVenta = Models.IdCuentaActiva
            };
            var resp = await ImprimirCuentaControler.CRUD(Session["db"].ToString(), cuenta, 0);
            if (resp.estado)
            {
                AlertModerno.Success(this, "Ok", "Cuenta enviada correctamente.", true, 1500);
            }
            else
            {
                AlertModerno.Error(this, "Error", "Cuenta no enviada correctamente.", true, 1500);
            }

            GuardarModelsEnSesion();
            BindProductos();
            DataBind();
        }


        protected async void btnActualizar_Click(object sender, EventArgs e)
        {
            await InicializarPagina(Session["db"].ToString()); // 🔁 Llama directamente tu método
        }



        private async Task btnDomicilio(string eventArgument)
        {
            await CargarModelsDesdeSesion();

            // 1) Validar argumento
            if (string.IsNullOrWhiteSpace(eventArgument))
            {
                AlertModerno.Error(this, "Error", "No hay una mesa seleccionada.", true);
                return;
            }

            // formato: idMesa|idServicio
            var parts = eventArgument.Split('|');

            if (!int.TryParse(parts[0], out int idMesa))
            {
                AlertModerno.Error(this, "Error", "Mesa inválida.", true);
                return;
            }

            int idServicio = Models.IdCuentaActiva;
            if (parts.Length > 1)
                int.TryParse(parts[1], out idServicio);

            // 2) Consultar mesa
            var mesa = await MesasControler.Consultar_id(Session["db"].ToString(), idMesa);

            if (mesa == null)
            {
                AlertModerno.Error(this, "Error", "No se encontró la mesa.", true);
                return;
            }

            // 3) Verificar que sea DOMICILIO
            if (string.IsNullOrWhiteSpace(mesa.nombreMesa) ||
                !mesa.nombreMesa.ToUpper().Contains("DOMICILIO"))
            {
                AlertModerno.Error(this, "Error", "La mesa seleccionada NO es domicilio.", true);
                return;
            }

            // 4) Actualizar modelo
            Models.IdMesaActiva = idMesa;
            Models.IdCuentaActiva = idServicio;


            GuardarModelsEnSesion();

            // 5) Hacer DataBind ANTES del script
            BindProductos();
            DataBind();


            // 🔥 Activar el flag SOLO para esta respuesta (NO lo vuelvas a guardar en sesión)
            Models.AbrirModalDomicilio = true;
        }











        private async Task btnCrearActualizarClienteDomicilio(string eventArgument)
        {
            await CargarModelsDesdeSesion();

            if (string.IsNullOrWhiteSpace(eventArgument)) return;

            // id|tel|nom|dir
            var parts = eventArgument.Split('|');
            if (parts.Length < 4) return;

            string idStr = parts[0];
            string tel = parts[1];
            string nom = parts[2];
            string dir = parts[3];

            Guid id;
            bool esNuevo;

            if (string.IsNullOrWhiteSpace(idStr))
            {
                esNuevo = true;
                id = Guid.NewGuid();
            }
            else
            {
                esNuevo = false;
                id = new Guid(idStr);
            }

            var entidad = new ClienteDomicilio
            {
                id = id,
                celularCliente = tel,
                nombreCliente = nom,
                direccionCliente = dir
            };

            int funcion = esNuevo ? 0 : 1;

            // Guardar en BD
            var resp = await ClienteDomicilioControler.CRUD(Session["db"].ToString(), entidad, funcion);

            if (!resp.estado)
            {
                AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo guardar el cliente.", true);
                return;
            }

            // Actualizar ID con el que devuelve la BD (por si lo genera allá)
            string idguid = $"{resp.idAfectado}";
            if (!string.IsNullOrWhiteSpace(idguid))
            {
                entidad.id = new Guid(idguid);
            }

            AlertModerno.Success(this, "OK", resp.mensaje ?? "Cliente guardado correctamente.", true, 1200);

            // Recargar lista desde BD
            Models.clienteDomicilios = await ClienteDomicilioControler.Lista(Session["db"].ToString());

            // 🔴 CLAVE: volver a abrir el modal después del postback
            Models.AbrirModalDomicilio = true;

            GuardarModelsEnSesion();
        }






        private async Task btnSeleccionarClienteDomicilio(string eventArgument)
        {
            await CargarModelsDesdeSesion();

            if (string.IsNullOrWhiteSpace(eventArgument)) return;

            // id|tel|nom|dir
            var parts = eventArgument.Split('|');
            if (parts.Length < 4) return;

            string idStr = parts[0];
            string tel = parts[1];
            string nom = parts[2];
            string dir = parts[3];

            if (!Guid.TryParse(idStr, out Guid idCliente))
            {
                AlertModerno.Error(this, "Error", "ID de cliente inválido.", true);
                return;
            }

            int idVenta = Models.IdCuentaActiva;
            int funcion = 0;

            //en esta parte consultamos si la relacion ya existe
            var consultarRelacion = await ClienteDomicilioControler.ConsultarRelacion(Session["db"].ToString(),idVenta);
            if (consultarRelacion == null)
            {
                funcion = 0;
                consultarRelacion = new R_VentaClienteDomicilio { id=0, idVenta=idVenta, idClienteDomicilio=new Guid(idStr) };
            }
            else
            {
                funcion = 1;
                consultarRelacion.idClienteDomicilio = new Guid(idStr);
            }

            // Aquí haces tu relación cliente-venta en tu DAL
            var resp = await ClienteDomicilioControler.RelacionarConVenta(
                Session["db"].ToString(),
                consultarRelacion,
                funcion
            );

            if (!resp.estado)
            {
                AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo relacionar el cliente con la venta.", true);
                return;
            }

            AlertModerno.Success(this, "OK", resp.mensaje ?? "Cliente relacionado con la venta.", true, 1200);

            // Cerrar modal en el cliente
            string scriptCerrar = @"
(function(){
    var modalEl = document.getElementById('modalDomicilio');
    if (modalEl && window.bootstrap) {
        var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modal.hide();
    }
})();
";
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "CerrarModalDomicilioDespuesSeleccionar",
                scriptCerrar,
                true
            );

            //aquí, si quieres, recargas datos de la venta
            Models.cuentas = await V_CuentasVentaControler.Lista_IdVendedor(Session["db"].ToString(),Models.IdMesero);
            GuardarModelsEnSesion();
            BindProductos();
            DataBind();
        }




        protected string MostrarNombreCliente(object nombreCD, object nombreMesa)
        {
            string cd = nombreCD == null ? "" : nombreCD.ToString();
            string mesa = nombreMesa == null ? "" : nombreMesa.ToString();

            // Si nombreCD no es "-"
            if (!string.IsNullOrWhiteSpace(cd) && cd != "-")
                return cd;

            // Si es "-" o vacío, mostrar nombre de mesa
            return mesa;
        }


        private async Task btnCerrarCaja()
        {
            var basecajaJson = Session["base_caja"]?.ToString();
            if (!string.IsNullOrEmpty(basecajaJson))
            {
                var basecaja = JsonConvert.DeserializeObject<BaseCaja>(basecajaJson);
                if (basecaja != null && basecaja.estadoBase == "ACTIVA")
                {
                    basecaja.estadoBase = "CERRADA";
                    var resp = await BaseCajaControler.CRUD(Session["db"]?.ToString(), basecaja, 1);
                    if (!resp)
                    {
                        await InicializarPagina(Session["db"].ToString()); // 🔁 Llama directamente tu método

                        AlertModerno.Error(this, "Error", "No fue posible cerrar la caja. Verifique e intente nuevamente.", true);

                        return;
                    }
                    else
                    {
                        // en esta parte enviamos el informe de cierre de caja a los administradores
                        // 1- consultamos la configuracon del SMTP

                        // 2- consultamos la lista de los correos 

                        // 3- consultamos la lista de los whatsapp

                        // 4- enviamos el informe por los medios configurados



                        await InicializarPagina(Session["db"].ToString()); // 🔁 Llama directamente tu método

                        AlertModerno.SuccessGoTo(this, "Ok", $"Caja cerrada con exito", "~/Salir.aspx", esToast: false, ms: 1200);
                    }
                }
            }

        }
    }
}
