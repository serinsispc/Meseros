using System.Text.Json;
using Microsoft.JSInterop;

namespace ComandasVentas.Blazor.Services.Auth;

public sealed class PosSessionPersistenceService(IJSRuntime jsRuntime)
{
    private const string StorageKey = "serinsis.pos.session";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task SaveAsync(AppSessionState sessionState)
    {
        var snapshot = sessionState.CreateSnapshot();
        if (snapshot is null)
        {
            await ClearAsync();
            return;
        }

        var payload = JsonSerializer.Serialize(snapshot, JsonOptions);
        await jsRuntime.InvokeVoidAsync("serinsisPosSession.save", StorageKey, payload);
    }

    public async Task<bool> TryRestoreAsync(AppSessionState sessionState, string? expectedDatabase = null)
    {
        var payload = await jsRuntime.InvokeAsync<string?>("serinsisPosSession.load", StorageKey);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        SessionSnapshot? snapshot;
        try
        {
            snapshot = JsonSerializer.Deserialize<SessionSnapshot>(payload, JsonOptions);
        }
        catch (JsonException)
        {
            await ClearAsync();
            return false;
        }

        if (snapshot?.Vendedor is null || string.IsNullOrWhiteSpace(snapshot.Database))
        {
            await ClearAsync();
            return false;
        }

        if (!string.IsNullOrWhiteSpace(expectedDatabase) &&
            !string.Equals(snapshot.Database, expectedDatabase, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        sessionState.Restore(snapshot);
        return true;
    }

    public ValueTask ClearAsync() =>
        jsRuntime.InvokeVoidAsync("serinsisPosSession.clear", StorageKey);
}
