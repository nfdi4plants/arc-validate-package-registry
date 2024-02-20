// ref: https://pratikpokhrel51.medium.com/creating-data-seeder-in-ef-core-that-reads-from-json-file-in-dot-net-core-69004df7ad0a

using Microsoft.CodeAnalysis;
using PackageRegistryService.Models;
using System.Security.Policy;
using System.Security.Cryptography;
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
            MD5 md5 = MD5.Create();

            if (!context.ValidationPackages.Any())
            {
                var index = DataInitializer.ReadIndex();

                context.SaveChanges();

                var validationPackages =
                    index
                        .Select((i) =>
                        {
                            var content = File.ReadAllBytes($"StagingArea/{i.Metadata.Name}/{i.FileName}");

                            return new ValidationPackage
                            {
                                Name = i.Metadata.Name,
                                Description = i.Metadata.Description,
                                MajorVersion = i.Metadata.MajorVersion,
                                MinorVersion = i.Metadata.MinorVersion,
                                PatchVersion = i.Metadata.PatchVersion,
                                PackageContent = content,
                                ReleaseDate = new(i.LastUpdated.Year, i.LastUpdated.Month, i.LastUpdated.Day),
                                Tags = i.Metadata.Tags,
                                ReleaseNotes = i.Metadata.ReleaseNotes,
                                Authors = i.Metadata.Authors,
                            };
                        });

                context.AddRange(validationPackages);

                var hashes = 
                    index
                        .Select((i) =>
                        {
                            var content = File.ReadAllBytes($"StagingArea/{i.Metadata.Name}/{i.FileName}");
                            return new PackageContentHash
                            {
                                PackageName = i.Metadata.Name,
                                PackageMajorVersion = i.Metadata.MajorVersion,
                                PackageMinorVersion = i.Metadata.MinorVersion,
                                PackagePatchVersion = i.Metadata.PatchVersion,
                                Hash = Convert.ToHexString(md5.ComputeHash(content)),
                            };
                        });

                context.AddRange(hashes);

                context.SaveChanges();
            }
        }
    }
}
