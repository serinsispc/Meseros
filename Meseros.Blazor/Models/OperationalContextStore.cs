namespace Meseros.Blazor.Models;

public sealed class OperationalContextStore
{
    public string Db { get; private set; } = string.Empty;
    public int VendedorId { get; private set; }
    public string VendedorNombre { get; private set; } = string.Empty;
    public string TokenEmpresa { get; private set; } = string.Empty;
    public int BaseCajaId { get; private set; }
    public int CajaUsuarioId { get; private set; }

    public void Set(string db, int vendedorId, string vendedorNombre, string tokenEmpresa, int baseCajaId, int cajaUsuarioId)
    {
        Db = db ?? string.Empty;
        VendedorId = vendedorId;
        VendedorNombre = vendedorNombre ?? string.Empty;
        TokenEmpresa = tokenEmpresa ?? string.Empty;
        BaseCajaId = baseCajaId;
        CajaUsuarioId = cajaUsuarioId;
    }

    public void Clear()
    {
        Db = string.Empty;
        VendedorId = 0;
        VendedorNombre = string.Empty;
        TokenEmpresa = string.Empty;
        BaseCajaId = 0;
        CajaUsuarioId = 0;
    }
}
