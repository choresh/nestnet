using NestNet.Cli.Infra;
using DbType = NestNet.Infra.Enums.DbType;

namespace NestNet.Cli.Generators.AppGenerator
{
    public abstract class AppGeneratorBase
    {
        protected AppType AppType { get; }

        protected AppGeneratorBase(AppType appType)
        {
            AppType = appType;
        }

        public void Generate(AppGenerationContext context)
        {
            Directory.CreateDirectory(context.AppPath);
            DoGenerate(context); // Call derived class.
            GenerateAppConfigFiles(context);
        }

        public abstract void DoGenerate(AppGenerationContext context);

        public AppGenerationContext? CreateAppGenerationContext()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var baseProjectName = new DirectoryInfo(currentDir).Name;
            var currProjectName = $"{baseProjectName}.{AppType}";
            var appPath = Path.Combine(currentDir, currProjectName);

            if (!Helpers.CheckTarDir(appPath))
            {
                return null;
            }

            // No need to ask for DbType (as it should be determined from Core project).
            var dbType = DetermineDbTypeFromCore(currentDir, baseProjectName);

            return new AppGenerationContext
            {
                BaseProjectName = baseProjectName,
                CurrProjectName = currProjectName,
                DbType = dbType,
                AppPath = appPath
            };
        }

        private static void GenerateAppConfigFiles(AppGenerationContext context)
        {
            var appSettingsContent = @"{
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Information"",
      ""Microsoft.AspNetCore"": ""Warning""
    }
  }
}";

            var appSettingsDevelopmentContent = @"{
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Information"",
      ""Microsoft.AspNetCore"": ""Warning""
    }
  }
}";

            var launchSettingsContent = $@"{{
    ""profiles"": {{
        ""{context.CurrProjectName}"": {{
            ""commandName"": ""Project"",
            ""launchBrowser"": true,
            ""launchUrl"": ""swagger"",
            ""applicationUrl"": ""https://localhost:7001;http://localhost:5001"",
            ""environmentVariables"": {{
                ""ASPNETCORE_ENVIRONMENT"": ""Development""
            }}
        }}
    }}
}}";

            File.WriteAllText(Path.Combine(context.AppPath, "appsettings.json"), appSettingsContent);
            File.WriteAllText(Path.Combine(context.AppPath, "appsettings.Development.json"), appSettingsDevelopmentContent);

            var tarDir = Path.Combine(context.AppPath, "Properties");
            Directory.CreateDirectory(tarDir);
            File.WriteAllText(Path.Combine(tarDir, "launchSettings.json"), launchSettingsContent);
        }

        private static DbType DetermineDbTypeFromCore(string currentDir, string baseProjectName)
        {
            var coreCsprojPath = Path.Combine(currentDir, $"{baseProjectName}.Core", $"{baseProjectName}.Core.csproj");
            if (!File.Exists(coreCsprojPath))
            {
                throw new Exception("Core project not found in current directory");
            }

            var csprojContent = File.ReadAllText(coreCsprojPath);

            DbType dbType;
            if (csprojContent.Contains("Npgsql.EntityFrameworkCore.PostgreSQL"))
            {
                dbType = DbType.Postgres;
            }
            else if (csprojContent.Contains("Microsoft.EntityFrameworkCore.SqlServer"))
            {
                dbType = DbType.Postgres;
            }
            else
            {
                throw new Exception("Failed to detect Db Type at core project");
            }

            return dbType;
        }


        protected static string GetConnectionStringMethod(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.MsSql:
                    return GetMsSqlConnectionStringMethod();
                case DbType.Postgres:
                    return GetPostgresConnectionStringMethod();
                default:
                    throw new ArgumentException($"DB type '{dbType}' not supported");
            }
        }

        protected static string GetMsSqlConnectionStringMethod()
        {
            return @"static string CreateConnectionString(string[] args)
{
    var server = ConfigHelper.GetConfigParam(args, ""MSSQL_SERVER"");
    var dbName = ConfigHelper.GetConfigParam(args, ""MSSQL_DB_NAME"");
    var user = ConfigHelper.GetConfigParam(args, ""MSSQL_USER"");
    var password = ConfigHelper.GetConfigParam(args, ""MSSQL_PASSWORD"");
    var trustServerCertificate = ConfigHelper.GetConfigParam(args, ""MSSQL_TRUST_SERVER_CERTIFICATE"", ""false"");
    var trustedConnection = ConfigHelper.GetConfigParam(args, ""MSSQL_TRUSTED_CONNECTION"", ""false"");
    var multipleActiveResultSets = ConfigHelper.GetConfigParam(args, ""MSSQL_MULTIPLE_ACTIVE_RESULT_SETS"", ""false"");

    return $""Server={server}; Database={dbName}; User Id={user}; Password={password}; TrustServerCertificate={trustServerCertificate}; Trusted_Connection={trustedConnection}; MultipleActiveResultSets={multipleActiveResultSets}"";
}";
        }

        private static string GetPostgresConnectionStringMethod()
        {
            return @"static string CreateConnectionString(string[] args)
{
    var server = ConfigHelper.GetConfigParam(args, ""POSTGRES_SERVER"");
    var dbName = ConfigHelper.GetConfigParam(args, ""POSTGRES_DB_NAME"");
    var user = ConfigHelper.GetConfigParam(args, ""POSTGRES_USER"");
    var password = ConfigHelper.GetConfigParam(args, ""POSTGRES_PASSWORD"");

    return $""Host={server}; Database={dbName}; Username={user}; Password={password}"";
}";
        }

        protected static string GetDbContextOptionsCode(DbType dbType, string connectionStringCode)
        {
            switch (dbType)
            {
                case DbType.MsSql:
                    return $"options.UseSqlServer({connectionStringCode})";
                case DbType.Postgres:
                    return $"options.UseNpgsql({connectionStringCode})";
                default:
                    throw new ArgumentException($"DB type '{dbType}' not supported");
            }
        }
    }
}