// ref: https://pratikpokhrel51.medium.com/creating-data-seeder-in-ef-core-that-reads-from-json-file-in-dot-net-core-69004df7ad0a

using PackageRegistryService.Models;
using System.Text.Json;
namespace PackageRegistryService

{
    public class DataInitializer
    {
        public static void SeedData(ValidationPackageDb context)
        {
            if (!context.ValidationPackages.Any())
            {
                var json = File.ReadAllText("Data/ValidationPackages.json");
                var validationPackages = JsonSerializer.Deserialize<List<ValidationPackage>>(json);
                context.AddRange(validationPackages);
                context.SaveChanges();
            }
        }
    }
}
