namespace PackageRegistryService.Models
{

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using System.Reflection.Metadata;

    public class ValidationPackageDb : DbContext
    {
        public ValidationPackageDb(DbContextOptions<ValidationPackageDb> options)
            : base(options) { }

        public DbSet<ValidationPackage> ValidationPackages => Set<ValidationPackage>();
    }
}
