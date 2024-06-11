// ref: https://pratikpokhrel51.medium.com/creating-data-seeder-in-ef-core-that-reads-from-json-file-in-dot-net-core-69004df7ad0a

using Microsoft.CodeAnalysis;
using PackageRegistryService.Models;
using System.Security.Policy;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AVPRIndex;
using static AVPRIndex.Domain;
using static AVPRIndex.Frontmatter;
using System.Reflection;

namespace PackageRegistryService.Data
    
{
    public class DataInitializer
    {
        public static void SeedData(ValidationPackageDb context)
        {
            MD5 md5 = MD5.Create();

            if (!context.ValidationPackages.Any())
            {
                var index = AVPRRepo.getPreviewIndex();

                context.SaveChanges();

                var validationPackages =
                    index
                        .Select((i) =>
                        {
                            var path = 
                                Path.Combine(
                                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                    $"StagingArea/{i.Metadata.Name}/{i.FileName}"
                                );
                            var content = 
                                File.ReadAllText(path)
                                .ReplaceLineEndings("\n");

                            return new ValidationPackage
                            {
                                Name = i.Metadata.Name,
                                Summary = i.Metadata.Summary,
                                Description = i.Metadata.Description,
                                MajorVersion = i.Metadata.MajorVersion,
                                MinorVersion = i.Metadata.MinorVersion,
                                PatchVersion = i.Metadata.PatchVersion,
                                PackageContent = Encoding.UTF8.GetBytes(content),
                                ReleaseDate = new(i.LastUpdated.Year, i.LastUpdated.Month, i.LastUpdated.Day),
                                Tags = i.Metadata.Tags,
                                ReleaseNotes = i.Metadata.ReleaseNotes,
                                Authors = i.Metadata.Authors,
                                CQCHookEndpoint = i.Metadata.CQCHookEndpoint
                            };
                        });

                context.AddRange(validationPackages);

                var hashes =
                    index
                        .Select((i) =>
                        {
                            var path =
                                Path.Combine(
                                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                    $"StagingArea/{i.Metadata.Name}/{i.FileName}"
                                );
                            var content =
                                File.ReadAllText(path)
                                .ReplaceLineEndings("\n");

                            var hash = Convert.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(content)));
                            if (hash != i.ContentHash)
                            {
                                throw new Exception($"Hash collision for indexed hash vs content hash: {$"StagingArea/{i.Metadata.Name}/{i.FileName}"}");
                            }
                            return new PackageContentHash
                            {
                                PackageName = i.Metadata.Name,
                                PackageMajorVersion = i.Metadata.MajorVersion,
                                PackageMinorVersion = i.Metadata.MinorVersion,
                                PackagePatchVersion = i.Metadata.PatchVersion,
                                Hash = hash,
                            };
                        });

                context.AddRange(hashes);

                var downloads =
                     index
                        .Select((i) =>
                        {
                            return new PackageDownloads
                            {
                                PackageName = i.Metadata.Name,
                                PackageMajorVersion = i.Metadata.MajorVersion,
                                PackageMinorVersion = i.Metadata.MinorVersion,
                                PackagePatchVersion = i.Metadata.PatchVersion,
                                Downloads = 0
                            };
                        });

                context.AddRange(downloads);

                context.SaveChanges();
            }
        }
    }
}
