using DAL;
using DAL.Controler;
using DAL.Funciones;
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.UI;
using WebApplication.Class;
using WebApplication.Helpers;
using WebApplication.ViewModels;

namespace WebApplication
{
    public partial class caja : System.Web.UI.Page
    {
        protected bool AutoFocusBusquedaDesktop()
        {
            return true;
        }

        protected int DesktopMinWidth()
        {
            return 992;
        }

        private const string ok = "Ok";
        private const string SessionVistaCaja = "CajaVistaActual";
        private const string VistaCaja = "caja";
        private const string VistaVentas = "ventas";
        private const string SessionModelsJson = "ModelsJson";
        protected MenuViewModels models = new MenuViewModels();
        protected List<V_TablaVentas> VentasCaja = new List<V_TablaVentas>();
        protected decimal VentasCajaTotal;
        protected int VentasCajaCantidad;
        protected decimal VentasCajaPendiente;
        protected int VentasCajaAnuladas;

        protected bool EnVistaVentas()
        {
            return string.Equals(Convert.ToString(Session[SessionVistaCaja] ?? VistaCaja), VistaVentas, StringComparison.OrdinalIgnoreCase);
        }

        protected string TextoBotonVentas()
        {
            return EnVistaVentas() ? "Caja" : "Ventas";
        }

        protected string AccionBotonVentas()
        {
            return EnVistaVentas() ? "VerCaja" : "Ventas";
        }

        protected string MonedaVistaVentas(decimal valor)
        {
            return valor.ToString("C0");
        }

        protected string FacturaLabelVista(V_TablaVentas venta)
        {
            if (!string.IsNullOrWhiteSpace(venta?.prefijo) && venta.numeroVenta > 0)
            {
                return venta.prefijo + "-" + venta.numeroVenta.ToString("000000");
            }
            if (venta?.numeroVenta > 0)
            {
                return venta.numeroVenta.ToString();
            }
            return "Sin numerar";
        }

        protected bool EsVentaAnulada(V_TablaVentas venta)
        {
            return string.Equals(venta?.estadoVenta, "CANCELADO", StringComparison.OrdinalIgnoreCase)
                || string.Equals(venta?.estadoVenta, "ANULADA", StringComparison.OrdinalIgnoreCase);
        }

        protected string EstadoVentaVista(V_TablaVentas venta)
        {
            if (EsVentaAnulada(venta)) return "Anulada";
            if ((venta?.totalPendienteVenta ?? 0) > 0) return "Pendiente";
            return "Pagada";
        }

        private void EstablecerVistaCaja(string vista)
        {
            Session[SessionVistaCaja] = string.IsNullOrWhiteSpace(vista) ? VistaCaja : vista;
        }

        private async Task CargarVistaVentasCaja()
        {
            VentasCaja = new List<V_TablaVentas>();
            VentasCajaTotal = 0;
            VentasCajaCantidad = 0;
            VentasCajaPendiente = 0;
            VentasCajaAnuladas = 0;

            var db = models?.db ?? Convert.ToString(Session[SessionContextHelper.DbKey]);
            var idBase = SessionContextHelper.ResolveBaseCajaId(Session, models);
            if (string.IsNullOrWhiteSpace(db) || idBase <= 0)
            {
                return;
            }

            var dal = new SqlAutoDAL();
            VentasCaja = await dal.ConsultarLista<V_TablaVentas>(db, x => x.idBaseCaja == idBase && x.eliminada == false) ?? new List<V_TablaVentas>();
            VentasCaja = VentasCaja.OrderByDescending(x => x.fechaVenta).ToList();

            VentasCajaTotal = VentasCaja.Where(x => !EsVentaAnulada(x)).Sum(x => x.total_A_Pagar);
            VentasCajaCantidad = VentasCaja.Count(x => x.numeroVenta > 0);
            VentasCajaPendiente = VentasCaja.Where(x => !EsVentaAnulada(x)).Sum(x => x.totalPendienteVenta);
            VentasCajaAnuladas = VentasCaja.Count(EsVentaAnulada);
        }

        protected bool PuedeEliminarServicioActivo()
        {
            return models?.IdCuentaActiva > 0 && (models.detalleCaja == null || !models.detalleCaja.Any());
        }

        protected IEnumerable<V_CuentaCliente> CuentasClienteActivas()
        {
            return (models?.v_CuentaClientes ?? new List<V_CuentaCliente>())
                .Where(x => x.idVenta == models.IdCuentaActiva && !x.eliminada)
                .OrderBy(x => x.fecha);
        }

        protected bool TieneDetalleServicioActivo()
        {
            return models?.detalleCaja != null && models.detalleCaja.Any();
        }

        protected decimal ResumenSubtotal()
        {
            return models.IdCuenteClienteActiva > 0 ? models.ventaCuenta?.subtotalVenta ?? 0 : models.venta?.subtotalVenta ?? 0;
        }

        protected decimal ResumenImpuestos()
        {
            return models.IdCuenteClienteActiva > 0 ? models.ventaCuenta?.ivaVenta ?? 0 : models.venta?.ivaVenta ?? 0;
        }

        protected decimal ResumenTotal1()
        {
            return models.IdCuenteClienteActiva > 0 ? models.ventaCuenta?.totalVenta ?? 0 : models.venta?.totalVenta ?? 0;
        }

        protected decimal ResumenPropina()
        {
            return models.IdCuenteClienteActiva > 0 ? models.ventaCuenta?.propina ?? 0 : models.venta?.propina ?? 0;
        }

        protected decimal ResumenPorcentajePropina()
        {
            var valor = models.IdCuenteClienteActiva > 0 ? models.ventaCuenta?.por_propina ?? 0 : models.venta?.por_propina ?? 0;
            return valor * 100m;
        }

        protected decimal ResumenTotal2()
        {
            return models.IdCuenteClienteActiva > 0 ? models.ventaCuenta?.total_A_Pagar ?? 0 : models.venta?.total_A_Pagar ?? 0;
        }

        protected string NombreCuentaClienteActiva()
        {
            if (models.IdCuenteClienteActiva <= 0)
            {
                return "Cuenta General";
            }

            return models.ventaCuenta?.nombreCuenta ?? $"Cuenta #{models.IdCuenteClienteActiva}";
        }

