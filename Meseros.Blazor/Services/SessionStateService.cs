using Meseros.Blazor.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Meseros.Blazor.Services;

public sealed class SessionStateService
{
    private const string SessionKey = "meseros.operational-context";
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly OperationalContextStore _contextStore;

    public SessionStateService(ProtectedSessionStorage sessionStorage, OperationalContextStore contextStore)
    {
        _sessionStorage = sessionStorage;
        _contextStore = contextStore;
    }

    public OperationalContextStore Context => _contextStore;

    public async Task SaveAsync(string db, int vendedorId, string vendedorNombre, string tokenEmpresa, int baseCajaId, int cajaUsuarioId)
    {
        _contextStore.Set(db, vendedorId, vendedorNombre, tokenEmpresa, baseCajaId, cajaUsuarioId);
        await _sessionStorage.SetAsync(SessionKey, new SessionPayload
        {
            Db = db,
            VendedorId = vendedorId,
            VendedorNombre = vendedorNombre,
            TokenEmpresa = tokenEmpresa,
            BaseCajaId = baseCajaId,
            CajaUsuarioId = cajaUsuarioId
        });
    }

    public async Task<bool> RestoreAsync()
    {
        if (_contextStore.IsAuthenticated)
        {
            return true;
        }

        var result = await _sessionStorage.GetAsync<SessionPayload>(SessionKey);
        if (!result.Success || result.Value is null)
        {
            _contextStore.Clear();
            return false;
        }

        _contextStore.Set(
            result.Value.Db,
            result.Value.VendedorId,
            result.Value.VendedorNombre,
            result.Value.TokenEmpresa,
            result.Value.BaseCajaId,
            result.Value.CajaUsuarioId);

        return _contextStore.IsAuthenticated;
    }

    public async Task ClearAsync()
    {
        _contextStore.Clear();
        await _sessionStorage.DeleteAsync(SessionKey);
    }

    private sealed class SessionPayload
    {
        public string Db { get; set; } = string.Empty;
        public int VendedorId { get; set; }
        public string VendedorNombre { get; set; } = string.Empty;
        public string TokenEmpresa { get; set; } = string.Empty;
        public int BaseCajaId { get; set; }
        public int CajaUsuarioId { get; set; }
    }
}
