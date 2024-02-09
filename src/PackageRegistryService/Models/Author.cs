using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace PackageRegistryService.Models
{
    public class Author
    {
        public required string FullName { get; set; }
        public string Email { get; set; }
        public string Affiliation { get; set; }
        public string AffiliationLink { get; set; }
    }
}
