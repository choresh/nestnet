# Set environment variables
[Environment]::SetEnvironmentVariable("MSSQL_SERVER", "localhost", "User")
[Environment]::SetEnvironmentVariable("MSSQL_DB_NAME", "MyDb1", "User")
[Environment]::SetEnvironmentVariable("MSSQL_USER", "???", "User")
[Environment]::SetEnvironmentVariable("MSSQL_PASSWORD", "???", "User")
[Environment]::SetEnvironmentVariable("MSSQL_TRUST_SERVER_CERTIFICATE", "true", "User")
[Environment]::SetEnvironmentVariable("MSSQL_TRUSTED_CONNECTION", "false", "User")
[Environment]::SetEnvironmentVariable("MSSQL_MULTIPLE_ACTIVE_RESULT_SETS", "false", "User")
