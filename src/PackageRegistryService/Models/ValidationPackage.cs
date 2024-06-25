using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Security.Cryptography;
using AVPRIndex;
using static AVPRIndex.Domain;

namespace PackageRegistryService.Models
{
    /// <summary>
    /// 
    /// </summary>
    [PrimaryKey(nameof(Name), nameof(MajorVersion), nameof(MinorVersion), nameof(PatchVersion), nameof(PreReleaseVersionSuffix), nameof(BuildMetadataVersionSuffix))]
    public class ValidationPackage
    {
        /// <summary>
        /// The name of the validation package.
        /// </summary>
        /// <example>MyPackage</example>
        public required string Name { get; set; }

        /// <summary>
        /// Single sentence validation package description.
        /// </summary>
        /// <example>MyPackage does the thing</example>
        public required string Summary { get; set; }

        /// <summary>
        /// Free text validation package description.
        /// </summary>
        /// <example>
        /// MyPackage does the thing.
        /// It does it very good, it does it very well.
        /// It does it very fast, it does it very swell.
        /// </example>
        public required string Description { get; set; }

        /// <summary>
        /// SemVer major version of the validation package.
        /// </summary>
        /// <example>1</example>
        public required int MajorVersion { get; set; }

        /// <summary>
        /// SemVer minor version of the validation package.
        /// </summary>
        /// <example>0</example>
        public required int MinorVersion { get; set; }

        /// <summary>
        /// SemVer patch version of the validationpackage.
        /// </summary>
        /// <example>0</example>
        public required int PatchVersion { get; set; }

        /// <summary>
        /// SemVer prerelease version of the validationpackage.
        /// </summary>
        /// <example>alpha.1</example>
        public string PreReleaseVersionSuffix { get; set; } = "";

        /// <summary>
        /// SemVer buildmetadata of the validationpackage.
        /// </summary>
        /// <example>0</example>
        public string BuildMetadataVersionSuffix { get; set; } = "";

        /// <summary>
        /// base64 encoded binary content of the validation package.
        /// </summary>
        /// <example>aHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1kUXc0dzlXZ1hjUQ==</example>
        public required byte[] PackageContent { get; set; }

        /// <summary>
        ///
        /// </summary>
        public required DateOnly ReleaseDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ICollection<AVPRIndex.Domain.OntologyAnnotation> Tags { get; set; } = Array.Empty<AVPRIndex.Domain.OntologyAnnotation>().ToList();

        /// <summary>
        /// 
        /// </summary>
        public string ReleaseNotes { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        public string CQCHookEndpoint { get; set; } = "";

        /// <summary>
        /// 
        /// </summary>
        public ICollection<AVPRIndex.Domain.Author> Authors { get; set; } = Array.Empty<AVPRIndex.Domain.Author>().ToList();// https://www.learnentityframeworkcore.com/relationships#navigation-properties

        /// <summary>
        /// 
        /// </summary>
        /// <returns>A string containing the semantic version of the validation package</returns>
        public string GetSemanticVersionString()
        {
            SemVer semVer = new SemVer {
                Major = MajorVersion,
                Minor = MinorVersion,
                Patch = PatchVersion,
                PreRelease = PreReleaseVersionSuffix,
                BuildMetadata = BuildMetadataVersionSuffix
            };
            return SemVer.toString(semVer);
        }

        /// <summary>
        /// Converts the binary content of the validation package to a string by converting it to base64 and then decoding it as UTF8.
        /// </summary>
        /// <returns>A string containing the package content</returns>
        public string GetPackageScriptContent() => Encoding.UTF8.GetString(Convert.FromBase64String(Convert.ToBase64String(PackageContent)));
        
        /// <summary>
        /// Returns the md5 hash of the package content.
        /// </summary>
        /// <returns>A string containing the package content</returns>
        public string GetPackageContentHash()
        {
            return AVPRIndex.Hash.hashContent(PackageContent);
        }
        
        /// <summary>
        /// Returns whether the package content CR characters - meaning its is has not been unified to only use LF.
        /// </summary>
        /// <returns>true or false</returns>
        public bool ContentContainsCarriageReturn()
        {
            return GetPackageScriptContent().Contains("\r");
        }
    }
}
