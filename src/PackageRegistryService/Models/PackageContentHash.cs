using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PackageRegistryService.Models
{
    [PrimaryKey(nameof(PackageName), nameof(PackageMajorVersion), nameof(PackageMinorVersion), nameof(PackagePatchVersion))]
    public class PackageContentHash
    {
        /// <summary>
        /// The name of the validation package. This is the unique identifier for the validation package, and will be used to retrieve the validation package.
        /// </summary>
        /// <example>MyPackage</example>
        [Key]
        public required string PackageName { get; set; }
        /// <summary>
        /// SemVer major version of the validation package.
        /// </summary>
        /// <example>1</example>
        [Key]
        public required int PackageMajorVersion { get; set; }
        /// <summary>
        /// SemVer minor version of the validation package.
        /// </summary>
        /// <example>0</example>
        [Key]
        public required int PackageMinorVersion { get; set; }
        /// <summary>
        /// SemVer patch version of the validationpackage.
        /// </summary>
        /// <example>0</example>

        public required int PackagePatchVersion { get; set; }
        /// <summary>
        /// MD5 hash hex string of the package content.
        /// </summary>
        /// <example>ACEA630D76D9AE406994641914A4488E</example>
        public required string Hash { get; set; }
    }
}
