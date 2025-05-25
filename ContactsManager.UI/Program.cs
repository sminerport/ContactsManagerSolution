using OfficeOpenXml;

using Serilog;
using CRUDExample;
using CRUDExample.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((
    HostBuilderContext context,
    IServiceProvider services,
    LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration) // read configuration settings from built-in IConfiguration
        .ReadFrom.Services(services); // read current app's services and make them available to serilog
});

// Extension method to add services to the container, setting up the database context, and configuring Identity
builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

// Configure the EPPlus library to use non-commercial license
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

app.UseHsts();
app.UseHttpsRedirection();

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

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}");

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action}"
    );
});

//Admin/Home/Index
//Admin

app.Run();

public partial class Program


{ } // make the auto-generated Program accessible programmatically