        protected string FormatearMoneda(object valor)
        {
            decimal numero;
            return decimal.TryParse(Convert.ToString(valor), out numero) ? numero.ToString("C0") : "$ 0";
        }
        protected string ClienteDomiciliosJson()
        {
            var lista = models?.clienteDomicilios ?? new List<ClienteDomicilio>();
            return JsonConvert.SerializeObject(lista).Replace("</", "<\\/");
        }
        protected string AdicionesCatalogoJson()
        {
            var lista = models?.adiciones ?? new List<V_CatagoriaAdicion>();
            return JsonConvert.SerializeObject(lista).Replace("</", "<\\/");
        }
        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!await DeserializarModels())
                {
                    AlertModerno.ErrorRedirect(this, "Error", "La sesion expiro o no contiene el contexto de trabajo.", "Default.aspx");
                    return;
                }
                //antes de iniciar verificamos que halla session activa 
                var resp = await VerificarSession();
                if (!resp)
                {
                    //retornamos para la pagina de login
                    AlertModerno.ErrorRedirect(this, "Error", "Aun no hay session activa.", "Default.aspx");
                    return;
                }

                await IniciarPagina();
            }
        }
        private async Task<bool> DeserializarModels()
        {
            var modelJson = Session[SessionModelsJson]?.ToString();
            if (string.IsNullOrWhiteSpace(modelJson))
            {
                return false;
            }

            models = JsonConvert.DeserializeObject<MenuViewModels>(modelJson);
            return models != null;
        }
        private async Task CargarDATA()
        {
            await Cargar_RP();
            Session[SessionModelsJson] = JsonConvert.SerializeObject(models);

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "CajaPostRenderConfig",
                $@"
        window.CajaConfig = window.CajaConfig || {{}};
        window.CajaConfig.autoFocusBusquedaDesktop = {(AutoFocusBusquedaDesktop() ? "true" : "false")};
        window.CajaConfig.desktopMinWidth = {DesktopMinWidth()};
        window.CajaConfig.preservarPosicionEnMobile = true;

        if (window.CajaViewport) {{
            if (typeof window.CajaViewport.restaurarEstadoScroll === 'function') {{
                window.CajaViewport.restaurarEstadoScroll();
            }}
            if (typeof window.CajaViewport.activarFocusBuscadorSiAplica === 'function') {{
                window.CajaViewport.activarFocusBuscadorSiAplica();
            }}
        }}
        ",
                true
            );
        }
        private async Task Cargar_RP()
        {
            rpCuentas.DataSource = models.cuentas;
            rpCuentasModal.DataSource = models.cuentas;
            rpCuentasCliente.DataSource = CuentasClienteActivas();
            rpDetalleCaja.DataSource = models.detalleCaja;
            rpZonas.DataSource = models.zonas;
            rpMesas.DataSource = models.Mesas;
            rpCategorias.DataSource = models.categorias;
            rpProductos.DataSource = models.productosLista ?? models.productos;
            DataBind();
        }

        private async Task<bool> VerificarSession()
        {
            if (models.vendedor.id != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private async Task IniciarPagina()
        {
            // Obtener cuentas del vendedor
            var cuentas = new List<V_CuentasVenta>();
            cuentas = await CargarCuentas();

            int idVenta;
            if (!cuentas.Any())
            {
                idVenta = await TablaVentas_f.NuevaVenta(models.db, models.Sede.porcentaje_propina);
                if (idVenta <= 0)
                {
                    AlertModerno.Error(this, "Error", "No fue posible crear una nueva cuenta.", true);
                    return;
                }

                var relacionado = await R_VentaVendedor_f.Relacionar_Vendedor_Venta(models.db, idVenta, models.vendedor.id);
                if (!relacionado)
                {
                    AlertModerno.Error(this, "Error", "No fue posible crear la relaci\u00f3n del vendedor con la venta.", true);
                    // a\u00fan as\u00ed intentamos recargar cuentas para que la UI no quede rota
                }

                // recargar cuentas
                cuentas = await V_CuentasVentaControler.Lista_IdVendedor(models.db, models.vendedor.id) ?? new List<V_CuentasVenta>();
            }
            else
            {
                idVenta = cuentas.First().id;
            }

            // Cargar colecciones base
            var zonas = await ZonasControler.Lista(models.db) ?? new List<Zonas>();
            if (!zonas.Any())
            {
                divZonas.Attributes["class"] = "d-none";
                divProductos.Attributes["class"] = "col-12 col-lg-12";
            }
            else
            {
                divZonas.Attributes["class"] = "col-12 col-lg-5 d-flex";
                divProductos.Attributes["class"] = "col-12 col-lg-7";
            }
            var categorias = await V_CategoriaControler.lista(models.db) ?? new List<V_Categoria>();
            var mesas = await MesasControler.Lista(models.db) ?? new List<Mesas>();
            var productos = await v_productoVentaControler.Lista(models.db) ?? new List<v_productoVenta>();
            if (!productos.Any())
            {
                AlertModerno.Error(this, "Error", "No fue posible cargar la lista de productos.", true);
            }
            int idZonaActiva = zonas.FirstOrDefault()?.id ?? 0;
            int idCategoriaActiva = categorias.FirstOrDefault()?.id ?? 0;
            var listacc = await V_CuentaClienteCotroler.Lista(models.db, false);

            // Construir ViewModel
            models.IdCuentaActiva = idVenta;
            if (zonas != null && zonas.Any())
            {
                models.IdZonaActiva = zonas.First().id;
            }
            else
            {
                models.IdZonaActiva = 0;
            }
            if (mesas != null && mesas.Any())
            {
                models.IdMesaActiva = mesas.First().id;
            }
            else
            {
                models.IdMesaActiva = 0;
            }
            if (categorias != null && categorias.Any())
            {
                models.IdCategoriaActiva = categorias.First().id;
            }
            else
            {
                models.IdCategoriaActiva = 0;
            }
            models.IdCuenteClienteActiva = 0;
            models.cuentas = cuentas;
            models.zonas = zonas;
            models.MesasLista = mesas;
            models.Mesas = mesas.Where(x => x.idZona == models.IdZonaActiva).ToList();
            models.categorias = categorias;
            models.productosLista = productos;
            models.productos = productos;
            models.venta = await V_TablaVentasControler.Consultar_Id(models.db, idVenta);
            models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(models.db, 0);
            models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(models.db, idVenta, 0);
            models.v_CuentaClientes = listacc;
            models.adiciones = await V_CatagoriaAdicionControler.Lista(models.db);
            models.clienteDomicilios = await ClienteDomicilioControler.Lista(models.db);
            models.AbrirModalDomicilio = false;
            models.cargoDescuentoVentas = await CargoDescuentoVentasControler.ObtenerPorVenta(models.db, idVenta);
            models.clientes = await ClientesControler.ListaClientes(models.db);
            models.cuentas = cuentas;
            models.IdCuentaActiva = idVenta;



            await CargarDATA();
        }
        private async Task<List<V_CuentasVenta>> CargarCuentas()
        {
            var cuentas = new List<V_CuentasVenta>();
            if (models.vendedor.cajaMovil == 1)
            {
                cuentas = await V_CuentasVentaControler.Lista_Cajero(models.db) ?? new List<V_CuentasVenta>();
            }
            else
            {
                cuentas = await V_CuentasVentaControler.Lista_IdVendedor(models.db, models.vendedor.id) ?? new List<V_CuentasVenta>();
            }
            return (cuentas ?? new List<V_CuentasVenta>()).Where(x => !x.eliminada).ToList();
        }
        protected async void Evento_Click(object sender, EventArgs e)
        {
            if (!await DeserializarModels())
            {
                AlertModerno.ErrorRedirect(this, "Error", "La sesion expiro o no contiene el contexto de trabajo.", "Default.aspx");
                return;
            }

            string accion = hidAccion.Value;
            string eventArgument = hidArgumento.Value;

            switch (accion)
            {
                case "Actualizar":
                    await Actualizar();
                    break;

                case "NuevoServicio":
                    await NuevoServicio();
                    break;

                case "EliminarServicio":
                    await EliminarServicio();
                    break;

                case "SeleccionarCuenta":
                    await SeleccionarCuenta(eventArgument);
                    break;

                case "EditarAliasCuenta":
                    await EditarAliasCuenta(eventArgument);
                    break;

                case "SeleccionarZona":
                    await SeleccionarZona(eventArgument);
                    break;

                case "SeleccionarMesa":
                    await SeleccionarMesa(eventArgument);
                    break;

                case "AccionMesa_CrearServicio":
                    await AccionMesa_CrearServicio();
                    break;

                case "AccionMesa_AmarrarMesa":
                    await AccionMesa_AmarrarMesa();
                    break;

                case "LiberarMesa":
                    await LiberarMesa();
                    break;

                case "AmarrarMesaCuenta":
                    await AmarrarMesaCuenta(eventArgument);
                    break;

                case "SeleccionarCategoria":
                    await SeleccionarCategoria(eventArgument);
                    break;


                case "BuscarCodigoProducto":
                    await BuscarCodigoProducto(eventArgument);
                    break;

                case "AgregarProducto":
                    await AgregarProducto(eventArgument);
                    break;

                case "SeleccionarCuentaCliente":
                    await SeleccionarCuentaCliente(eventArgument);
                    break;

                case "CrearCuentaCliente":
                    await CrearCuentaCliente(eventArgument);
                    break;

                case "ActualizarCantidadDetalle":
                    await ActualizarCantidadDetalle(eventArgument);
                    break;

                case "EliminarDetalle":
                    await EliminarDetalleCaja(eventArgument);
                    break;

                case "GuardarNotaDetalle":
                    await GuardarNotaDetalle(eventArgument);
                    break;

                case "DividirDetalle":
                    await DividirDetalle(eventArgument);
                    break;

                case "AnclarDetalleCuenta":
                    await AnclarDetalleCuenta(eventArgument);
                    break;

                case "EditarValorDetalle":
                    await EditarValorDetalle(eventArgument);
                    break;

                case "EditarNombreDetalle":
                    await EditarNombreDetalle(eventArgument);
                    break;

                case "EditarPropina":
                    await EditarPropina(eventArgument);
                    break;

                case "Domicilio":
                    await Domicilio(eventArgument);
                    break;

                case "CrearActualizarClienteDomicilio":
                    await CrearActualizarClienteDomicilio(eventArgument);
                    break;

                case "SeleccionarClienteDomicilio":
                    await SeleccionarClienteDomicilio(eventArgument);
                    break;

                case "Comandar":
                    await Comandar();
                    break;

                case "SolicitarCuenta":
                    await SolicitarCuenta();
                    break;

                case "Cobrar":
                    await Cobrar();
                    break;

                case "CerrarCaja":
                    await CerrarCaja();
                    break;

                case "CerrarSesion":
                    await CerrarSesion();
                    break;

                case "Ventas":
                    await Ventas();
                    break;
            }
        }
        private async Task Actualizar()
        {
            await IniciarPagina();
            AlertModerno.Success(this, "Ok", "Evento Actualizar", true);
        }
        private async Task NuevoServicio()
        {
            int idVenta = await TablaVentas_f.NuevaVenta(models.db, models.Sede.porcentaje_propina);
            if (idVenta <= 0)
            {
                AlertModerno.Error(this, "Error", "No se cre\u00f3 el servicio.", true, 2000);
                return;
            }
            else
            {
                // procedemos a modificar el alias
                var resp = await TablaVentasControler.Consultar_Id(models.db, idVenta);
                if (resp.estado)
                {
                    var venta = resp.data as TablaVentas;
                    venta.aliasVenta = Convert.ToString(idVenta);
                    var crud = await TablaVentasControler.CRUD(models.db, venta, 1);
                }
            }


            // amarro venta con vendedor (uso session idvendedor si existe)
            var rvv = await R_VentaVendedor_f.Relacionar_Vendedor_Venta(models.db, idVenta, models.vendedor.id);
            if (rvv)
            {
                // Actualizar modelos y UI
                models.IdCuenteClienteActiva = 0;
                models.IdCuentaActiva = idVenta;
                models.cuentas = await CargarCuentas();
                models.venta = await V_TablaVentasControler.Consultar_Id(models.db, models.IdCuentaActiva);
                models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(models.db, models.IdCuentaActiva, models.IdCuenteClienteActiva);
                models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(models.db, false, models.IdCuentaActiva);
                models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(models.db, models.IdCuenteClienteActiva);

                await CargarDATA();
                AlertModerno.Success(this, "Listo", $"Servicio #{idVenta} creado con \u00e9xito.", true, 2000);
            }
            else
            {
                AlertModerno.Error(this, "Error", $"Servicio #{idVenta} creado con \u00e9xito, pero no se amarr\u00f3 al vendedor.", true, 2000);
            }

        }

        private async Task EliminarServicio()
        {
            int idVenta = models.IdCuentaActiva;
            if (idVenta <= 0)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "No hay un servicio activo para eliminar.", true, 2200);
                return;
            }

            var detalles = await V_DetalleCajaControler.Lista_IdVenta(models.db, idVenta, models.IdCuenteClienteActiva);
            if (detalles != null && detalles.Count > 0)
            {
                AlertModerno.Error(this, "Error", $"El servicio #{idVenta} a\u00fan tiene items cargados.", true, 2200);
                return;
            }

            var rsp = await TablaVentasControler.Consultar_Id(models.db, idVenta);
            if (!rsp.estado)
            {
                AlertModerno.Error(this, "Error", $"El servicio #{idVenta} no se pudo eliminar.", true, 2200);
                return;
            }

            var venta = rsp.data as TablaVentas;
            if (venta == null)
            {
                venta = JsonConvert.DeserializeObject<TablaVentas>(rsp.data);
            }
            if (venta == null)
            {
                AlertModerno.Error(this, "Error", $"El servicio #{idVenta} no se pudo eliminar.", true, 2200);
                return;
            }

            venta.eliminada = true;
            var rspCrud = await TablaVentasControler.CRUD(models.db, venta, 1);
            if (!rspCrud.estado)
            {
                AlertModerno.Error(this, "Error", $"El servicio #{idVenta} no se pudo eliminar.", true, 2200);
                return;
            }

            var relaciones = await R_VentaMesaControler.ListaRelacion(models.db, idVenta) ?? new List<R_VentaMesa>();
            foreach (var relacion in relaciones)
            {
                var mesa = await MesasControler.Consultar_id(models.db, relacion.idMesa);
                if (mesa != null)
                {
                    mesa.estadoMesa = 0;
                    await MesasControler.CRUD(models.db, mesa, 1);
                }

                await R_VentaMesaControler.CRUD(models.db, relacion, 2);
            }

            await IniciarPagina();
            AlertModerno.Success(this, "OK", $"Servicio #{idVenta} eliminado con \u00e9xito.", true, 1600);
        }

        private async Task EditarAliasCuenta(string parametros)
        {
            try
            {

                var args = new EventArgumentParser(parametros);

                int idCuenta = args.GetInt("ID");
                string alias = args.GetString("ALIAS")?.Trim();

                if (idCuenta <= 0)
                {
                    AlertModerno.Warning(this, "Atenci\u00f3n", "No se recibi\u00f3 un ID v\u00e1lido.", true, 2500);
                    return;
                }

                if (string.IsNullOrWhiteSpace(alias) || alias.Length < 2)
                {
                    AlertModerno.Warning(this, "Atenci\u00f3n", "El nombre debe tener m\u00ednimo 2 caracteres.", true, 2500);
                    return;
                }

                var venta = new TablaVentas();
                var resp = await TablaVentasControler.Consultar_Id(models.db, Convert.ToInt32(idCuenta));
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", $"no se encontro la venta {idCuenta}");
                    return;
                }

                venta = JsonConvert.DeserializeObject<TablaVentas>(resp.data);
                venta.aliasVenta = alias;
                var crud = await TablaVentasControler.CRUD(models.db, venta, 1);
                if (!crud.estado)
                {
                    AlertModerno.Error(this, "Error", $"no se modifico el alias");
                }



                models.cuentas = await CargarCuentas();
                await CargarDATA();

                AlertModerno.Success(this, "Ok", "Cuenta actualizada correctamente.", true, 2200);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 3000);
            }
        }

        private async Task SeleccionarCuenta(string parametros)
        {

            var args = new EventArgumentParser(parametros);

            int idCuenta = args.GetInt("ID");

            if (idCuenta > 0) 
            {
                //consultamos una relacion 
                var r = await R_VentaMesaControler.ListaRelacion(models.db,idCuenta);
                if(r.Count>0)
                {
                    int idmesa = r.FirstOrDefault().idMesa;
                    var mesa = models.MesasLista.Where(x => x.id == idmesa).FirstOrDefault();
                    if(mesa != null)
                    {
                        models.IdZonaActiva = mesa.idZona;
                        models.Mesas = models.MesasLista.Where(x => x.idZona == mesa.idZona).ToList();
                    }
                }
                models.IdCuenteClienteActiva = 0;
                models.IdCuentaActiva = idCuenta;
                models.venta = await V_TablaVentasControler.Consultar_Id(models.db, idCuenta);
                models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(models.db, 0);
                models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(models.db, idCuenta, 0);

                await CargarDATA();
            }


        }

        private async Task SeleccionarZona(string parametros)
        {
            var datos=new EventArgumentParser(parametros);
            int idZona = datos.GetInt("ID"); 

            if(idZona > 0)
            {
                models.IdZonaActiva = idZona;
                models.Mesas = models.MesasLista.Where(x => x.idZona == idZona).ToList();

                await CargarDATA();
            }
        }

        private async Task SeleccionarMesa(string parametros)
        {
            try
            {
                // ? Siempre trabajar con models actualizado (viene de Session)
                await DeserializarModels();

                var data = new EventArgumentParser(parametros);
                int idMesa = data.GetInt("ID");

                if (idMesa <= 0)
                {
                    AlertModerno.Warning(this, "Atenci\u00f3n", "No se recibi\u00f3 una mesa v\u00e1lida.", true, 2200);
                    return;
                }

                // ? Set activo
                models.IdMesaActiva = idMesa;

                // ? Buscar mesa sin riesgo de null
                var mesa = models.MesasLista?.FirstOrDefault(x => x.id == idMesa);
                lblMesaSeleccionada.InnerText = mesa != null ? mesa.nombreMesa : $"Mesa #{idMesa}";

                // Cargar data dependiente (tu l\u00f3gica)
                await CargarDATA();


                // Abrir modal (despu\u00e9s de cargar/persistir)
                ModalHelper.Open(this, modalAccionesMesa);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 3000);
            }
        }
        private async Task AccionMesa_CrearServicio()
        {
            int idVenta = await TablaVentas_f.NuevaVenta(models.db, models.Sede.porcentaje_propina);
            if (idVenta <= 0)
            {
                AlertModerno.Error(this, "Error", "No se cre\u00f3 el servicio.", true, 2000);
                return;
            }
            else
            {
                // procedemos a modificar el alias
                var resp_ = await TablaVentasControler.Consultar_Id(models.db, idVenta);
                if (resp_.estado)
                {
                    var venta = JsonConvert.DeserializeObject<TablaVentas>(resp_.data);
                    venta.aliasVenta = Convert.ToString(idVenta);
                    var crud = await TablaVentasControler.CRUD(models.db, venta, 1);
                }
            }


            // amarro venta con vendedor (uso session idvendedor si existe)
            var rvv = await R_VentaVendedor_f.Relacionar_Vendedor_Venta(models.db, idVenta, models.vendedor.id);
            if (!rvv)
            {
                AlertModerno.Error(this, "Error", $"Servicio #{idVenta} creado con \u00e9xito, pero no se amarr\u00f3 al vendedor.", true, 2000);
                return;
            }

            //amaramos la mesa con la cuenta

            var rv = await R_VentaMesaControler.Consultar_relacion(models.db, idVenta, models.IdMesaActiva);
            if (rv != null)
            {
                                AlertModerno.Warning(this, "Atenci\u00f3n", $"La mesa seleccionada ya est\u00e1 amarrada con la cuenta {idVenta}.", true, 2200);
                return;
            }

            rv = new R_VentaMesa { id = 0, idMesa = models.IdMesaActiva, idVenta = idVenta };
            var resp = await R_VentaMesaControler.CRUD(models.db, rv, 0);
            if (!resp.estado)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "No fue posible terminar el proceso.", true, 2200);
                return;
            }

            //cambiamos el estado de la mesa
            var mesa = await MesasControler.Consultar_id(models.db, models.IdMesaActiva);
            if (mesa == null)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "No se encontr\u00f3 la mesa.", true, 2200);
                return;
            }

            mesa.estadoMesa = 1;
            var respm = await MesasControler.CRUD(models.db, mesa, 1);
            if (!respm.estado)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", $"No se logr\u00f3 cambiar el estado de la mesa: {mesa.nombreMesa}.", true, 2200);
                return;
            }

            var mesas = await MesasControler.Lista(models.db);
            if (mesas.Count == 0)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "No se logr\u00f3 cargar la lista de las mesas.", true, 2200);
                return;
            }

            // Actualizar modelos y UI
            models.IdCuentaActiva = idVenta;
            models.cuentas = await CargarCuentas();
            models.venta = await V_TablaVentasControler.Consultar_Id(models.db, models.IdCuentaActiva);
            models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(models.db, models.IdCuentaActiva, models.IdCuenteClienteActiva);
            models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(models.db, false, models.IdCuentaActiva);
            models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(models.db, models.IdCuenteClienteActiva);

            models.MesasLista = new List<Mesas>();
            models.MesasLista = mesas;

            models.Mesas = new List<Mesas>();
            models.Mesas = mesas.Where(x => x.idZona == models.IdZonaActiva).ToList();



            await CargarDATA();

            AlertModerno.Success(this, "Listo", $"La mesa {mesa.nombreMesa} se relacion\u00f3 correctamente con la cuenta {idVenta}", true, 2000);
        }

        private async Task LiberarMesa()
        {
            try
            {
                if (models.IdMesaActiva <= 0)
                {
                    AlertModerno.Warning(this, "Atenci\u00f3n", "No se ha seleccionado una mesa v\u00e1lida.", true, 2200);
                    return;
                }

                var mesa = await MesasControler.Consultar_id(models.db, models.IdMesaActiva);
                if (mesa == null)
                {
                    AlertModerno.Warning(this, "Atenci\u00f3n", "No se encontr\u00f3 la mesa seleccionada.", true, 2200);
                    return;
                }

                mesa.estadoMesa = 0;
                var actualizarMesa = await MesasControler.CRUD(models.db, mesa, 1);
                if (!actualizarMesa.estado)
                {
                    AlertModerno.Error(this, "Error", $"No fue posible liberar la mesa {mesa.nombreMesa}.", true);
                    return;
                }

                var cuentasMesa = await V_CuentasControler.Lista_Mesa(models.db, mesa.nombreMesa) ?? new List<V_Cuentas>();
                if (cuentasMesa.Any())
                {
                    var relacion = await R_VentaMesaControler.Consultar_relacion(models.db, cuentasMesa.First().id, mesa.id);
                    if (relacion != null)
                    {
                        var eliminarRelacion = await R_VentaMesaControler.CRUD(models.db, relacion, 2);
                        if (!eliminarRelacion.estado)
                        {
                            AlertModerno.Error(this, "Error", $"La mesa {mesa.nombreMesa} cambi\u00f3 de estado, pero no se elimin\u00f3 la relaci\u00f3n con la cuenta.", true);
                            return;
                        }
                    }
                }

                var mesas = await MesasControler.Lista(models.db) ?? new List<Mesas>();
                models.MesasLista = mesas;
                models.Mesas = mesas.Where(x => x.idZona == models.IdZonaActiva).ToList();
                await CargarDATA();

                AlertModerno.Success(this, "OK", $"Mesa {mesa.nombreMesa} liberada correctamente.", true, 1500);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }
        private async Task AccionMesa_AmarrarMesa()
        {
            ModalHelper.Open(this,mdlCuentas);
        }

        private async Task AmarrarMesaCuenta(string parametros)
        {
            var data =new EventArgumentParser(parametros);
            int idCuentaAmarrar=data.GetInt("id");

            if (idCuentaAmarrar <= 0)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "No se recibi\u00f3 una cuenta v\u00e1lida.", true, 2200);
                return;
            }

            var rv = await R_VentaMesaControler.Consultar_relacion(models.db,idCuentaAmarrar,models.IdMesaActiva);
            if (rv != null)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", $"La mesa seleccionada ya est\u00e1 amarrada con la cuenta {idCuentaAmarrar}.", true, 2200);
                return;
            }

            rv=new R_VentaMesa { id=0, idMesa=models.IdMesaActiva, idVenta=idCuentaAmarrar };
            var resp = await R_VentaMesaControler.CRUD(models.db,rv,0);
            if (!resp.estado)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "No fue posible terminar el proceso.", true, 2200);
                return;
            }

            //cambiamos el estado de la mesa
            var mesa = await MesasControler.Consultar_id(models.db,models.IdMesaActiva);
            if (mesa == null)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "No se encontr\u00f3 la mesa.", true, 2200);
                return;
            }

            mesa.estadoMesa = 1;
            var respm = await MesasControler.CRUD(models.db,mesa,1);
            if (!respm.estado)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", $"No se logr\u00f3 cambiar el estado de la mesa: {mesa.nombreMesa}.", true, 2200);
                return;
            }

            var mesas = await MesasControler.Lista(models.db);
            if (mesas.Count == 0)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "No se logr\u00f3 cargar la lista de las mesas.", true, 2200);
                return;
            }

            models.MesasLista = new List<Mesas>();
            models.MesasLista = mesas;

            models.Mesas = new List<Mesas>();
            models.Mesas = mesas.Where(x => x.idZona == models.IdZonaActiva).ToList();

            models.cuentas = await CargarCuentas();
            models.IdCuentaActiva=idCuentaAmarrar;

            await CargarDATA();

            AlertModerno.Success(this,ok,"Mesa amarrada correctamente.");

        }

        private async Task BuscarCodigoProducto(string eventArgument)
        {
            try
            {
                string texto = (eventArgument ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(texto))
                {
                    ScriptManager.RegisterStartupScript(
                        this,
                        GetType(),
                        "CajaFocusBusquedaVacia",
                        "if (window.CajaViewport && typeof window.CajaViewport.activarFocusBuscadorSiAplica === 'function') { window.CajaViewport.activarFocusBuscadorSiAplica(); }",
                        true
                    );
                    return;
                }

                var producto = (models.productosLista ?? models.productos)
                    ?.FirstOrDefault(x => string.Equals(x.codigoProducto?.Trim(), texto, StringComparison.OrdinalIgnoreCase));

                if (producto == null)
                {
                    AlertModerno.Warning(this, "Atención", "Producto no encontrado", true, 2000);

                    ScriptManager.RegisterStartupScript(
                        this,
                        GetType(),
                        "CajaFocusProductoNoEncontrado",
                        "if (window.CajaViewport && typeof window.CajaViewport.activarFocusBuscadorSiAplica === 'function') { window.CajaViewport.activarFocusBuscadorSiAplica(); }",
                        true
                    );
                    return;
                }

                var resp = await DetalleVenta_f.AgregarProducto(models.db, producto.idPresentacion, 1, models.IdCuentaActiva);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No fue posible agregar el producto.", true);
                    return;
                }

                if (models.IdCuenteClienteActiva > 0 && resp.data != null)
                {
                    var relacion = new R_CuentaCliente_DetalleVenta
                    {
                        id = 0,
                        fecha = DateTime.Now,
                        idCuentaCliente = models.IdCuenteClienteActiva,
                        idDetalleVenta = Convert.ToInt32(resp.data),
                        eliminada = false
                    };
                    await R_CuentaCliente_DetalleVentaControler.CRUD(models.db, relacion, 0);
                }

                AlertModerno.Success(this, "OK", resp.mensaje ?? "Producto agregado correctamente.", true, 800);
                models.IdCategoriaActiva = producto.idCategoria;
                await RecargarVentaActiva();

                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "LimpiarBuscadorCaja",
                    @"
    if (window.CajaBuscador) { CajaBuscador.clear(false); }
    if (window.CajaViewport) {
        if (typeof window.CajaViewport.restaurarEstadoScroll === 'function') {
            window.CajaViewport.restaurarEstadoScroll();
        }
        if (typeof window.CajaViewport.activarFocusBuscadorSiAplica === 'function') {
            window.CajaViewport.activarFocusBuscadorSiAplica();
        }
    }
    ",
                    true
                );
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task AgregarProducto(string parametros)
        {
            try
            {
                var data = new EventArgumentParser(parametros);
                int idPresentacion = data.GetInt("ID");
                decimal cantidad = Convert.ToDecimal((data.GetString("CANTIDAD") ?? "1").Replace(".", ","));

                if (idPresentacion <= 0)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No se recibiÃƒÂ³ un producto vÃƒÂ¡lido.", true, 2200);
                    return;
                }

                if (cantidad <= 0)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "La cantidad debe ser mayor a cero.", true, 2200);
                    return;
                }

                var resp = await DetalleVenta_f.AgregarProducto(models.db, idPresentacion, cantidad, models.IdCuentaActiva);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No fue posible agregar el producto.", true);
                    return;
                }

                if (models.IdCuenteClienteActiva > 0 && resp.data != null)
                {
                    var relacion = new R_CuentaCliente_DetalleVenta
                    {
                        id = 0,
                        fecha = DateTime.Now,
                        idCuentaCliente = models.IdCuenteClienteActiva,
                        idDetalleVenta = Convert.ToInt32(resp.data),
                        eliminada = false
                    };
                    await R_CuentaCliente_DetalleVentaControler.CRUD(models.db, relacion, 0);
                }

                AlertModerno.Success(this, "OK", resp.mensaje ?? "Producto agregado correctamente.", true, 900);
                await RecargarVentaActiva();
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task SeleccionarCuentaCliente(string parametros)
        {
            var data = new EventArgumentParser(parametros);
            int idCuentaCliente = data.GetInt("ID");

            models.IdCuenteClienteActiva = idCuentaCliente;
            await RecargarVentaActiva();
        }

        private async Task CrearCuentaCliente(string parametros)
        {
            try
            {
                string nombreCuenta = (parametros ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(nombreCuenta) || nombreCuenta.Length < 2)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "El nombre de la cuenta debe tener mÃƒÂ­nimo 2 caracteres.", true, 2200);
                    return;
                }

                int nuevaCuentaId = await CuentaCliente_f.Crear(models.db, models.IdCuentaActiva, nombreCuenta, Convert.ToInt32(models.Sede.porcentaje_propina));
                if (nuevaCuentaId <= 0)
                {
                    AlertModerno.Error(this, "Error", "No fue posible crear la cuenta.", true, 2200);
                    return;
                }

                models.IdCuenteClienteActiva = nuevaCuentaId;
                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", "Cuenta creada correctamente.", true, 1600);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task ActualizarCantidadDetalle(string parametros)
        {
            try
            {
                var data = new EventArgumentParser(parametros);
                int idDetalle = data.GetInt("ID");
                int cantidad = data.GetInt("CANTIDAD");

                if (idDetalle <= 0 || cantidad <= 0)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No se recibiÃƒÂ³ una cantidad vÃƒÂ¡lida.", true, 1800);
                    return;
                }

                var resp = await DetalleVenta_f.ActualizarCantidadDetalle(models.db, idDetalle, cantidad);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo actualizar la cantidad.", true, 1800);
                    return;
                }

                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", resp.mensaje ?? "Cantidad actualizada correctamente.", true, 800);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task EliminarDetalleCaja(string parametros)
        {
            try
            {
                var data = new EventArgumentParser(parametros);
                int idDetalle = data.GetInt("ID");
                string nota = data.GetString("NOTA") ?? string.Empty;

                if (idDetalle <= 0)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No se recibiÃƒÂ³ un detalle vÃƒÂ¡lido.", true, 1800);
                    return;
                }

                var resp = await DetalleVenta_f.Eliminar(models.db, idDetalle, nota);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo eliminar el producto.", true, 1800);
                    return;
                }

                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", resp.mensaje ?? "Producto eliminado correctamente.", true, 800);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task GuardarNotaDetalle(string parametros)
        {
            try
            {
                var data = new EventArgumentParser(parametros);
                int idDetalle = data.GetInt("ID");
                string nota = data.GetString("NOTA") ?? string.Empty;

                if (idDetalle <= 0)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No se recibiÃƒÂ³ un detalle vÃƒÂ¡lido.", true, 1800);
                    return;
                }

                var resp = await DetalleVenta_f.NotasDetalle(models.db, idDetalle, nota);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo actualizar la nota.", true, 1800);
                    return;
                }

                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", resp.mensaje ?? "Nota actualizada correctamente.", true, 900);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task DividirDetalle(string parametros)
        {
            try
            {
                var data = new EventArgumentParser(parametros);
                int idDetalle = data.GetInt("ID");
                int cantidadActual = data.GetInt("ACTUAL");
                int cantidadDividir = data.GetInt("DIVIDIR");

                if (idDetalle <= 0 || cantidadActual <= 1 || cantidadDividir <= 0 || cantidadDividir >= cantidadActual)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "La cantidad a dividir no es vÃƒÂ¡lida.", true, 1800);
                    return;
                }

                var resp = await DetalleVenta_f.Dividir(models.db, idDetalle, cantidadActual, cantidadDividir, models.IdCuentaActiva);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo dividir el detalle.", true, 1800);
                    return;
                }

                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", resp.mensaje ?? "Detalle dividido correctamente.", true, 900);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task AnclarDetalleCuenta(string parametros)
        {
            try
            {
                var data = new EventArgumentParser(parametros);
                int idDetalle = data.GetInt("ID");
                int idCuentaCliente = data.GetInt("CUENTA");

                if (idDetalle <= 0 || idCuentaCliente <= 0)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "Seleccione una cuenta vÃƒÂ¡lida para anclar el detalle.", true, 1800);
                    return;
                }

                var resp = await R_CuentaCliente_DetalleVenta_f.Insert(models.db, idCuentaCliente, idDetalle);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo anclar el detalle a la cuenta.", true, 1800);
                    return;
                }

                models.IdCuenteClienteActiva = idCuentaCliente;
                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", resp.mensaje ?? "Detalle anclado correctamente.", true, 900);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task EditarValorDetalle(string parametros)
        {
            try
            {
                var data = new EventArgumentParser(parametros);
                int idDetalle = data.GetInt("ID");
                decimal valor = Convert.ToDecimal((data.GetString("VALOR") ?? "0").Replace(".", ","));

                if (idDetalle <= 0 || valor <= 0)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "Ingrese un valor vÃƒÂ¡lido.", true, 1800);
                    return;
                }

                var detalle = await DetalleVentaControler.ConsultarId(models.db, idDetalle);
                if (detalle == null)
                {
                    AlertModerno.Error(this, "Error", "No se encontrÃƒÂ³ el detalle a modificar.", true, 1800);
                    return;
                }

                detalle.precioVenta = valor;
                var resp = await DetalleVentaControler.CRUD(models.db, detalle, 1);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo actualizar el valor.", true, 1800);
                    return;
                }

                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", resp.mensaje ?? "Valor actualizado correctamente.", true, 900);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private async Task EditarNombreDetalle(string parametros)
        {
            try
            {
                var data = new EventArgumentParser(parametros);
                int idDetalle = data.GetInt("ID");
                string nombre = (data.GetString("NOMBRE") ?? string.Empty).Trim();

                if (idDetalle <= 0 || string.IsNullOrWhiteSpace(nombre))
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "Ingrese una descripciÃƒÂ³n vÃƒÂ¡lida.", true, 1800);
                    return;
                }

                var detalle = await DetalleVentaControler.ConsultarId(models.db, idDetalle);
                if (detalle == null)
                {
                    AlertModerno.Error(this, "Error", "No se encontrÃƒÂ³ el detalle a modificar.", true, 1800);
                    return;
                }

                detalle.nombreProducto = nombre;
                var resp = await DetalleVentaControler.CRUD(models.db, detalle, 1);
                if (!resp.estado)
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo actualizar el producto.", true, 1800);
                    return;
                }

                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", resp.mensaje ?? "Producto actualizado correctamente.", true, 900);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }

        private class EditarPropinaDto
        {
            public decimal porcentaje { get; set; }
            public int propina { get; set; }
            public int idventa { get; set; }
            public int idcuenta { get; set; }
        }

        private async Task EditarPropina(string parametros)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parametros))
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No se recibiÃƒÂ³ informaciÃƒÂ³n de propina.", true, 1800);
                    return;
                }

                var dto = JsonConvert.DeserializeObject<EditarPropinaDto>(parametros);
                if (dto == null)
                {
                    AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No se pudo interpretar la informaciÃƒÂ³n de propina.", true, 1800);
                    return;
                }

                decimal porPropina = dto.porcentaje / 100m;
                if (dto.idcuenta > 0)
                {
                    var cuenta = await CuentaClienteControler.CuentaCliente(models.db, dto.idcuenta);
                    if (cuenta == null)
                    {
                        AlertModerno.Error(this, "Error", "No se encontrÃƒÂ³ la cuenta cliente para actualizar la propina.", true, 1800);
                        return;
                    }

                    cuenta.por_propina = porPropina;
                    cuenta.propina = dto.propina;
                    var respCuenta = await CuentaClienteControler.CRUD(models.db, cuenta, 1);
                    if (!respCuenta.estado)
                    {
                        AlertModerno.Error(this, "Error", respCuenta.mensaje ?? "No se pudo actualizar la propina de la cuenta.", true, 1800);
                        return;
                    }
                }
                else
                {
                    var respuesta = await TablaVentasControler.Consultar_Id(models.db, dto.idventa);
                    if (!respuesta.estado)
                    {
                        AlertModerno.Error(this, "Error", "No se encontrÃƒÂ³ la venta para actualizar la propina.", true, 1800);
                        return;
                    }

                    var venta = respuesta.data as TablaVentas;
                    if (venta == null)
                    {
                        venta = JsonConvert.DeserializeObject<TablaVentas>(respuesta.data);
                    }
                    if (venta == null)
                    {
                        AlertModerno.Error(this, "Error", "No se encontrÃƒÂ³ la venta para actualizar la propina.", true, 1800);
                        return;
                    }

                    venta.porpropina = porPropina;
                    venta.propina = dto.propina;
                    var respVenta = await TablaVentasControler.CRUD(models.db, venta, 1);
                    if (!respVenta.estado)
                    {
                        AlertModerno.Error(this, "Error", respVenta.mensaje ?? "No se pudo actualizar la propina del servicio.", true, 1800);
                        return;
                    }
                }

                models.IdCuenteClienteActiva = dto.idcuenta;
                await RecargarVentaActiva();
                AlertModerno.Success(this, "OK", "Propina actualizada correctamente.", true, 1000);
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
        }
        private async Task Comandar()
        {
            if (models.detalleCaja == null || !models.detalleCaja.Any())
            {
                AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No hay productos cargados para comandar.", true, 1800);
                return;
            }

            if (models.detalleCaja.Where(x => x.itemComandado == 0).ToList().Count == 0)
            {
                AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No hay items pendientes por comandar.", true, 1800);
                return;
            }

            var comanda = new ImprecionComandaAdd
            {
                id = 0,
                idVenta = models.IdCuentaActiva,
                idMesa = Convert.ToString(models.IdMesaActiva),
                idMesero = Convert.ToString(models.vendedor.id),
                estado = 1
            };

            var resp = await ImprecionComandaAddControler.CRUD(models.db, comanda, 0);
            if (resp.estado)
            {
                AlertModerno.Success(this, "OK", "Comanda enviada correctamente.", true, 1500);
            }
            else
            {
                AlertModerno.Error(this, "Error", "Comanda no enviada correctamente.", true, 1800);
            }
        }

        private async Task SolicitarCuenta()
        {
            if (models.IdCuentaActiva <= 0)
            {
                AlertModerno.Warning(this, "AtenciÃƒÂ³n", "No hay un servicio activo para imprimir la cuenta.", true, 1800);
                return;
            }

            var cuenta = new ImprimirCuenta
            {
                id = 0,
                idVenta = models.IdCuentaActiva
            };

            var resp = await ImprimirCuentaControler.CRUD(models.db, cuenta, 0);
            if (resp.estado)
            {
                AlertModerno.Success(this, "OK", "Cuenta enviada correctamente.", true, 1500);
            }
            else
            {
                AlertModerno.Error(this, "Error", "Cuenta no enviada correctamente.", true, 1800);
            }
        }

        private async Task Cobrar()
        {
            SessionContextHelper.ApplyOperationalContext(Session, models);
            Response.Redirect("~/Cobrar.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
            await Task.CompletedTask;
        }

        private async Task CerrarCaja()
        {
            SessionContextHelper.ApplyOperationalContext(Session, models);
            Response.Redirect("~/CerrarCaja.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
            await Task.CompletedTask;
        }

        private async Task CerrarSesion()
        {
            SessionContextHelper.ApplyOperationalContext(Session, models);
            Response.Redirect("~/Salir.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
            await Task.CompletedTask;
        }

        private async Task Ventas()
        {
            SessionContextHelper.ApplyOperationalContext(Session, models);
            Response.Redirect("~/HVentas.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
            await Task.CompletedTask;
        }

        private async Task Domicilio(string eventArgument)
        {
            if (string.IsNullOrWhiteSpace(eventArgument))
            {
                AlertModerno.Error(this, "Error", "No hay una mesa seleccionada.", true);
                return;
            }

            var parts = eventArgument.Split('|');
            if (!int.TryParse(parts[0], out int idMesa) || idMesa <= 0)
            {
                AlertModerno.Error(this, "Error", "Mesa invÃƒÂ¡lida.", true);
                return;
            }

            int idServicio = models.IdCuentaActiva;
            if (parts.Length > 1)
            {
                int.TryParse(parts[1], out idServicio);
            }

            if (idServicio <= 0)
            {
                AlertModerno.Error(this, "Error", "No hay un servicio activo.", true);
                return;
            }

            var mesa = await MesasControler.Consultar_id(models.db, idMesa);
            if (mesa == null)
            {
                AlertModerno.Error(this, "Error", "No se encontrÃƒÂ³ la mesa.", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(mesa.nombreMesa) || !mesa.nombreMesa.ToUpperInvariant().Contains("DOMICILIO"))
            {
                AlertModerno.Error(this, "Error", "La mesa seleccionada no es un domicilio.", true);
                return;
            }

            models.IdMesaActiva = idMesa;
            models.IdCuentaActiva = idServicio;
            models.IdCuenteClienteActiva = 0;
            models.venta = await V_TablaVentasControler.Consultar_Id(models.db, idServicio);
            models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(models.db, idServicio, 0);
            models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(models.db, false, idServicio);
            models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(models.db, 0);
            models.clienteDomicilios = await ClienteDomicilioControler.Lista(models.db);
            models.AbrirModalDomicilio = true;

            await CargarDATA();
        }

        private async Task CrearActualizarClienteDomicilio(string eventArgument)
        {
            if (string.IsNullOrWhiteSpace(eventArgument))
            {
                return;
            }

            var parts = eventArgument.Split('|');
            if (parts.Length < 4)
            {
                return;
            }

            var idStr = parts[0];
            var tel = parts[1];
            var nom = parts[2];
            var dir = parts[3];

            Guid id;
            var esNuevo = string.IsNullOrWhiteSpace(idStr);
            if (esNuevo)
            {
                id = Guid.NewGuid();
            }
            else
            {
                id = new Guid(idStr);
            }

            var entidad = new ClienteDomicilio
            {
                id = id,
                celularCliente = tel,
                nombreCliente = nom,
                direccionCliente = dir
            };

            var funcion = esNuevo ? 0 : 1;
            var resp = await ClienteDomicilioControler.CRUD(models.db, entidad, funcion);
            if (!resp.estado)
            {
                AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo guardar el cliente.", true);
                return;
            }

            models.clienteDomicilios = await ClienteDomicilioControler.Lista(models.db);
            models.AbrirModalDomicilio = true;

            AlertModerno.Success(this, "OK", resp.mensaje ?? "Cliente guardado correctamente.", true, 1200);
            await CargarDATA();
        }

        private async Task SeleccionarClienteDomicilio(string eventArgument)
        {
            if (string.IsNullOrWhiteSpace(eventArgument))
            {
                return;
            }

            var parts = eventArgument.Split('|');
            if (parts.Length < 4)
            {
                return;
            }

            var idStr = parts[0];
            if (!Guid.TryParse(idStr, out Guid idCliente))
            {
                AlertModerno.Error(this, "Error", "ID de cliente invÃƒÂ¡lido.", true);
                return;
            }

            var idVenta = models.IdCuentaActiva;
            var consultarRelacion = await ClienteDomicilioControler.ConsultarRelacion(models.db, idVenta);
            var funcion = consultarRelacion == null ? 0 : 1;

            if (consultarRelacion == null)
            {
                consultarRelacion = new R_VentaClienteDomicilio
                {
                    id = 0,
                    idVenta = idVenta,
                    idClienteDomicilio = idCliente
                };
            }
            else
            {
                consultarRelacion.idClienteDomicilio = idCliente;
            }

            var resp = await ClienteDomicilioControler.RelacionarConVenta(models.db, consultarRelacion, funcion);
            if (!resp.estado)
            {
                AlertModerno.Error(this, "Error", resp.mensaje ?? "No se pudo relacionar el cliente con la venta.", true);
                return;
            }

            models.clienteDomicilios = await ClienteDomicilioControler.Lista(models.db);
            models.cuentas = await CargarCuentas();
            models.AbrirModalDomicilio = false;

            AlertModerno.Success(this, "OK", resp.mensaje ?? "Cliente relacionado con la venta.", true, 1200);
            await CargarDATA();

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "CerrarModalDomicilioCaja",
                "(function(){var modalEl=document.getElementById('modalDomicilio');if(modalEl&&window.bootstrap){bootstrap.Modal.getOrCreateInstance(modalEl).hide();}})();",
                true
            );
        }

        private async Task RecargarVentaActiva()
        {
            models.cuentas = await CargarCuentas();
            models.venta = await V_TablaVentasControler.Consultar_Id(models.db, models.IdCuentaActiva);
            models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(models.db, models.IdCuentaActiva, models.IdCuenteClienteActiva);
            models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(models.db, false, models.IdCuentaActiva);
            models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(models.db, models.IdCuenteClienteActiva);
            await CargarDATA();
        }

        private async Task SeleccionarCategoria(string parametros)
        {
            var data = new EventArgumentParser(parametros);
            int idCategoria = data.GetInt("id");

            if(idCategoria == 0)
            {
                AlertModerno.Warning(this, "Atenci\u00f3n", "Id de categor\u00eda no recibido.", true, 2200);
                return;
            }

            models.IdCategoriaActiva = idCategoria;
            models.productos = models.productosLista.Where(x => x.idCategoria == idCategoria).ToList();

            await CargarDATA();
        }
    }
}





























