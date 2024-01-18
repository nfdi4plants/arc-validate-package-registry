using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Text;


namespace PackageRegistryService.Models
{
    [PrimaryKey(nameof(Name), nameof(MajorVersion), nameof(MinorVersion), nameof(RevisionVersion))]
    public class ValidationPackage
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required int MajorVersion { get; set; }
        public required int MinorVersion { get; set; }
        public required int RevisionVersion { get; set; }
        public required byte[] PackageContent { get; set; }

        public string GetSemanticVersionString() => $"{MajorVersion}.{MinorVersion}.{RevisionVersion}";

        public string GetPackageScriptContent() => Encoding.UTF8.GetString(Convert.FromBase64String(Convert.ToBase64String(PackageContent)));

    }
}
