using Microsoft.Extensions.DependencyInjection;

// Define our namespace
namespace SyncStream.Configuration;

/// <summary>
///     This class maintains our package's IServiceCollection extensions
/// </summary>
public static class SyncStreamConfigurationServiceCollectionExtensions
{
    /// <summary>
    ///     This method adds <paramref name="configurationFiles" /> into the application's configuration profile
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="configurationFiles">The configuration files to add</param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseSyncStreamConfiguration(this IServiceCollection instance,
        IEnumerable<string> configurationFiles)
    {
        // Configure the library
        ConfigurationService.Configure(configurationFiles);

        // We're done, return the IServiceCollection instance
        return instance;
    }

    /// <summary>
    ///     This method adds <paramref name="configurationFiles" /> into the application's configuration profile
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="configurationFiles">The configuration files to add</param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseSyncStreamConfiguration(this IServiceCollection instance,
        params string[] configurationFiles) => UseSyncStreamConfiguration(instance, configurationFiles.ToList());

    /// <summary>
    ///     This method adds <paramref name="filePath" /> to the application's configuration profile
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="filePath">The configuration file to add</param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseSyncStreamConfiguration(this IServiceCollection instance, string filePath) =>
        UseSyncStreamConfiguration(instance, new[] { filePath });
}
