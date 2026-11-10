# Local development

Shared Development settings are stored in `src/iMasjid.Api/appsettings.Development.json`.
Local database and secret values belong in the ignored `src/iMasjid.Api/Secrets.Local.Json` file. Never commit that file.

## Configure local settings

The Development environment loads `Secrets.Local.Json` automatically when it exists. Create it with the local values required by your machine.

For example, set local environment variables in PowerShell before starting the API:

```powershell
New-Item -ItemType File -Path .\src\iMasjid.Api\Secrets.Local.Json -Force
```

```powershell
dotnet run --project src/iMasjid.Api
```

The API also requires `Jwt:Issuer`, `Jwt:Audience`, and `Cors:AllowedOrigins`; the tracked `appsettings.json` contains non-secret defaults for these values.

## Database cutover

Before deploying the hashed OTP and refresh-token implementation to an existing database, run:

```text
src/iMasjid.Infrastructure/Persistence/Scripts/SecurityTokenCutover.sql
```

The script invalidates existing OTPs and refresh tokens because old plaintext values cannot be safely converted.

## Tests

The PostgreSQL smoke tests require:

- A running PostgreSQL instance
- The schema and seed scripts applied
- A seeded `superadmin` account expected by the tests
- The same JWT and `Security:OtpHashKey` settings configured through `Secrets.Local.Json`

Run the tests with:

```powershell
dotnet test iMasjid.sln
```

Do not commit `Secrets.Local.Json`, connection strings, test database passwords, JWT keys, OTP-HMAC keys, SMTP credentials, or Hangfire passwords.
