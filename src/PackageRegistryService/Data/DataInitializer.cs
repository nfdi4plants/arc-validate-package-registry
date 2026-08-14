// ref: https://pratikpokhrel51.medium.com/creating-data-seeder-in-ef-core-that-reads-from-json-file-in-dot-net-core-69004df7ad0a

using AVPR.Staging;
using PackageRegistryService.Models;
using System.Reflection;

namespace PackageRegistryService.Data
    
{
    /// <summary>
    /// Populates a newly created registry database from validated staged packages.
    /// </summary>
    public class DataInitializer
    {
        /// <summary>
        /// Seeds packages, content hashes, and download counters when the registry is empty.
        /// </summary>
        /// <param name="context">The registry database context to initialize.</param>
        public static void SeedData(ValidationPackageDb context)
        {
            if (!context.ValidationPackages.Any())
            {
                var stagedPackages = StagingRepository.discover(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!);

                context.SaveChanges();

                var validationPackages =
                    stagedPackages.Select(stagedPackage => stagedPackage.ToServiceModel());

                context.AddRange(validationPackages);

                var hashes =
                    stagedPackages
                        .Select((i) =>
                        {
                            var hash = ContentHash.ofFile(i.RepoPath);

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
                                PackagePreReleaseVersionSuffix = i.Metadata.PreReleaseVersionSuffix,
                                PackageBuildMetadataVersionSuffix = i.Metadata.BuildMetadataVersionSuffix,
                                Hash = hash,
                            };
                        });

                context.AddRange(hashes);

                var downloads =
                     stagedPackages
                        .Select((i) =>
                        {
                            return new PackageDownloads
                            {
                                PackageName = i.Metadata.Name,
                                PackageMajorVersion = i.Metadata.MajorVersion,
                                PackageMinorVersion = i.Metadata.MinorVersion,
                                PackagePatchVersion = i.Metadata.PatchVersion,
                                PackagePreReleaseVersionSuffix = i.Metadata.PreReleaseVersionSuffix,
                                PackageBuildMetadataVersionSuffix = i.Metadata.BuildMetadataVersionSuffix,
                                Downloads = 0
                            };
                        });

                context.AddRange(downloads);

                context.SaveChanges();
            }
        }
    }
}
