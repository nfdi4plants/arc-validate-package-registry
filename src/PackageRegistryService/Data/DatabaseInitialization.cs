using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using PackageRegistryService.Models;

namespace PackageRegistryService.Data;

/// <summary>
/// Encapsulates startup decisions for development database initialization.
/// </summary>
public static class DatabaseInitialization
{
    private const string DevelopmentBuildChannel = "dev";

    /// <summary>
    /// Determines whether a build channel identifies the deployed development service.
    /// </summary>
    /// <param name="buildChannel">The configured build channel.</param>
    /// <returns><see langword="true"/> only for the development deployment channel.</returns>
    public static bool IsDeployedDevelopmentInstance(string? buildChannel) =>
        string.Equals(
            buildChannel,
            DevelopmentBuildChannel,
            StringComparison.Ordinal
        );

    /// <summary>
    /// Determines whether the deployed development service should create and seed its database.
    /// </summary>
    /// <param name="buildChannel">The configured build channel.</param>
    /// <param name="hasSchema">Whether the target database already has a schema.</param>
    /// <returns><see langword="true"/> when an empty development database should be initialized.</returns>
    public static bool ShouldInitializeDeployedDatabase(
        string? buildChannel,
        bool hasSchema
    ) => IsDeployedDevelopmentInstance(buildChannel) && !hasSchema;

    /// <summary>
    /// Checks whether the relational database contains any schema tables.
    /// </summary>
    /// <param name="context">The registry database context.</param>
    /// <returns><see langword="true"/> when tables already exist.</returns>
    public static bool HasSchema(ValidationPackageDb context) =>
        context.Database.GetService<IRelationalDatabaseCreator>().HasTables();
}
