using Microsoft.EntityFrameworkCore;
using PackageRegistryService;
using PackageRegistryService.Models;
using Microsoft.AspNetCore.HttpOverrides;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using static System.Net.Mime.MediaTypeNames;
using PackageRegistryService.API;
using PackageRegistryService.Pages.Components;
using PackageRegistryService.Pages;

// ------------------------- ApplicationBuilder -------------------------
// in this section, we will add the necessary code to configure the application builder,
// which defines the application's configuration and services.
// This is the main Dependency Injection (DI) container.

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// configurte NSwag OpenAPI document with document-level settings
builder.Services.AddOpenApiDocument(settings =>
    {
        settings.Title = "ARC validation package registry API";
        settings.Version = "v1";
        settings.Description = "A simple API for retrieving ARC validation packages";
    });

builder.Services.AddEndpointsApiExplorer(); 

// Add database related services
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDbContext<ValidationPackageDb>(opt => 
    opt.UseNpgsql(
        // retrieve connection string from configuration via "PostgressConnectionString" key
        // this is found in the appsettings.json file locally, and in the environment variables when deployed
        connectionString: builder.Configuration.GetConnectionString("PostgressConnectionString")
    )
);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// ------------------------- WebApplication -------------------------
// in this section, we will add the necessary code to configure the WebApplication,
// which defines the HTTP request pipeline.

var app = builder.Build();

app.UseStaticFiles(); // serve wwwroot content https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-8.0

// serve OpenAPI document and Swagger UI
app.UseOpenApi();
app.UseSwaggerUi(settings => {
    settings.DocExpansion = "list";
    settings.ValidateSpecification = true;
}); 

if (app.Environment.IsDevelopment())
{
    // if we are in development mode, apply migrations and seed the database
    // otherwise do not touch the database, and apply necessary migrations by exporting migration sql scripts
    // e.g. via `dotnet ef migrations script`
    using (var scope = app.Services.CreateScope())
    {
        var ctx = scope.ServiceProvider.GetRequiredService<ValidationPackageDb>();
        ctx.Database.Migrate();
        DataInitializer.SeedData(ctx);
    }
    app.UseHttpsRedirection();
}

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();

}

// Configure the HTTP request pipeline.

// ======================== Packages endpoints =========================

// app.MapGet binds a response handler function to a HTTP request on a specific route pattern

app.MapGroup("/api/v1")
    .MapApiV1()
    .WithTags("Packages");

app.MapGroup("/")
    .MapPageEndpoints();

app.Run();
