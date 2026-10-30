# Local development

The API does not keep database, JWT, OTP-HMAC, SMTP, or Hangfire credentials in tracked files.

## Configure User Secrets

Run these commands from the repository root and replace placeholder values with local-only values:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=iMasjid;Username=postgres;Password=<local-password>" --project src/iMasjid.Api
dotnet user-secrets set "Jwt:SecretKey" "<at-least-32-byte-development-key>" --project src/iMasjid.Api
dotnet user-secrets set "Security:OtpHashKey" "<at-least-32-byte-development-hmac-key>" --project src/iMasjid.Api
dotnet user-secrets set "Hangfire:DashboardUsername" "<local-dashboard-user>" --project src/iMasjid.Api
dotnet user-secrets set "Hangfire:DashboardPassword" "<local-dashboard-password>" --project src/iMasjid.Api
```

The API also requires `Jwt:Issuer`, `Jwt:Audience`, and `Cors:AllowedOrigins`; the tracked `appsettings.json` contains non-secret development defaults for these values.

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
- The same JWT and `Security:OtpHashKey` settings configured through User Secrets

Run the tests with:

```powershell
dotnet test iMasjid.sln
```

Do not commit User Secrets, connection strings, test database passwords, JWT keys, or OTP-HMAC keys.
