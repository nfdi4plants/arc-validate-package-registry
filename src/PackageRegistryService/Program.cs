using Microsoft.EntityFrameworkCore;
using PackageRegistryService.Models;
using SemanticVersioning;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<ValidationPackageDb>(opt => opt.UseInMemoryDatabase("ValidationPackageRegistry"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/v1/packages", async (ValidationPackageDb db) =>
{
    return await db.ValidationPackages.ToListAsync();
})
.WithName("GetPackages(v1)")
.WithOpenApi();

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
.WithName("GetLatestPackageByName(v1)")
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
.WithName("GetPackageByName(v1)")
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
.WithName("CreatePackage(v1)")
.WithOpenApi();

app.Run();
