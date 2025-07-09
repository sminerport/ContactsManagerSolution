# ContactsManager

ContactsManager is an ASP.NET Core MVC application for managing contacts. It provides functionality to add, edit, delete and list contacts with CSV, Excel and PDF export options. The project uses Entity Framework Core, Identity for authentication, and Serilog for logging.

## Requirements

- .NET 7 SDK (for building and running the application)
- SQL Server (or SQL Server Express / LocalDB) if you want to use a real database instead of the in-memory data provided in `persons.json` and `countries.json`.

## Building and Running

```bash
# Restore packages
 dotnet restore

# Build the solution
 dotnet build

# Run the web application
 dotnet run --project ContactsManager.UI
```

The site will start on `https://localhost:5001` by default. Adjust the port in `launchSettings.json` if needed.

## Testing

The solution includes several test projects. To run them all use:

```bash
 dotnet test
```

## Logging

Application logs are written to `ContactsManager.UI/logs/`. This folder is excluded from version control via `.gitignore`.

## Security Notes

- Connection strings in `appsettings.json` are configured for local development. Override these values using environment variables or secret storage when deploying to production.
- A custom token based authorization filter used in earlier versions has been removed in favor of built-in Identity authentication.

Before publishing the site, run `dotnet list package --vulnerable` to check for known package vulnerabilities and update dependencies as needed.
