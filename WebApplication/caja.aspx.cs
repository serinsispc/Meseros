using DAL.Controler;
using DAL.Funciones;
using DAL.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.UI;
using WebApplication.Class;
using WebApplication.Helpers;
using WebApplication.ViewModels;

namespace WebApplication
{
    public partial class caja : System.Web.UI.Page
    {
        private const string ok = "Ok";
        private const string SessionModelsJson = "ModelsJson";
        protected MenuViewModels models = new MenuViewModels();
        protected bool PuedeEliminarServicioActivo()
        {
            return models?.detalleCaja == null || !models.detalleCaja.Any();
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

            btnbuscar.Focus();
        }
        private async Task Cargar_RP()
        {
            rpCuentas.DataSource = models.cuentas;
            rpCuentasModal.DataSource = models.cuentas;
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
            return cuentas;
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
                models.IdCuentaActiva = idCuenta;
                models.venta = await V_TablaVentasControler.Consultar_Id(models.db, idCuenta);
                models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(models.db, idCuenta);
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
                    btnbuscar.Focus();
                    return;
                }

                var producto = (models.productosLista ?? models.productos)
                    ?.FirstOrDefault(x => string.Equals(x.codigoProducto?.Trim(), texto, StringComparison.OrdinalIgnoreCase));

                if (producto == null)
                {
                    AlertModerno.Warning(this, "Atenci\u00f3n", "Producto no encontrado", true, 2000);
                    btnbuscar.Focus();
                    return;
                }

                var resp = await DetalleVenta_f.AgregarProducto(models.db, producto.idPresentacion, 1, models.IdCuentaActiva);
                if (resp.estado)
                {
                    AlertModerno.Success(this, "OK", resp.mensaje ?? "Producto agregado correctamente.", true, 800);
                    models.IdCategoriaActiva = producto.idCategoria;
                    models.venta = await V_TablaVentasControler.Consultar_Id(models.db, models.IdCuentaActiva);
                    models.detalleCaja = await V_DetalleCajaControler.Lista_IdVenta(models.db, models.IdCuentaActiva, models.IdCuenteClienteActiva);
                    models.v_CuentaClientes = await V_CuentaClienteCotroler.Lista(models.db, false, models.IdCuentaActiva);
                    models.ventaCuenta = await V_CuentaClienteCotroler.Consultar(models.db, models.IdCuenteClienteActiva);
                    await CargarDATA();
                }
                else
                {
                    AlertModerno.Error(this, "Error", resp.mensaje ?? "No fue posible agregar el producto.", true);
                }

                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "LimpiarBuscadorCaja",
                    "if (window.CajaBuscador) { CajaBuscador.clear(false); } document.getElementById('btnbuscar')?.focus();",
                    true
                );
            }
            catch (Exception ex)
            {
                AlertModerno.Error(this, "Error", ex.Message, true, 2500);
            }
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


