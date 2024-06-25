namespace PackageRegistryService.Models
{

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using System.Reflection.Metadata;

    public class ValidationPackageDb : DbContext
    {
        public ValidationPackageDb(DbContextOptions<ValidationPackageDb> options) : base(options) { }

        public DbSet<ValidationPackage> ValidationPackages => Set<ValidationPackage>();
        public DbSet<PackageContentHash> Hashes => Set<PackageContentHash>();
        public DbSet<PackageDownloads> Downloads => Set<PackageDownloads>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ValidationPackage>()
            .OwnsMany(v => v.Authors, a =>
            {
                a.ToJson();
            })
            .OwnsMany(v => v.Tags, t =>
            {
                 t.ToJson();
            });
        }

        public static bool ValidatePackageContent(ValidationPackage package, ValidationPackageDb database)
        {
            var hash = database.Hashes.SingleOrDefault(h => h.PackageName == package.Name && h.PackageMajorVersion == package.MajorVersion && h.PackageMinorVersion == package.MinorVersion && h.PackagePatchVersion == package.PatchVersion && h.PackagePreReleaseVersionSuffix == package.PreReleaseVersionSuffix && h.PackageBuildMetadataVersionSuffix == package.BuildMetadataVersionSuffix);
            
            if (hash == null)
            {
                return false;
            }
            
            var packageHash = package.GetPackageContentHash();
            return hash.Hash == packageHash;
        }
        public static bool CreatePackageContentHash(ValidationPackage package, ValidationPackageDb database)
        {
            var result = database.Hashes.SingleOrDefault(d => d.PackageName == package.Name && d.PackageMajorVersion == package.MajorVersion && d.PackageMinorVersion == package.MinorVersion && d.PackagePatchVersion == package.PatchVersion && d.PackagePreReleaseVersionSuffix == package.PreReleaseVersionSuffix && d.PackageBuildMetadataVersionSuffix == package.BuildMetadataVersionSuffix);

            if (result != null)
            {
                return false; // there is an existing hash!
            }
            else
            {
                var h = new PackageContentHash
                {
                    PackageName = package.Name,
                    PackageMajorVersion = package.MajorVersion,
                    PackageMinorVersion = package.MinorVersion,
                    PackagePatchVersion = package.PatchVersion,
                    PackagePreReleaseVersionSuffix = package.PreReleaseVersionSuffix,
                    PackageBuildMetadataVersionSuffix = package.BuildMetadataVersionSuffix,
                    Hash = package.GetPackageContentHash()
                };
                database.Hashes.Add(h);
                return true;
            }
        }
        public static void IncrementDownloadCount(ValidationPackage package, ValidationPackageDb database)
        {
            var result = database.Downloads.SingleOrDefault(d => d.PackageName == package.Name && d.PackageMajorVersion == package.MajorVersion && d.PackageMinorVersion == package.MinorVersion && d.PackagePatchVersion == package.PatchVersion && d.PackagePreReleaseVersionSuffix == package.PreReleaseVersionSuffix && d.PackageBuildMetadataVersionSuffix == package.BuildMetadataVersionSuffix);

            if (result != null)
            {
                result.Downloads += 1; // increment download count for each package
            }
            else
            {
                var d = new PackageDownloads
                {
                    PackageName = package.Name,
                    PackageMajorVersion = package.MajorVersion,
                    PackageMinorVersion = package.MinorVersion,
                    PackagePatchVersion = package.PatchVersion,
                    PackagePreReleaseVersionSuffix = package.PreReleaseVersionSuffix,
                    PackageBuildMetadataVersionSuffix = package.BuildMetadataVersionSuffix,
                    Downloads = 1
                };
                database.Downloads.Add(d);
            }
        }
    }
}
