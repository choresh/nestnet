# Get environment variables
Write-Host "Environment variables set:"
Write-Host "POSTGRES_SERVER: $([Environment]::GetEnvironmentVariable('POSTGRES_SERVER', 'User'))"
Write-Host "POSTGRES_DB_NAME: $([Environment]::GetEnvironmentVariable('POSTGRES_DB_NAME', 'User'))"
Write-Host "POSTGRES_USER: $([Environment]::GetEnvironmentVariable('POSTGRES_USER', 'User'))"
Write-Host "POSTGRES_PASSWORD: $([Environment]::GetEnvironmentVariable('POSTGRES_PASSWORD', 'User'))"