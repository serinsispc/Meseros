using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using ComandasVentas.Blazor.Models.Auth;
using Microsoft.Data.SqlClient;

namespace ComandasVentas.Blazor.Services.Auth;

public sealed class LoginDataService(IConfiguration configuration)
{
    private const string DefaultSqlServer = "www.serinsispc.com";
    private const string DefaultSqlUser = "emilianop";
    private const string DefaultSqlPassword = "Ser1ns1s@2020*";
    private static readonly Regex SafeDbNameRegex = new("[^A-Za-z0-9_\\-]", RegexOptions.Compiled);

    public async Task<LoginBootstrapResult> LoadLoginAsync(string? db, CancellationToken cancellationToken = default)
    {
        var database = SanitizeDatabaseName(db);
        if (string.IsNullOrWhiteSpace(database))
        {
            return new LoginBootstrapResult
            {
                Database = string.Empty,
                ErrorMessage = "No se encuentra la base de datos en la URL. Usa ?db=NombreBase.",
                Sede = null,
                DbConexion = null,
                Vendedores = []
            };
        }

        try
        {
            await using var connection = new SqlConnection(BuildSqlConnectionString(database));
            await connection.OpenAsync(cancellationToken);

            var sede = await LoadSedeAsync(connection, cancellationToken);
            if (sede is null)
            {
                return new LoginBootstrapResult
                {
                    Database = database,
                    ErrorMessage = "No se encontro la sede configurada para esta base.",
                    Sede = null,
                    DbConexion = null,
                    Vendedores = []
                };
            }

            var vendedores = await LoadVendedoresAsync(connection, cancellationToken);
            var dbConexion = await LoadDbConexionAsync(connection, cancellationToken);

            return new LoginBootstrapResult
            {
                Database = database,
                Sede = sede,
                DbConexion = dbConexion,
                Vendedores = vendedores,
                ErrorMessage = vendedores.Count == 0 ? "No hay vendedores disponibles para iniciar sesion." : null
            };
        }
        catch (Exception ex)
        {
            return new LoginBootstrapResult
            {
                Database = database,
                ErrorMessage = $"No fue posible cargar el login para la base {database}. {ex.Message}",
                Sede = null,
                DbConexion = null,
                Vendedores = []
            };
        }
    }

    public async Task<LoginResult> AuthenticateAsync(string? db, string? usuario, string? clave, CancellationToken cancellationToken = default)
    {
        var database = SanitizeDatabaseName(db);
        if (string.IsNullOrWhiteSpace(database))
        {
            return Failure(database, "No se encuentra la base de datos en la URL.");
        }

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
        {
            return Failure(database, "Debes seleccionar un usuario e ingresar la contrasena.");
        }

        try
        {
            await using var connection = new SqlConnection(BuildSqlConnectionString(database));
            await connection.OpenAsync(cancellationToken);

            var sede = await LoadSedeAsync(connection, cancellationToken);
            var dbConexion = await LoadDbConexionAsync(connection, cancellationToken);
            var vendedor = await LoadVendedorByCredentialsAsync(connection, usuario.Trim(), clave.Trim(), cancellationToken);

            if (vendedor is null)
            {
                return Failure(database, "Usuario o contrasena no validos.", sede, dbConexion);
            }

            if (vendedor.CajaMovil == 1)
            {
                var usuarioCaja = await LoadVendedorUsuarioAsync(connection, vendedor.Id, cancellationToken);
                if (usuarioCaja is null)
                {
                    return Failure(database, "No tiene un usuario de caja relacionado, contacte al administrador.", sede, dbConexion, vendedor);
                }

                var baseCaja = await LoadBaseCajaActivaAsync(connection, usuarioCaja.IdUsuario, cancellationToken);
                if (baseCaja is null)
                {
                    return new LoginResult
                    {
                        Success = false,
                        Database = database,
                        ErrorMessage = "El usuario de caja no tiene una base activa. Debes aperturar caja antes de continuar.",
                        Sede = sede,
                        DbConexion = dbConexion,
                        Vendedor = vendedor,
                        UsuarioCaja = usuarioCaja,
                        RequireCajaApertura = true
                    };
                }

                return new LoginResult
                {
                    Success = true,
                    Database = database,
                    Sede = sede,
                    DbConexion = dbConexion,
                    Vendedor = vendedor,
                    UsuarioCaja = usuarioCaja,
                    BaseCaja = baseCaja
                };
            }

            return new LoginResult
            {
                Success = true,
                Database = database,
                Sede = sede,
                DbConexion = dbConexion,
                Vendedor = vendedor
            };
        }
        catch (Exception ex)
        {
            return Failure(database, $"No fue posible iniciar sesion. {ex.Message}");
        }
    }

