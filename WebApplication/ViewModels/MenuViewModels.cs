using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication.ViewModels
{
    public class MenuViewModels
    {
        public string db=string.Empty;
        public string TokenEmpresa=string.Empty;
        public int IdCuentaActiva { get; set; } = 0;
        public int IdZonaActiva { get; set; } = 0;
        public int IdMesaActiva { get; set; } = 0;
        public int IdCategoriaActiva { get; set; } = 0;
        public int IdCuenteClienteActiva { get; set; } = 0;
        public BaseCaja BaseCaja { get; set; } = new BaseCaja();
        public Sede Sede { get; set; } = new Sede();
        public List<V_CuentasVenta> cuentas { get; set; } = new List<V_CuentasVenta>();
        public List<Zonas> zonas { get; set; }=new List<Zonas>();
        public List<Mesas> Mesas { get; set; } = new List<Mesas>();
        public List<Mesas> MesasLista { get; set; } = new List<Mesas>();
        public List<V_Categoria> categorias { get; set; } = new List<V_Categoria>();
        public List<v_productoVenta> productos { get; set; } = new List<v_productoVenta>();
        public List<v_productoVenta> productosLista { get; set; } = new List<v_productoVenta>();
        public V_TablaVentas venta { get; set; } = new V_TablaVentas();
        public List<V_DetalleCaja> detalleCaja { get; set; } = new List<V_DetalleCaja>();
        public List<V_CuentaCliente> v_CuentaClientes { get; set; } = new List<V_CuentaCliente>();
        public List<V_CatagoriaAdicion> adiciones { get; set; } = new List<V_CatagoriaAdicion>();
        public V_CuentaCliente ventaCuenta { get; set; } = new V_CuentaCliente();
        public List<ClienteDomicilio> clienteDomicilios { get; set; } = new List<ClienteDomicilio>();
        public bool AbrirModalDomicilio { get; set; } = false;
        public List<Vendedor> ListaVendedor { get; set; } = new List<Vendedor>();
        public Vendedor vendedor { get; set; }= new Vendedor();
        public List<CargoDescuentoVentas> cargoDescuentoVentas { get; set; } = new List<CargoDescuentoVentas>();
        public List<Clientes> clientes { get; set; } = new List<Clientes>();
    }
}