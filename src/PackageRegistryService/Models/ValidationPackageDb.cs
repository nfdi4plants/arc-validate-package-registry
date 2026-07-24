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
            var validationPackage = modelBuilder.Entity<ValidationPackage>();

            validationPackage.OwnsMany(v => v.Authors, author =>
            {
                author.ToJson();
            });

            validationPackage.OwnsMany(v => v.Tags, tag =>
            {
                tag.ToJson();
            });

            validationPackage.OwnsMany(v => v.Inputs, input =>
            {
                input.ToJson();

                // A JSON-owned collection needs an internal ordinal key. Without an
                // explicit key, EF treats CWL's semantic string Id property as that
                // key and consequently excludes it from the stored JSON document.
                input.Property<int>("__ordinal")
                    .ValueGeneratedOnAdd();
                input.HasKey(
                    "ValidationPackageName",
                    "ValidationPackageMajorVersion",
                    "ValidationPackageMinorVersion",
                    "ValidationPackagePatchVersion",
                    "ValidationPackagePreReleaseVersionSuffix",
                    "ValidationPackageBuildMetadataVersionSuffix",
                    "__ordinal");

                input.Property(i => i.Id)
                    .IsRequired()
                    .HasJsonPropertyName("id");
                input.Property(i => i.Label)
                    .HasJsonPropertyName("label");
                input.Property(i => i.Doc)
                    .HasJsonPropertyName("doc");

                input.OwnsOne(i => i.Type, inputType =>
                {
                    inputType.HasJsonPropertyName("type");
                    inputType.Property(t => t.PrimitiveType)
                        .HasConversion<CwlPrimitiveStorageConverter>()
                        .HasJsonPropertyName("primitiveType");
                    inputType.Property(t => t.IsNullable)
                        .HasJsonPropertyName("isNullable");
                });
                input.Navigation(i => i.Type).IsRequired();

                input.OwnsOne(i => i.InputBinding, binding =>
                {
                    binding.HasJsonPropertyName("inputBinding");
                    binding.Property(b => b.Position)
                        .HasJsonPropertyName("position");
                    binding.Property(b => b.Prefix)
                        .HasJsonPropertyName("prefix");
                    binding.Property(b => b.Separate)
                        .HasJsonPropertyName("separate");
                });
                input.Navigation(i => i.InputBinding).IsRequired();
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
