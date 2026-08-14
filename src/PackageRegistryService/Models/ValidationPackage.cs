using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using AVPR.Staging;
using System.Text;
using PortableSemVer = global::ValidationPackage.Model.SemVer;

namespace PackageRegistryService.Models
{
    /// <summary>
    /// Represents one published validation-package version as persisted by the registry service.
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
        /// The date on which this package version was released.
        /// </summary>
        public required DateOnly ReleaseDate { get; set; }

        /// <summary>
        /// Ontology annotations used to categorize the package.
        /// </summary>
        public ICollection<OntologyAnnotation> Tags { get; set; } = [];

        /// <summary>
        /// Release notes for this package version.
        /// </summary>
        public string ReleaseNotes { get; set; } = "";

        /// <summary>
        /// The optional endpoint used for continuous-quality-control integration.
        /// </summary>
        public string CQCHookEndpoint { get; set; } = "";

        /// <summary>
        /// The package authors.
        /// </summary>
        public ICollection<Author> Authors { get; set; } = [];

        /// <summary>
        /// The language used by the executable validation package.
        /// </summary>
        public string ProgrammingLanguage { get; set; } = "";

        /// <summary>
        /// The CWL command inputs this validation package accepts.
        /// </summary>
        public ICollection<CommandInputParameter> Inputs { get; set; } = [];

        /// <summary>
        /// Formats the package's complete semantic version.
        /// </summary>
        /// <returns>The canonical semantic version of the validation package.</returns>
        public string GetSemanticVersionString()
        {
            PortableSemVer semVer = new PortableSemVer {
                Major = MajorVersion,
                Minor = MinorVersion,
                Patch = PatchVersion,
                PreRelease = PreReleaseVersionSuffix,
                BuildMetadata = BuildMetadataVersionSuffix
            };
            return PortableSemVer.toString(semVer);
        }

        /// <summary>
        /// Decodes the package content as UTF-8 text.
        /// </summary>
        /// <returns>The executable package source.</returns>
        public string GetPackageScriptContent() => Encoding.UTF8.GetString(Convert.FromBase64String(Convert.ToBase64String(PackageContent)));
        
        /// <summary>
        /// Computes the normalized content fingerprint of the package source.
        /// </summary>
        /// <returns>The package content fingerprint.</returns>
        public string GetPackageContentHash()
        {
            return ContentHash.ofBytes(PackageContent);
        }
        
        /// <summary>
        /// Determines whether package source still contains carriage-return characters.
        /// </summary>
        /// <returns><see langword="true"/> when the package content is not normalized to LF-only line endings.</returns>
        public bool ContentContainsCarriageReturn()
        {
            return GetPackageScriptContent().Contains("\r");
        }
    }
}
