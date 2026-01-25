using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication.ViewModels
{
    public class MenuViewModels
    {
        public bool estadopropina { get; set; }
        public int porpropina { get; set; }
        public int cajero { get; set; }
        public int IdMesero { get; set; }
        public string NombreMesero { get; set; }
        public int IdCuentaActiva { get; set; }
        public int IdZonaActiva { get; set; }
        public int IdMesaActiva { get; set; }
        public int IdCategoriaActiva { get; set; }
        public int IdCuenteClienteActiva { get; set; }
        public List<V_CuentasVenta> cuentas { get; set; }
        public List<Zonas> zonas { get; set; }
        public List<Mesas> Mesas { get; set; }
        public List<V_Categoria> categorias { get; set; }
        public List<v_productoVenta> productos { get; set; }
        public V_TablaVentas venta { get; set; }
        public List<V_DetalleCaja> detalleCaja { get; set; }
        public List<V_CuentaCliente> v_CuentaClientes { get; set; }
        public List<V_CatagoriaAdicion> adiciones { get; set; }
        public V_CuentaCliente ventaCuenta { get; set; }
        public List<ClienteDomicilio> clienteDomicilios { get; set; }
        public bool AbrirModalDomicilio { get; set; }
        public List<Vendedor> ListaVendedor { get; set; }
        public List<CargoDescuentoVentas> cargoDescuentoVentas { get; set; }
        public List<Clientes> clientes { get; set; }
    }
}