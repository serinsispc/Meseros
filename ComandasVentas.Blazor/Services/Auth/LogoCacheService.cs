using System.Data;
using Microsoft.Data.SqlClient;

namespace ComandasVentas.Blazor.Services.Auth;

public sealed class LogoCacheService(
    IWebHostEnvironment environment,
    IConfiguration configuration,
    ILogger<LogoCacheService> logger)
{
    private const string DefaultSqlServer = "www.serinsispc.com";
    private const string DefaultSqlUser = "emilianop";
    private const string DefaultSqlPassword = "Ser1ns1s@2020*";

    public async Task<string> EnsureLogoAsync(string? db, Guid guidSede, CancellationToken cancellationToken = default)
    {
        var database = SanitizeDatabaseName(db);
        if (string.IsNullOrWhiteSpace(database))
        {
            return "/Recursos/Imagenes/Logo/Logo.png";
        }

        var logosDirectory = Path.Combine(environment.WebRootPath, "Recursos", "Imagenes", "Logo");
        Directory.CreateDirectory(logosDirectory);

        var logoFileName = $"{database}.png";
        var logoFilePath = Path.Combine(logosDirectory, logoFileName);

        if (File.Exists(logoFilePath))
        {
            return $"/Recursos/Imagenes/Logo/{logoFileName}?v={File.GetLastWriteTimeUtc(logoFilePath).Ticks}";
        }

        if (guidSede == Guid.Empty)
        {
            return "/Recursos/Imagenes/Logo/Logo.png";
        }

        try
        {
            await using var connection = new SqlConnection(BuildSqlConnectionString(database));
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                SELECT TOP (1) imagenBytes
                FROM Imagenes
                WHERE id = @id
                """;

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", guidSede);

            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
            if (await reader.ReadAsync(cancellationToken) &&
                reader["imagenBytes"] is byte[] imageBytes &&
                imageBytes.Length > 0)
            {
                await File.WriteAllBytesAsync(logoFilePath, imageBytes, cancellationToken);
                return $"/Recursos/Imagenes/Logo/{logoFileName}?v={File.GetLastWriteTimeUtc(logoFilePath).Ticks}";
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No fue posible cachear el logo de la base {Database}.", database);
        }

        return "/Recursos/Imagenes/Logo/Logo.png";
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

    private static string SanitizeDatabaseName(string? db)
    {
        return string.Concat((db ?? string.Empty).Trim().Where(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-'));
    }
}
