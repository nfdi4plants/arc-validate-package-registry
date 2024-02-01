using Microsoft.EntityFrameworkCore;
using PackageRegistryService.Models;
using Microsoft.Extensions.Configuration;
using System.Configuration;


var builder = WebApplication.CreateBuilder(args);

var connString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDbContext<ValidationPackageDb>(opt => opt.UseInMemoryDatabase("ValidationPackageRegistry"));
builder.Services.AddDbContext<ValidationPackageDb>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgressConnectionString")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi(); // serve OpenAPI/Swagger documents
    app.UseReDoc(); // serve Swagger UI
    using (var scope = app.Services.CreateScope())
    {
        var ctx = scope.ServiceProvider.GetRequiredService<ValidationPackageDb>();
        ctx.Database.EnsureCreated();
    }
}

app.UseHttpsRedirection();

app.MapGet("/api/v1/packages", async (ValidationPackageDb db) =>
{
    return await db.ValidationPackages.ToListAsync();
})
.WithOpenApi()
.WithName("GetPackages")
.WithDescription("Get all packages")
.WithSummary("");


app.MapGet("/api/v1/packages/{name}", async (string name, ValidationPackageDb db) =>
{
    return await
        db.ValidationPackages
        .Where(p => p.Name == name)
        .OrderByDescending(p => p.MajorVersion)
        .ThenByDescending(p => p.MinorVersion)
        .ThenByDescending(p => p.RevisionVersion)
        .FirstOrDefaultAsync();
})
.WithName("GetLatestPackageByName")
.WithOpenApi();

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
.WithOpenApi();

app.MapPost("/api/v1/packages", async (ValidationPackage package, ValidationPackageDb db) =>
{
    if (await db.ValidationPackages.FindAsync(package.Name, package.MajorVersion, package.MinorVersion, package.RevisionVersion) is not null)
    {
        return Results.Conflict();
    }

    var version = package.GetSemanticVersionString();

    await db.ValidationPackages.AddAsync(package);
    await db.SaveChangesAsync();

    return Results.Created($"/api/v1/packages/{package.Name}/{version}", package);
})
.WithName("CreatePackage")
.WithOpenApi();

app.Run();
