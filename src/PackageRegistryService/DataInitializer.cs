// ref: https://pratikpokhrel51.medium.com/creating-data-seeder-in-ef-core-that-reads-from-json-file-in-dot-net-core-69004df7ad0a

using PackageRegistryService.Models;
using System.Text.Json;

namespace PackageRegistryService

{
    public class DataInitializer
    {
        public static List<ValidationPackageIndex> ReadIndex() 
        {
            var json = File.ReadAllText(@"Data/arc-validate-package-index.json");
            var index = JsonSerializer.Deserialize<List<ValidationPackageIndex>>(json);
            return index;
        }
        public static void SeedData(ValidationPackageDb context)
        {
            if (!context.ValidationPackages.Any())
            {
                var index = DataInitializer.ReadIndex();
                var validationPackages =
                    index
                        .Select(i =>
                            new ValidationPackage
                            {
                                Name = i.FileName,
                                Description = i.Metadata.Description,
                                MajorVersion = i.Metadata.MajorVersion,
                                MinorVersion = i.Metadata.MinorVersion,
                                PatchVersion = i.Metadata.PatchVersion,
                                PackageContent = File.ReadAllBytes($"StagingArea/{i.FileName}/{i.FileName}@{i.Metadata.MajorVersion}.{i.Metadata.MinorVersion}.{i.Metadata.PatchVersion}.fsx")
                            }
                        );
                context.AddRange(validationPackages);
                context.SaveChanges();
            }
        }
    }
}
