using ServiceContracts;
using Microsoft.EntityFrameworkCore;
using Entities;

using Services;
using OfficeOpenXml;

using Repositories;
using Serilog;
using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.PersonsListResultFilter;
using CRUDExample;
using CRUDExample.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration

    .ReadFrom.Configuration(context.Configuration) // read configuration settings from built-in IConfiguration
    .ReadFrom.Services(services); // read current app's services and make them available to serilog
});

builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

ExcelPackage.License.SetNonCommercialPersonal("Scott Miner");

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseExceptionHandlingMiddleware();
}

app.UseSerilogRequestLogging();

app.UseHttpLogging();

if (builder.Environment.IsEnvironment("Test") == false)
{
    Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", "Rotativa");
}

app.UseStaticFiles();
app.UseRouting(); // Identifying action method based on route
app.UseAuthentication(); // Reading Identity cookie
app.UseAuthorization(); // Validates access permissions of the user
app.MapControllers(); // Execute the filter pipeline (action + filters)

app.Run();

public partial class Program


{ } // make the auto-generated Program accessible programmatically