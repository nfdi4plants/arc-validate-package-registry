// ref: https://pratikpokhrel51.medium.com/creating-data-seeder-in-ef-core-that-reads-from-json-file-in-dot-net-core-69004df7ad0a

using Microsoft.CodeAnalysis;
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

                context.SaveChanges();

                var validationPackages =
                    index
                        .Select(i =>
                            new ValidationPackage
                            {
                                Name = i.Metadata.Name,
                                Description = i.Metadata.Description,
                                MajorVersion = i.Metadata.MajorVersion,
                                MinorVersion = i.Metadata.MinorVersion,
                                PatchVersion = i.Metadata.PatchVersion,
                                PackageContent = File.ReadAllBytes($"StagingArea/{i.Metadata.Name}/{i.FileName}"),
                                ReleaseDate = new(i.LastUpdated.Year, i.LastUpdated.Month, i.LastUpdated.Day),
                                Tags = i.Metadata.Tags,
                                ReleaseNotes = i.Metadata.ReleaseNotes,
                                Authors = i.Metadata.Authors
                            }
                        );
                context.AddRange(validationPackages);
                context.SaveChanges();
            }
        }
    }
}
