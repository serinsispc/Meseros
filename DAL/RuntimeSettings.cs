using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace DAL
{
    public static class RuntimeSettings
    {
        private const string DefaultSqlServer = "www.serinsispc.com";
        private const string DefaultSqlUser = "emilianop";
        private const string DefaultSqlPassword = "Ser1ns1s@2020*";
        private static readonly Regex SafeDbNameRegex = new Regex(@"[^A-Za-z0-9_\-]", RegexOptions.Compiled);

        public static string BuildSqlConnectionString(string db)
        {
            var databaseName = SanitizeDatabaseName(db);
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ArgumentException("El nombre de la base de datos es invalido.", nameof(db));
            }

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = GetSetting("COMANDASVENTAS_DB_SERVER", "ComandasVentas.DbServer", DefaultSqlServer),
                InitialCatalog = databaseName,
                UserID = GetSetting("COMANDASVENTAS_DB_USER", "ComandasVentas.DbUser", DefaultSqlUser),
                Password = GetSetting("COMANDASVENTAS_DB_PASSWORD", "ComandasVentas.DbPassword", DefaultSqlPassword),
                TrustServerCertificate = true,
                MultipleActiveResultSets = true,
                PersistSecurityInfo = true
            };

            return builder.ConnectionString;
        }

        public static string GetDianApiBaseUrl()
        {
            return NormalizeUrl(
                GetSetting(
                    "COMANDASVENTAS_DIAN_API_BASE_URL",
                    "ComandasVentas.DianApiBaseUrl",
                    "https://erog.apifacturacionelectronica.xyz/api/ubl2.1/"
                )
            );
        }

        private static string SanitizeDatabaseName(string db)
        {
            return SafeDbNameRegex.Replace((db ?? string.Empty).Trim(), string.Empty);
        }

        private static string GetSetting(string envName, string appSettingName, string fallbackValue)
        {
            var envValue = Environment.GetEnvironmentVariable(envName);
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                return envValue.Trim();
            }

            var configValue = ConfigurationManager.AppSettings[appSettingName];
            if (!string.IsNullOrWhiteSpace(configValue))
            {
                return configValue.Trim();
            }

            return fallbackValue;
        }

        private static string NormalizeUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Trim().TrimEnd('/') + "/";
        }
    }
}
