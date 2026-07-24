using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PackageRegistryService.Models;

namespace PackageRegistryTestHost;

public sealed class PackageRegistryWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = $"PackageRegistryTests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ValidationPackageDb>();
            services.RemoveAll<DbContextOptions<ValidationPackageDb>>();

            services.AddDbContext<ValidationPackageDb>(options =>
                options.UseInMemoryDatabase(databaseName));
        });
    }

    public Task SeedPackageAsync(ValidationPackage package) =>
        SeedPackagesAsync([package]);

    public async Task SeedPackagesAsync(IEnumerable<ValidationPackage> packages)
    {
        using var scope = Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ValidationPackageDb>();

        await database.Database.EnsureCreatedAsync();
        foreach (var package in packages)
        {
            database.ValidationPackages.Add(package);
            database.Hashes.Add(new PackageContentHash
            {
                PackageName = package.Name,
                PackageMajorVersion = package.MajorVersion,
                PackageMinorVersion = package.MinorVersion,
                PackagePatchVersion = package.PatchVersion,
                PackagePreReleaseVersionSuffix = package.PreReleaseVersionSuffix,
                PackageBuildMetadataVersionSuffix = package.BuildMetadataVersionSuffix,
                Hash = package.GetPackageContentHash()
            });
            database.Downloads.Add(new PackageDownloads
            {
                PackageName = package.Name,
                PackageMajorVersion = package.MajorVersion,
                PackageMinorVersion = package.MinorVersion,
                PackagePatchVersion = package.PatchVersion,
                PackagePreReleaseVersionSuffix = package.PreReleaseVersionSuffix,
                PackageBuildMetadataVersionSuffix = package.BuildMetadataVersionSuffix,
                Downloads = 0
            });
        }

        await database.SaveChangesAsync();
    }
}
