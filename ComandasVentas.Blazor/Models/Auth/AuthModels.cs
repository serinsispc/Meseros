namespace ComandasVentas.Blazor.Models.Auth;

public sealed class LoginBootstrapResult
{
    public required string Database { get; init; }
    public required SedeInfo? Sede { get; init; }
    public required DbConexionInfo? DbConexion { get; init; }
    public required IReadOnlyList<VendedorInfo> Vendedores { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsValid => string.IsNullOrWhiteSpace(ErrorMessage) && !string.IsNullOrWhiteSpace(Database);
}

public sealed class LoginResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public required string Database { get; init; }
    public SedeInfo? Sede { get; init; }
    public DbConexionInfo? DbConexion { get; init; }
    public VendedorInfo? Vendedor { get; init; }
    public BaseCajaInfo? BaseCaja { get; init; }
    public RVendorUsuarioInfo? UsuarioCaja { get; init; }
    public bool RequireCajaApertura { get; init; }
}

public sealed class SedeInfo
{
    public int Id { get; init; }
    public string? NombreSede { get; init; }
    public Guid GuidSede { get; init; }
    public int PorcentajePropina { get; init; }
}

public sealed class VendedorInfo
{
    public int Id { get; init; }
    public string NombreVendedor { get; init; } = string.Empty;
    public string TelefonoVendedor { get; init; } = string.Empty;
    public string ClaveVendedor { get; init; } = string.Empty;
    public int CajaMovil { get; init; }
}

public sealed class DbConexionInfo
{
    public string? NombreCaja { get; init; }
    public bool MeserosCompartidos { get; init; }
    public bool ComandasCaja { get; init; }
    public bool ServidorImpresora { get; init; }
}

public sealed class BaseCajaInfo
{
    public int Id { get; init; }
    public DateTime FechaApertura { get; init; }
    public int IdUsuarioApertura { get; init; }
    public decimal ValorBase { get; init; }
    public DateTime? FechaCierre { get; init; }
    public int? IdUsuarioCierre { get; init; }
    public string EstadoBase { get; init; } = string.Empty;
    public int IdSedeBase { get; init; }
}

public sealed class RVendorUsuarioInfo
{
    public int Id { get; init; }
    public int IdVendedor { get; init; }
    public int IdUsuario { get; init; }
}
