using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace PackageRegistryService.Models
{
    [PrimaryKey(nameof(PackageName), nameof(PackageMajorVersion), nameof(PackageMinorVersion), nameof(PackagePatchVersion), nameof(PackagePreReleaseVersionSuffix), nameof(PackageBuildMetadataVersionSuffix))]
    public class PackageDownloads
    {
        /// <summary>
        /// The name of the validation package.
        /// </summary>
        /// <example>MyPackage</example>
        public required string PackageName { get; set; }

        /// <summary>
        /// SemVer major version of the validation package.
        /// </summary>
        /// <example>1</example>
        public required int PackageMajorVersion { get; set; }

        /// <summary>
        /// SemVer minor version of the validation package.
        /// </summary>
        /// <example>0</example>
        public required int PackageMinorVersion { get; set; }

        /// <summary>
        /// SemVer patch version of the validationpackage.
        /// </summary>
        /// <example>0</example>

        public required int PackagePatchVersion { get; set; }

        /// <summary>
        /// SemVer prerelease version of the validationpackage.
        /// </summary>
        /// <example>alpha.1</example>
        public required string PackagePreReleaseVersionSuffix { get; set; }

        /// <summary>
        /// SemVer buildmetadata of the validationpackage.
        /// </summary>
        /// <example>0</example>
        public required string PackageBuildMetadataVersionSuffix { get; set; }

        /// <summary>
        /// Number of downloads for the package.
        /// </summary>
        /// <example>420691337</example>
        public required int Downloads { get; set; }
    }
}
