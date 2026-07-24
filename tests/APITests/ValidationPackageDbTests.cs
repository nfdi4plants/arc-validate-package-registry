using Microsoft.EntityFrameworkCore;
using PackageRegistryService.Models;
using System.Text.Json;
using static AVPRIndex.Domain;

namespace APITests;

public class ValidationPackageDbTests
{
    public static TheoryData<CwlPrimitive, string> PrimitiveStorageValues => new()
    {
        { CwlPrimitive.Boolean, "boolean" },
        { CwlPrimitive.Int, "int" },
        { CwlPrimitive.Long, "long" },
        { CwlPrimitive.Float, "float" },
        { CwlPrimitive.Double, "double" },
        { CwlPrimitive.String, "string" }
    };

    [Theory]
    [MemberData(nameof(PrimitiveStorageValues))]
    public void CwlPrimitiveStorageConverterRoundTripsLowercaseValues(
        CwlPrimitive primitive,
        string storedValue)
    {
        Assert.Equal(storedValue, CwlPrimitiveStorageConverter.ToStorageValue(primitive));
        Assert.Equal(primitive, CwlPrimitiveStorageConverter.FromStorageValue(storedValue));
    }

    [Fact]
    public void CwlPrimitiveStorageConverterRejectsUnsupportedValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CwlPrimitiveStorageConverter.ToStorageValue((CwlPrimitive)999));
        Assert.Throws<ArgumentException>(
            () => CwlPrimitiveStorageConverter.FromStorageValue("Boolean"));
    }

    [Fact]
    public void InputsUseTheExpectedOwnedJsonShape()
    {
        var options = new DbContextOptionsBuilder<ValidationPackageDb>()
            .UseNpgsql("Host=localhost;Database=avpr_model_test;Username=test;Password=test")
            .Options;

        using var database = new ValidationPackageDb(options);

        var packageEntity = database.Model.FindEntityType(typeof(ValidationPackage));
        Assert.NotNull(packageEntity);

        var inputsNavigation = packageEntity.FindNavigation(nameof(ValidationPackage.Inputs));
        Assert.NotNull(inputsNavigation);

        var inputEntity = inputsNavigation.TargetEntityType;
        Assert.True(inputEntity.IsMappedToJson());
        Assert.Equal("Inputs", inputEntity.GetContainerColumnName());
        Assert.Equal("ValidationPackages", inputEntity.GetTableName());
        Assert.Contains(inputEntity.FindPrimaryKey()!.Properties, property => property.Name == "__ordinal");
        Assert.DoesNotContain(
            inputEntity.FindPrimaryKey()!.Properties,
            property => property.Name == nameof(CommandInputParameter.Id));
        Assert.Equal("id", inputEntity.FindProperty(nameof(CommandInputParameter.Id))?.GetJsonPropertyName());
        Assert.Equal("label", inputEntity.FindProperty(nameof(CommandInputParameter.Label))?.GetJsonPropertyName());
        Assert.Equal("doc", inputEntity.FindProperty(nameof(CommandInputParameter.Doc))?.GetJsonPropertyName());

        var typeNavigation = inputEntity.FindNavigation(nameof(CommandInputParameter.Type));
        Assert.NotNull(typeNavigation);
        Assert.True(typeNavigation.ForeignKey.IsRequiredDependent);
        Assert.Equal("type", typeNavigation.TargetEntityType.GetJsonPropertyName());

        var typeEntity = typeNavigation.TargetEntityType;
        var primitiveProperty = typeEntity.FindProperty(nameof(CommandInputType.PrimitiveType));
        Assert.NotNull(primitiveProperty);
        Assert.Equal("primitiveType", primitiveProperty.GetJsonPropertyName());
        Assert.Equal(typeof(string), primitiveProperty.GetTypeMapping().Converter?.ProviderClrType);
        Assert.Equal(
            "isNullable",
            typeEntity.FindProperty(nameof(CommandInputType.IsNullable))?.GetJsonPropertyName());

        var bindingNavigation = inputEntity.FindNavigation(nameof(CommandInputParameter.InputBinding));
        Assert.NotNull(bindingNavigation);
        Assert.True(bindingNavigation.ForeignKey.IsRequiredDependent);
        Assert.Equal("inputBinding", bindingNavigation.TargetEntityType.GetJsonPropertyName());

        var bindingEntity = bindingNavigation.TargetEntityType;
        Assert.Equal(
            "position",
            bindingEntity.FindProperty(nameof(CommandInputBinding.Position))?.GetJsonPropertyName());
        Assert.Equal(
            "prefix",
            bindingEntity.FindProperty(nameof(CommandInputBinding.Prefix))?.GetJsonPropertyName());
        Assert.Equal(
            "separate",
            bindingEntity.FindProperty(nameof(CommandInputBinding.Separate))?.GetJsonPropertyName());
    }

    [Fact]
    public void ServiceJsonUsesTheCwlScalarTypeContract()
    {
        var package = new ValidationPackage
        {
            Name = "test",
            Summary = "summary",
            Description = "description",
            MajorVersion = 1,
            MinorVersion = 0,
            PatchVersion = 0,
            PackageContent = [],
            ReleaseDate = new DateOnly(2026, 7, 24),
            Inputs =
            [
                new CommandInputParameter
                {
                    Id = "verbose",
                    Type = new CommandInputType
                    {
                        PrimitiveType = CwlPrimitive.Boolean,
                        IsNullable = true
                    },
                    InputBinding = new CommandInputBinding
                    {
                        Prefix = "--verbose"
                    }
                }
            ]
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = null };
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(package, options));

        var root = document.RootElement;
        var input = root.GetProperty("Inputs")[0];
        Assert.Equal("verbose", input.GetProperty("id").GetString());
        Assert.Equal("boolean?", input.GetProperty("type").GetString());
        Assert.Equal("--verbose", input.GetProperty("inputBinding").GetProperty("prefix").GetString());
        Assert.False(root.TryGetProperty("inputs", out _));
        Assert.False(input.TryGetProperty("Type", out _));
        Assert.False(input.TryGetProperty("primitiveType", out _));
    }
}
