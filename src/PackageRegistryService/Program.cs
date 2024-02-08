using Microsoft.EntityFrameworkCore;
using PackageRegistryService.Models;
using PackageRegistryService;

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

// ------------------------- WebApplication -------------------------
// in this section, we will add the necessary code to configure the WebApplication,
// which defines the HTTP request pipeline.

var app = builder.Build();

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

// Configure the HTTP request pipeline.

// app.MapGet binds a response handler function to a HTTP request on a specific route pattern
app.MapGet("/api/v1/packages", async (ValidationPackageDb db) =>
{

    return await db.ValidationPackages.ToListAsync();
})
// add metadata to the endpoint for OpenAPI document generation
.WithOpenApi()
.WithName("GetAllPackages")
.WithSummary("This is a summary")
.WithDescription("This is a description")
.WithTags("Packages");

app.MapGet("/api/v1/packages/{name}", async (string name, ValidationPackageDb db) =>
{
    return await
        db.ValidationPackages
        .Where(p => p.Name == name)
        .OrderByDescending(p => p.MajorVersion)
        .ThenByDescending(p => p.MinorVersion)
        .ThenByDescending(p => p.PatchVersion)
        .FirstOrDefaultAsync();
})
.WithName("GetLatestPackageByName")
.WithTags("Packages");

app.MapGet("/api/v1/packages/{name}/{version}", async (string name, string version, ValidationPackageDb db) =>
{
    var splt = version.Split('.');
    if (splt.Length != 3)
    {
        return Results.BadRequest("version was not a of valid format MAJOR.MINOR.REVISION");
    }

    var major = int.Parse(splt[0]);
    var minor = int.Parse(splt[1]);
    var revision = int.Parse(splt[2]);

    return await db.ValidationPackages.FindAsync(name, major, minor, revision)
        is ValidationPackage package
            ? Results.Ok(package)
            : Results.NotFound();

})
.WithName("GetPackageByNameAndVersion")
.WithOpenApi()
.WithTags("Packages");

app.MapPost("/api/v1/packages", async (ValidationPackage package, ValidationPackageDb db) =>
{
    if (await db.ValidationPackages.FindAsync(package.Name, package.MajorVersion, package.MinorVersion, package.PatchVersion) is not null)
    {
        return Results.Conflict();
    }

    var version = package.GetSemanticVersionString();

    await db.ValidationPackages.AddAsync(package);
    await db.SaveChangesAsync();

    return Results.Created($"/api/v1/packages/{package.Name}/{version}", package);
})
.WithName("CreatePackage")
.WithOpenApi()
.WithTags("Packages");

app.Run();
