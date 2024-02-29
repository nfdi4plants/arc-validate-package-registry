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
        public DbSet<PackageContentHash> Hashes => Set<PackageContentHash>();

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
    }
}
