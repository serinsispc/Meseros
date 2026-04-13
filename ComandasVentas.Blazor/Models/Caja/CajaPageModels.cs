namespace ComandasVentas.Blazor.Models.Caja;

public sealed class CajaPageData
{
    public List<CuentaCajaData> Cuentas { get; init; } = [];
    public List<CuentaCajaData> CuentasMesasVista { get; init; } = [];
    public List<VentaMesaRelacionData> RelacionesVentaMesa { get; init; } = [];
    public List<ZonaCajaData> Zonas { get; init; } = [];
    public List<MesaCajaData> Mesas { get; init; } = [];
    public List<CategoriaCajaData> Categorias { get; init; } = [];
    public List<ProductoCajaData> Productos { get; init; } = [];
    public VentaCajaData? VentaActiva { get; init; }
    public List<CuentaClienteCajaData> CuentasCliente { get; init; } = [];
    public List<DetalleCajaData> Detalles { get; init; } = [];
}

public sealed class CuentaCajaData
{
    public int Id { get; init; }
    public string AliasVenta { get; init; } = string.Empty;
    public bool Eliminada { get; init; }
    public int IdVendedor { get; init; }
    public string NombreVendedor { get; init; } = string.Empty;
    public string NombreMesa { get; init; } = string.Empty;
    public string NombreClienteDomicilio { get; init; } = string.Empty;
}

public sealed class ZonaCajaData
{
    public int Id { get; init; }
    public string NombreZona { get; init; } = string.Empty;
}

public sealed class MesaCajaData
{
    public int Id { get; init; }
    public string NombreMesa { get; init; } = string.Empty;
    public int EstadoMesa { get; init; }
    public int IdZona { get; init; }
}

public sealed class CategoriaCajaData
{
    public int Id { get; init; }
    public string NombreCategoria { get; init; } = string.Empty;
    public int Visible { get; init; }
}

public sealed class ProductoCajaData
{
    public int Id { get; init; }
    public int IdCategoria { get; init; }
    public int ImpuestoId { get; init; }
    public int IdPresentacion { get; init; }
    public string CodigoProducto { get; init; } = string.Empty;
    public string NombreProducto { get; init; } = string.Empty;
    public decimal CostoMasImpuesto { get; init; }
    public decimal PorcentajeImpuesto { get; init; }
    public decimal PrecioVenta { get; init; }
    public int Visible { get; init; }
    public int EstadoProducto { get; init; }
    public int EstadoPresentacion { get; init; }
}

public sealed class VentaCajaData
{
    public int Id { get; init; }
    public string AliasVenta { get; init; } = string.Empty;
    public decimal SubtotalVenta { get; init; }
    public decimal IvaVenta { get; init; }
    public decimal TotalVenta { get; init; }
    public decimal Propina { get; init; }
    public decimal PorcentajePropina { get; init; }
    public decimal TotalAPagar { get; init; }
}

public sealed class CuentaClienteCajaData
{
    public int Id { get; init; }
    public int IdVenta { get; init; }
    public string NombreCuenta { get; init; } = string.Empty;
    public bool Eliminada { get; init; }
    public decimal SubtotalVenta { get; init; }
    public decimal IvaVenta { get; init; }
    public decimal TotalVenta { get; init; }
    public decimal PorcentajePropina { get; init; }
    public decimal Propina { get; init; }
    public decimal TotalAPagar { get; init; }
}

public sealed class DetalleCajaData
{
    public int Id { get; init; }
    public int IdVenta { get; init; }
    public int IdCuentaCliente { get; init; }
    public int IdCategoria { get; init; }
    public int EstadoDetalle { get; init; }
    public decimal Unidad { get; init; }
    public string NombreProducto { get; init; } = string.Empty;
    public string NombreCuenta { get; init; } = string.Empty;
    public string Adiciones { get; init; } = string.Empty;
    public int ItemComandado { get; init; }
    public decimal PrecioVenta { get; init; }
    public decimal TotalDetalle { get; init; }
}

public sealed class VentaMesaRelacionData
{
    public int Id { get; init; }
    public int IdVenta { get; init; }
    public int IdMesa { get; init; }
}
