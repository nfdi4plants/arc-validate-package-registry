using PackageRegistryService.Models;
using PortableAuthor = global::ValidationPackage.Model.Author;
using PortableBinding = global::ValidationPackage.Model.CommandInputBinding;
using PortableCwlPrimitive = global::ValidationPackage.Model.CwlPrimitive;
using PortableInput = global::ValidationPackage.Model.CommandInputParameter;
using PortableInputType = global::ValidationPackage.Model.CommandInputType;
using PortableMetadata = global::ValidationPackage.Model.ValidationPackageMetadata;
using PortableOntologyAnnotation = global::ValidationPackage.Model.OntologyAnnotation;

namespace APITests;

public class ValidationPackageModelMappingTests
{
    [Fact]
    public void PortableMetadataRoundTripsThroughTheServiceModel()
    {
        var metadata = new PortableMetadata
        {
            Name = "test-package",
            Summary = "summary",
            Description = "description",
            MajorVersion = 1,
            MinorVersion = 2,
            PatchVersion = 3,
            PreReleaseVersionSuffix = "alpha.1",
            BuildMetadataVersionSuffix = "build.4",
            ProgrammingLanguage = "FSharp",
            Authors =
            [
                new PortableAuthor
                {
                    FullName = "Ada Lovelace",
                    Email = "ada@example.org",
                    Affiliation = "Analytical Engine Society",
                    AffiliationLink = "https://example.org"
                }
            ],
            Tags =
            [
                new PortableOntologyAnnotation
                {
                    Name = "validation",
                    TermSourceREF = "NCIT",
                    TermAccessionNumber = "NCIT:C16237"
                }
            ],
            ReleaseNotes = "Initial portable mapping.",
            CQCHookEndpoint = "https://example.org/cqc",
            Inputs =
            [
                new PortableInput
                {
                    Id = "verbose",
                    Type = new PortableInputType
                    {
                        PrimitiveType = PortableCwlPrimitive.Boolean,
                        IsNullable = true
                    },
                    Label = "Verbose output",
                    Doc = "Enable detailed output.",
                    InputBinding = new PortableBinding
                    {
                        Position = 2,
                        Prefix = "--verbose"
                    }
                }
            ]
        };
        var content = "printfn \"test\"\n"u8.ToArray();
        var releaseDate = new DateOnly(2026, 7, 30);

        var serviceModel = metadata.ToServiceModel(content, releaseDate);
        var actual = serviceModel.ToPortableModel();

        Assert.Equal(metadata, actual);
        Assert.Same(content, serviceModel.PackageContent);
        Assert.Equal(releaseDate, serviceModel.ReleaseDate);
    }

    [Fact]
    public void EmptyServiceCollectionsMapToEmptyPortableArrays()
    {
        var serviceModel = new PackageRegistryService.Models.ValidationPackage
        {
            Name = "minimal",
            Summary = "summary",
            Description = "description",
            MajorVersion = 1,
            MinorVersion = 0,
            PatchVersion = 0,
            PackageContent = [],
            ReleaseDate = new DateOnly(2026, 7, 30)
        };

        var actual = serviceModel.ToPortableModel();

        Assert.Empty(actual.Authors);
        Assert.Empty(actual.Tags);
        Assert.Empty(actual.Inputs);
    }

    [Fact]
    public void EveryPortableCwlPrimitiveMapsInBothDirections()
    {
        foreach (var primitive in Enum.GetValues<PortableCwlPrimitive>())
        {
            var portable = new PortableInputType
            {
                PrimitiveType = primitive,
                IsNullable = true
            };

            var actual = portable.ToServiceModel().ToPortableModel();

            Assert.Equal(portable, actual);
        }
    }
}