    public async Task<LoginResult> AperturarBaseCajaAsync(
        string? db,
        VendedorInfo? vendedor,
        RVendorUsuarioInfo? usuarioCaja,
        SedeInfo? sede,
        decimal valorBase,
        CancellationToken cancellationToken = default)
    {
        var database = SanitizeDatabaseName(db);
        if (string.IsNullOrWhiteSpace(database))
        {
            return Failure(database, "No se encuentra la base de datos en la URL.", sede: sede, vendedor: vendedor);
        }

        if (vendedor is null || usuarioCaja is null)
        {
            return Failure(database, "No se encontro el contexto del usuario de caja para aperturar la base.", sede: sede, vendedor: vendedor);
        }

        if (valorBase <= 0)
        {
            return Failure(database, "Ingresa un valor de base valido.", sede: sede, vendedor: vendedor);
        }

        try
        {
            await using var connection = new SqlConnection(BuildSqlConnectionString(database));
            await connection.OpenAsync(cancellationToken);

            var dbConexion = await LoadDbConexionAsync(connection, cancellationToken);

            var baseNueva = new BaseCajaCrudPayload
            {
                id = 0,
                fechaApertura = DateTime.Now,
                idUsuarioApertura = usuarioCaja.IdUsuario,
                valorBase = valorBase,
                fechaCierre = DateTime.Now,
                idUsuarioCierre = usuarioCaja.IdUsuario,
                estadoBase = "ACTIVA",
                idSedeBAse = sede?.Id ?? 1
            };

            var respuesta = await ExecuteCrudAsync(connection, "CRUD_BaseCaja", baseNueva, 0, cancellationToken);
            if (!IsSuccessfulCrudResponse(respuesta))
            {
                return Failure(database, respuesta.mensaje ?? "No fue posible aperturar la caja.", sede, dbConexion, vendedor, usuarioCaja);
            }

            var baseCaja = respuesta.IdFinal > 0
                ? await LoadBaseCajaByIdAsync(connection, respuesta.IdFinal, cancellationToken)
                : await LoadBaseCajaActivaAsync(connection, usuarioCaja.IdUsuario, cancellationToken);

            if (baseCaja is null)
            {
                return Failure(database, "La caja fue aperturada, pero no se pudo recuperar la base activa.", sede, dbConexion, vendedor, usuarioCaja);
            }

            return new LoginResult
            {
                Success = true,
                Database = database,
                Sede = sede,
                DbConexion = dbConexion,
                Vendedor = vendedor,
                UsuarioCaja = usuarioCaja,
                BaseCaja = baseCaja
            };
        }
        catch (Exception ex)
        {
            return Failure(database, $"No fue posible aperturar la caja. {ex.Message}", sede: sede, vendedor: vendedor, usuarioCaja: usuarioCaja);
        }
    }

