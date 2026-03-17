using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using Meseros.Blazor.Models;

namespace Meseros.Blazor.Services;

public sealed class LoginDataService
{
    private readonly IConfiguration _configuration;

    public LoginDataService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string SanitizeDb(string? db)
    {
        if (string.IsNullOrWhiteSpace(db))
        {
            return string.Empty;
        }

        return db.Replace("/", string.Empty)
            .Replace("\\", string.Empty)
            .Replace("..", string.Empty)
            .Trim();
    }

    public async Task<LoginBootstrapData?> LoadBootstrapAsync(string db)
    {
        var safeDb = SanitizeDb(db);
        if (string.IsNullOrWhiteSpace(safeDb))
        {
            return null;
        }

        var sede = await QuerySingleAsync(
            safeDb,
            "SELECT TOP 1 id, nombreSede, guidSede FROM Sede WHERE id > 0",
            reader => new SedeRecord
            {
                id = reader.GetInt32(reader.GetOrdinal("id")),
                nombreSede = reader["nombreSede"] as string,
                guidSede = reader.GetGuid(reader.GetOrdinal("guidSede"))
            });

        if (sede is null)
        {
            return null;
        }

        var vendedores = await QueryListAsync(
            safeDb,
            "SELECT id, nombreVendedor, telefonoVendedor, calveVendedor, cajaMovil FROM Vendedor ORDER BY nombreVendedor",
            reader => new VendedorRecord
            {
                id = reader.GetInt32(reader.GetOrdinal("id")),
                nombreVendedor = reader["nombreVendedor"] as string,
                telefonoVendedor = reader["telefonoVendedor"] as string,
                calveVendedor = reader["calveVendedor"] as string,
                cajaMovil = reader.GetInt32(reader.GetOrdinal("cajaMovil"))
            });

        var imagen = await QuerySingleAsync(
            safeDb,
            "SELECT TOP 1 id, imagenBytes FROM Imagenes WHERE id = @id",
            reader => new ImagenRecord
            {
                id = reader.GetGuid(reader.GetOrdinal("id")),
                imagenBytes = reader["imagenBytes"] == DBNull.Value ? null : (byte[])reader["imagenBytes"]
            },
            new SqlParameter("@id", sede.guidSede));

        return new LoginBootstrapData
        {
            Db = safeDb,
            SedeId = sede.id,
            SedeGuid = sede.guidSede,
            SedeNombre = sede.nombreSede ?? string.Empty,
            LogoDataUrl = imagen?.imagenBytes is { Length: > 0 }
                ? $"data:image/png;base64,{Convert.ToBase64String(imagen.imagenBytes)}"
                : null,
            Vendedores = vendedores.Select(v => new VendedorInfo
            {
                Id = v.id,
                Nombre = v.nombreVendedor ?? string.Empty,
                Telefono = v.telefonoVendedor ?? string.Empty,
                Clave = v.calveVendedor ?? string.Empty,
                CajaMovil = v.cajaMovil == 1
            }).ToList()
        };
    }

    public async Task<LoginResult> AuthenticateAsync(string db, string usuario, string clave, int sedeId)
    {
        var safeDb = SanitizeDb(db);
        if (string.IsNullOrWhiteSpace(safeDb))
        {
            return Failure("No se encuentra la base de datos.");
        }

        var vendedor = await QuerySingleAsync(
            safeDb,
            "SELECT TOP 1 id, nombreVendedor, telefonoVendedor, calveVendedor, cajaMovil FROM Vendedor WHERE telefonoVendedor = @usuario AND calveVendedor = @clave",
            reader => new VendedorRecord
            {
                id = reader.GetInt32(reader.GetOrdinal("id")),
                nombreVendedor = reader["nombreVendedor"] as string,
                telefonoVendedor = reader["telefonoVendedor"] as string,
                calveVendedor = reader["calveVendedor"] as string,
                cajaMovil = reader.GetInt32(reader.GetOrdinal("cajaMovil"))
            },
            new SqlParameter("@usuario", usuario),
            new SqlParameter("@clave", clave));

        if (vendedor is null)
        {
            return Failure("Usuario o contraseña inválidos.");
        }

        var result = new LoginResult
        {
            Success = true,
            Db = safeDb,
            SedeId = sedeId,
            VendedorId = vendedor.id,
            VendedorNombre = vendedor.nombreVendedor ?? string.Empty
        };

        if (vendedor.cajaMovil != 1)
        {
            return result;
        }

        var usuarioCaja = await QuerySingleAsync(
            safeDb,
            "SELECT TOP 1 id, idVendedor, idUSuario FROM R_VendedorUsuario WHERE idVendedor = @idVendedor",
            reader => new R_VendedorUsuarioRecord
            {
                id = reader.GetInt32(reader.GetOrdinal("id")),
                idVendedor = reader.GetInt32(reader.GetOrdinal("idVendedor")),
                idUSuario = reader.GetInt32(reader.GetOrdinal("idUSuario"))
            },
            new SqlParameter("@idVendedor", vendedor.id));

        if (usuarioCaja is null)
        {
            return Failure("No tiene un usuario de caja relacionado, contacte al administrador.");
        }

        result.CajaUsuarioId = usuarioCaja.id;

        var tokenEmpresa = await QuerySingleAsync(
            safeDb,
            "SELECT TOP 1 token FROM tokenEmpresa WHERE id > 0",
            reader => new TokenEmpresaRecord
            {
                token = reader["token"] as string
            });

        result.TokenEmpresa = tokenEmpresa?.token ?? string.Empty;

        var baseActiva = await QuerySingleAsync(
            safeDb,
            "SELECT TOP 1 id, idUsuarioApertura FROM BaseCaja WHERE idUsuarioApertura = @idUsuario AND estadoBase = 'ACTIVA' ORDER BY id DESC",
            reader => new BaseCajaRecord
            {
                id = reader.GetInt32(reader.GetOrdinal("id")),
                idUsuarioApertura = reader.GetInt32(reader.GetOrdinal("idUsuarioApertura"))
            },
            new SqlParameter("@idUsuario", usuarioCaja.id));

        if (baseActiva is not null)
        {
            result.BaseCajaId = baseActiva.id;
            return result;
        }

        result.RequiresBaseAperture = true;
        return result;
    }

