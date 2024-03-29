﻿using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Security.Cryptography;


namespace PackageRegistryService.Models
{
    /// <summary>
    /// 
    /// </summary>
    [PrimaryKey(nameof(Name), nameof(MajorVersion), nameof(MinorVersion), nameof(PatchVersion))]
    public class ValidationPackage
    {
        /// <summary>
        /// The name of the validation package.
        /// </summary>
        /// <example>MyPackage</example>
        [Key]
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
        [Key]
        public required int MajorVersion { get; set; }
        /// <summary>
        /// SemVer minor version of the validation package.
        /// </summary>
        /// <example>0</example>
        [Key]
        public required int MinorVersion { get; set; }
        /// <summary>
        /// SemVer patch version of the validationpackage.
        /// </summary>
        /// <example>0</example>
        
        public required int PatchVersion { get; set; }
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
        public ICollection<AVPRIndex.Domain.OntologyAnnotation>? Tags { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? ReleaseNotes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ICollection<AVPRIndex.Domain.Author>? Authors { get; set; } // https://www.learnentityframeworkcore.com/relationships#navigation-properties
        /// <summary>
        /// 
        /// </summary>
        /// <returns>A string containing the semantic version of the validation package</returns>
        public string GetSemanticVersionString() => $"{MajorVersion}.{MinorVersion}.{PatchVersion}";
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
            using (var md5 = MD5.Create())
            {
                return Convert.ToHexString(md5.ComputeHash(PackageContent));
            }
        }
    }
}