    public async Task<(bool Success, string Message, BaseCajaInfo? BaseCaja)> CerrarBaseCajaAsync(
        string? db,
        BaseCajaInfo? baseCaja,
        VendedorInfo? vendedor,
        CancellationToken cancellationToken = default)
    {
        var database = SanitizeDatabaseName(db);
        if (string.IsNullOrWhiteSpace(database))
        {
            return (false, "No se encuentra la base de datos en la URL.", null);
        }

        if (baseCaja is null || baseCaja.Id <= 0)
        {
            return (false, "No se encontro una base activa para cerrar.", null);
        }

        if (!string.Equals(baseCaja.EstadoBase, "ACTIVA", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "La caja ya se encuentra cerrada.", baseCaja);
        }

        try
        {
            await using var connection = new SqlConnection(BuildSqlConnectionString(database));
            await connection.OpenAsync(cancellationToken);

            var baseActualizada = new BaseCajaCrudPayload
            {
                id = baseCaja.Id,
                fechaApertura = baseCaja.FechaApertura,
                idUsuarioApertura = baseCaja.IdUsuarioApertura,
                valorBase = baseCaja.ValorBase,
                fechaCierre = DateTime.Now,
                idUsuarioCierre = vendedor?.Id ?? baseCaja.IdUsuarioCierre ?? baseCaja.IdUsuarioApertura,
                estadoBase = "CERRADA",
                idSedeBAse = baseCaja.IdSedeBase
            };

            var respuesta = await ExecuteCrudAsync(connection, "CRUD_BaseCaja", baseActualizada, 1, cancellationToken);
            if (!IsSuccessfulCrudResponse(respuesta))
            {
                return (false, respuesta.mensaje ?? "No fue posible cerrar la caja.", baseCaja);
            }

            var baseCerrada = await LoadBaseCajaByIdAsync(connection, baseCaja.Id, cancellationToken) ?? new BaseCajaInfo
            {
                Id = baseCaja.Id,
                FechaApertura = baseCaja.FechaApertura,
                IdUsuarioApertura = baseCaja.IdUsuarioApertura,
                ValorBase = baseCaja.ValorBase,
                FechaCierre = baseActualizada.fechaCierre,
                IdUsuarioCierre = baseActualizada.idUsuarioCierre,
                EstadoBase = "CERRADA",
                IdSedeBase = baseCaja.IdSedeBase
            };

            return (true, "Caja cerrada con exito.", baseCerrada);
        }
        catch (Exception ex)
        {
            return (false, $"No fue posible cerrar la caja. {ex.Message}", baseCaja);
        }
    }

