namespace PackageRegistryService.Models
{
    public class ValidationPackagemetadata
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public int PatchVersion { get; set; }
    }
    public class ValidationPackageIndex
    {
        public string RepoPath { get; set; }
        public string FileName { get; set; }
        public System.DateTimeOffset LastUpdated { get; set; }
        public ValidationPackagemetadata Metadata { get; set; }
    }
}
