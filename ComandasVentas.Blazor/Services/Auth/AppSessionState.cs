using ComandasVentas.Blazor.Models.Auth;

namespace ComandasVentas.Blazor.Services.Auth;

public sealed class AppSessionState
{
    public string Database { get; private set; } = string.Empty;
    public SedeInfo? Sede { get; private set; }
    public DbConexionInfo? DbConexion { get; private set; }
    public VendedorInfo? Vendedor { get; private set; }
    public BaseCajaInfo? BaseCaja { get; private set; }
    public RVendorUsuarioInfo? UsuarioCaja { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Database) && Vendedor is not null;

    public void SetLogin(LoginResult result)
    {
        Database = result.Database;
        Sede = result.Sede;
        DbConexion = result.DbConexion;
        Vendedor = result.Vendedor;
        BaseCaja = result.BaseCaja;
        UsuarioCaja = result.UsuarioCaja;
    }

    public void Restore(SessionSnapshot snapshot)
    {
        Database = snapshot.Database;
        Sede = snapshot.Sede;
        DbConexion = snapshot.DbConexion;
        Vendedor = snapshot.Vendedor;
        BaseCaja = snapshot.BaseCaja;
        UsuarioCaja = snapshot.UsuarioCaja;
    }

    public SessionSnapshot? CreateSnapshot()
    {
        if (!IsAuthenticated)
        {
            return null;
        }

        return new SessionSnapshot
        {
            Database = Database,
            Sede = Sede,
            DbConexion = DbConexion,
            Vendedor = Vendedor,
            BaseCaja = BaseCaja,
            UsuarioCaja = UsuarioCaja
        };
    }

    public void SetBaseCaja(BaseCajaInfo? baseCaja)
    {
        BaseCaja = baseCaja;
    }

    public void Clear()
    {
        Database = string.Empty;
        Sede = null;
        DbConexion = null;
        Vendedor = null;
        BaseCaja = null;
        UsuarioCaja = null;
    }
}

public sealed class SessionSnapshot
{
    public string Database { get; init; } = string.Empty;
    public SedeInfo? Sede { get; init; }
    public DbConexionInfo? DbConexion { get; init; }
    public VendedorInfo? Vendedor { get; init; }
    public BaseCajaInfo? BaseCaja { get; init; }
    public RVendorUsuarioInfo? UsuarioCaja { get; init; }
}