    private async Task<SedeInfo?> LoadSedeAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) id, nombreSede, guidSede, porcentaje_propina
            FROM Sede
            WHERE id > 0
            ORDER BY id
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new SedeInfo
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            NombreSede = reader["nombreSede"] as string,
            GuidSede = reader["guidSede"] is Guid guid ? guid : Guid.Empty,
            PorcentajePropina = reader["porcentaje_propina"] is int porcentaje ? porcentaje : 0
        };
    }

    private async Task<List<VendedorInfo>> LoadVendedoresAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, nombreVendedor, telefonoVendedor, calveVendedor, cajaMovil
            FROM Vendedor
            ORDER BY nombreVendedor
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var vendedores = new List<VendedorInfo>();
        while (await reader.ReadAsync(cancellationToken))
        {
            vendedores.Add(new VendedorInfo
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                NombreVendedor = Convert.ToString(reader["nombreVendedor"]) ?? string.Empty,
                TelefonoVendedor = Convert.ToString(reader["telefonoVendedor"]) ?? string.Empty,
                ClaveVendedor = Convert.ToString(reader["calveVendedor"]) ?? string.Empty,
                CajaMovil = reader["cajaMovil"] is int cajaMovil ? cajaMovil : 0
            });
        }

        return vendedores;
    }

    private async Task<VendedorInfo?> LoadVendedorByCredentialsAsync(SqlConnection connection, string usuario, string clave, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) id, nombreVendedor, telefonoVendedor, calveVendedor, cajaMovil
            FROM Vendedor
            WHERE telefonoVendedor = @usuario AND calveVendedor = @clave
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@usuario", usuario);
        command.Parameters.AddWithValue("@clave", clave);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new VendedorInfo
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            NombreVendedor = Convert.ToString(reader["nombreVendedor"]) ?? string.Empty,
            TelefonoVendedor = Convert.ToString(reader["telefonoVendedor"]) ?? string.Empty,
            ClaveVendedor = Convert.ToString(reader["calveVendedor"]) ?? string.Empty,
            CajaMovil = reader["cajaMovil"] is int cajaMovil ? cajaMovil : 0
        };
    }

    private async Task<DbConexionInfo?> LoadDbConexionAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) NombreCaja, meserosCompartidos, ComandasCaja, ServidorImpresora
            FROM DBConexion
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new DbConexionInfo
        {
            NombreCaja = Convert.ToString(reader["NombreCaja"]),
            MeserosCompartidos = reader["meserosCompartidos"] is bool meserosCompartidos && meserosCompartidos,
            ComandasCaja = reader["ComandasCaja"] is bool comandasCaja && comandasCaja,
            ServidorImpresora = reader["ServidorImpresora"] is bool servidorImpresora && servidorImpresora
        };
    }

    private async Task<RVendorUsuarioInfo?> LoadVendedorUsuarioAsync(SqlConnection connection, int idVendedor, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) id, idVendedor, idUSuario
            FROM R_VendedorUsuario
            WHERE idVendedor = @idVendedor
            ORDER BY id DESC
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idVendedor", idVendedor);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new RVendorUsuarioInfo
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            IdVendedor = reader.GetInt32(reader.GetOrdinal("idVendedor")),
            IdUsuario = reader.GetInt32(reader.GetOrdinal("idUSuario"))
        };
    }

    private async Task<BaseCajaInfo?> LoadBaseCajaActivaAsync(SqlConnection connection, int idUsuario, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) id, fechaApertura, idUsuarioApertura, valorBase, fechaCierre, idUsuarioCierre, estadoBase, idSedeBAse
            FROM BaseCaja
            WHERE idUsuarioApertura = @idUsuario AND estadoBase = 'ACTIVA'
            ORDER BY id DESC
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idUsuario", idUsuario);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new BaseCajaInfo
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            FechaApertura = reader.GetDateTime(reader.GetOrdinal("fechaApertura")),
            IdUsuarioApertura = reader.GetInt32(reader.GetOrdinal("idUsuarioApertura")),
            ValorBase = reader.GetDecimal(reader.GetOrdinal("valorBase")),
            FechaCierre = reader["fechaCierre"] is DBNull ? null : reader.GetDateTime(reader.GetOrdinal("fechaCierre")),
            IdUsuarioCierre = reader["idUsuarioCierre"] is DBNull ? null : reader.GetInt32(reader.GetOrdinal("idUsuarioCierre")),
            EstadoBase = Convert.ToString(reader["estadoBase"]) ?? string.Empty,
            IdSedeBase = reader["idSedeBAse"] is int idSedeBase ? idSedeBase : 0
        };
    }

    private async Task<BaseCajaInfo?> LoadBaseCajaByIdAsync(SqlConnection connection, int idBaseCaja, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) id, fechaApertura, idUsuarioApertura, valorBase, fechaCierre, idUsuarioCierre, estadoBase, idSedeBAse
            FROM BaseCaja
            WHERE id = @idBaseCaja
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idBaseCaja", idBaseCaja);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new BaseCajaInfo
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            FechaApertura = reader.GetDateTime(reader.GetOrdinal("fechaApertura")),
            IdUsuarioApertura = reader.GetInt32(reader.GetOrdinal("idUsuarioApertura")),
            ValorBase = reader.GetDecimal(reader.GetOrdinal("valorBase")),
            FechaCierre = reader["fechaCierre"] is DBNull ? null : reader.GetDateTime(reader.GetOrdinal("fechaCierre")),
            IdUsuarioCierre = reader["idUsuarioCierre"] is DBNull ? null : reader.GetInt32(reader.GetOrdinal("idUsuarioCierre")),
            EstadoBase = Convert.ToString(reader["estadoBase"]) ?? string.Empty,
            IdSedeBase = reader["idSedeBAse"] is int idSedeBase ? idSedeBase : 0
        };
    }

    private static async Task<CrudResponse> ExecuteCrudAsync<T>(
        SqlConnection connection,
        string storedProcedureName,
        T payload,
        int funcion,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload).Replace("'", "''");
        var sql = $"EXEC [dbo].[{storedProcedureName}] @json = N'{json}', @funcion = {funcion}";

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return new CrudResponse { estado = false, mensaje = "Sin respuesta del servidor." };
        }

        return new CrudResponse
        {
            estado = ReadBool(reader, "estado"),
            mensaje = ReadString(reader, "mensaje"),
            data = ReadInt(reader, "data"),
            nuevoId = ReadInt(reader, "nuevoId"),
            idAfectado = ReadInt(reader, "idAfectado")
        };
    }

    private static bool ReadBool(SqlDataReader reader, string columnName)
    {
        var value = ReadValue(reader, columnName);
        if (value is null || value is DBNull)
        {
            return false;
        }

        return value switch
        {
            bool boolValue => boolValue,
            byte byteValue => byteValue != 0,
            short shortValue => shortValue != 0,
            int intValue => intValue != 0,
            long longValue => longValue != 0,
            string stringValue when bool.TryParse(stringValue, out var parsedBool) => parsedBool,
            string stringValue when int.TryParse(stringValue, out var parsedInt) => parsedInt != 0,
            _ => false
        };
    }

    private static int ReadInt(SqlDataReader reader, string columnName)
    {
        var value = ReadValue(reader, columnName);
        if (value is null || value is DBNull)
        {
            return 0;
        }

        return value switch
        {
            int intValue => intValue,
            long longValue => Convert.ToInt32(longValue),
            short shortValue => shortValue,
            decimal decimalValue => Convert.ToInt32(decimalValue),
            string stringValue when int.TryParse(stringValue, out var parsedInt) => parsedInt,
            _ => 0
        };
    }

    private static string? ReadString(SqlDataReader reader, string columnName)
    {
        var value = ReadValue(reader, columnName);
        return value is null || value is DBNull ? null : Convert.ToString(value);
    }

    private static object? ReadValue(SqlDataReader reader, string columnName)
    {
        for (var index = 0; index < reader.FieldCount; index++)
        {
            if (string.Equals(reader.GetName(index), columnName, StringComparison.OrdinalIgnoreCase))
            {
                return reader.GetValue(index);
            }
        }

        return null;
    }

    private static bool IsSuccessfulCrudResponse(CrudResponse response)
    {
        if (response.estado || response.IdFinal > 0)
        {
            return true;
        }

        var message = (response.mensaje ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return message.Contains("insert ok", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("update ok", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("actualizado", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("correctamente", StringComparison.OrdinalIgnoreCase);
    }

    private string BuildSqlConnectionString(string db)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = GetSetting("COMANDASVENTAS_DB_SERVER", "ComandasVentas:DbServer", DefaultSqlServer),
            InitialCatalog = db,
            UserID = GetSetting("COMANDASVENTAS_DB_USER", "ComandasVentas:DbUser", DefaultSqlUser),
            Password = GetSetting("COMANDASVENTAS_DB_PASSWORD", "ComandasVentas:DbPassword", DefaultSqlPassword),
            TrustServerCertificate = true,
            MultipleActiveResultSets = true,
            PersistSecurityInfo = true
        };

        return builder.ConnectionString;
    }

    public string SanitizeDatabaseName(string? db)
    {
        return SafeDbNameRegex.Replace((db ?? string.Empty).Trim(), string.Empty);
    }

    private string GetSetting(string envName, string configPath, string fallback)
    {
        var envValue = Environment.GetEnvironmentVariable(envName);
        if (!string.IsNullOrWhiteSpace(envValue))
        {
            return envValue.Trim();
        }

        var configValue = configuration[configPath];
        if (!string.IsNullOrWhiteSpace(configValue))
        {
            return configValue.Trim();
        }

        return fallback;
    }

    private static LoginResult Failure(
        string database,
        string message,
        SedeInfo? sede = null,
        DbConexionInfo? dbConexion = null,
        VendedorInfo? vendedor = null,
        RVendorUsuarioInfo? usuarioCaja = null)
    {
        return new LoginResult
        {
            Success = false,
            Database = database,
            ErrorMessage = message,
            Sede = sede,
            DbConexion = dbConexion,
            Vendedor = vendedor,
            UsuarioCaja = usuarioCaja
        };
    }

    private sealed class CrudResponse
    {
        public bool estado { get; init; }
        public string? mensaje { get; init; }
        public int IdFinal => data != 0 ? data : nuevoId;
        public int data { get; init; }
        public int nuevoId { get; init; }
        public int idAfectado { get; init; }
    }

    private sealed class BaseCajaCrudPayload
    {
        public int id { get; init; }
        public DateTime fechaApertura { get; init; }
        public int idUsuarioApertura { get; init; }
        public decimal valorBase { get; init; }
        public DateTime? fechaCierre { get; init; }
        public int? idUsuarioCierre { get; init; }
        public string estadoBase { get; init; } = string.Empty;
        public int idSedeBAse { get; init; }
    }
}
