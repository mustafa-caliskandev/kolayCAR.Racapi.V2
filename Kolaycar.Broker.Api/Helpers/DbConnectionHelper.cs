using System;
using KolayCAR.Broker.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;

namespace KolayCAR.Broker.API.Helpers
{
    public class DbConnectionHelper
    {
        #region Descriptions
        private static DbConnectionHelper _dbConnectionHelper;

        private readonly IConfiguration _configuration;

        private string connectionString = "";
        public string ConnectionString
        {
            get => connectionString;
        }
        #endregion

        #region Instance
        public static DbConnectionHelper Instance()
        {
            return _dbConnectionHelper ?? new DbConnectionHelper();
        }
        #endregion

        /// <summary>
        /// Sadece Startup.cs icesinde new olarak kullanilmali aksi taktirde hata alinacaktir.!!!!
        /// </summary>
        /// <param name="configuration"></param>
        public DbConnectionHelper(IConfiguration configuration = null)
        {
            _dbConnectionHelper = this;

            _configuration = configuration ?? throw new InvalidOperationException(
                "DbConnectionHelper requires IConfiguration. Ensure appsettings.json is loaded before creating the helper.");

            #region Db Connection String Decrypt
            var appSettingsSection = _configuration.GetSection("AppSettings");
            var appSettings = appSettingsSection.Get<AppSettings>();
            var dbPassword = appSettingsSection.GetValue<string>("DbPassword");

            if (!appSettingsSection.Exists() ||
                appSettings == null ||
                string.IsNullOrWhiteSpace(appSettings.ConnectionString) ||
                string.IsNullOrWhiteSpace(dbPassword))
            {
                throw new InvalidOperationException(
                    "Required configuration is missing. Ensure appsettings.json is included in the publish output and AppSettings/DbPassword are populated.");
            }

            var connectionsStringBase = appSettings.ConnectionString.EndsWith(";") ? appSettings.ConnectionString : appSettings.ConnectionString + ";";
            var decryptedDbPassword = EncryptionHelper.Decrypt(dbPassword);
            connectionString = $"{connectionsStringBase}Password={decryptedDbPassword}";
            #endregion
        }
    }
}
