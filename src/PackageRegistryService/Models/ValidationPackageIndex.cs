namespace PackageRegistryService.Models
{
    public class ValidationPackageMetadata
    {
        // mandatory fields
        public string Name { get; set; }
        public string Description { get; set; }
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public int PatchVersion { get; set; }

        //optional fields
        public bool Publish { get; set; }
        public Author[] Authors { get; set; }
        public string[] Tags { get; set; }
        public string ReleaseNotes { get; set; }
    }
    public class ValidationPackageIndex
    {
        public string RepoPath { get; set; }
        public string FileName { get; set; }
        public System.DateTimeOffset LastUpdated { get; set; }
        public ValidationPackageMetadata Metadata { get; set; }
    }
}