    public async Task<LoginResult> OpenBaseAsync(BaseCajaRequest request)
    {
        var safeDb = SanitizeDb(request.Db);
        if (string.IsNullOrWhiteSpace(safeDb))
        {
            return Failure("No se encuentra la base de datos.");
        }

        if (request.ValorBase <= 0)
        {
            return Failure("Ingrese un valor de base válido.");
        }

        var payload = new BaseCajaRecord
        {
            id = 0,
            fechaApertura = DateTime.Now,
            idUsuarioApertura = request.CajaUsuarioId,
            valorBase = request.ValorBase,
            fechaCierre = DateTime.Now,
            idUsuarioCierre = request.CajaUsuarioId,
            estadoBase = "ACTIVA",
            idSedeBAse = request.SedeId <= 0 ? 1 : request.SedeId
        };

        var respuesta = await ExecuteCrudAsync(safeDb, payload, 0);
        if (respuesta is null || !respuesta.estado || !int.TryParse(respuesta.FinalId, out var baseCajaId))
        {
            return Failure(respuesta?.mensaje ?? "No fue posible aperturar la caja.");
        }

        return new LoginResult
        {
            Success = true,
            Db = safeDb,
            SedeId = request.SedeId,
            VendedorId = request.VendedorId,
            VendedorNombre = request.VendedorNombre,
            CajaUsuarioId = request.CajaUsuarioId,
            BaseCajaId = baseCajaId,
            TokenEmpresa = request.TokenEmpresa
        };
    }

    private async Task<CrudResponse?> ExecuteCrudAsync<T>(string db, T payload, int funcion)
    {
        await using var connection = new SqlConnection(BuildConnectionString(db));
        await connection.OpenAsync();

        var sql = $"EXEC [dbo].[CRUD_{typeof(T).Name}] @json = @json, @funcion = @funcion";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@json", JsonSerializer.Serialize(payload));
        command.Parameters.AddWithValue("@funcion", funcion);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        var json = reader[0]?.ToString();
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<CrudResponse>(json);
    }

    private async Task<T?> QuerySingleAsync<T>(string db, string sql, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
    {
        var list = await QueryListAsync(db, sql, map, parameters);
        return list.FirstOrDefault();
    }

    private async Task<List<T>> QueryListAsync<T>(string db, string sql, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
    {
        var results = new List<T>();

        await using var connection = new SqlConnection(BuildConnectionString(db));
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);
        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        while (await reader.ReadAsync())
        {
            results.Add(map(reader));
        }

        return results;
    }

    private string BuildConnectionString(string db)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = GetSetting("COMANDASVENTAS_DB_SERVER", "ComandasVentas:DbServer", "www.serinsispc.com"),
            InitialCatalog = db,
            UserID = GetSetting("COMANDASVENTAS_DB_USER", "ComandasVentas:DbUser", "emilianop"),
            Password = GetSetting("COMANDASVENTAS_DB_PASSWORD", "ComandasVentas:DbPassword", "Ser1ns1s@2020*"),
            TrustServerCertificate = true,
            MultipleActiveResultSets = true,
            PersistSecurityInfo = true
        };

        return builder.ConnectionString;
    }

    private string GetSetting(string envName, string configKey, string fallback)
    {
        var envValue = Environment.GetEnvironmentVariable(envName);
        if (!string.IsNullOrWhiteSpace(envValue))
        {
            return envValue.Trim();
        }

        var configValue = _configuration[configKey];
        if (!string.IsNullOrWhiteSpace(configValue))
        {
            return configValue.Trim();
        }

        return fallback;
    }

    private static LoginResult Failure(string message) => new()
    {
        Success = false,
        ErrorMessage = message
    };
}
