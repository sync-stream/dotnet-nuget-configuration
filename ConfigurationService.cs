using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using SyncStream.Serializer;

// Define our namespace
namespace SyncStream.Configuration;

/// <summary>
///     This class maintains the structure of our configuration service
/// </summary>
public class ConfigurationService
{
    /// <summary>
    ///     This property contains our configuration construct
    /// </summary>
    public static IConfiguration Configuration { get; private set; }

    /// <summary>
    ///     This property contains the environment variable prefix to use
    /// </summary>
    public static string EnvironmentPrefix { get; set; } = "SS_";

    /// <summary>
    ///     This method configures the service with a new <paramref name="configuration" /> instance
    /// </summary>
    /// <param name="configuration">The configuration instance to use</param>
    public static void Configure(IConfiguration configuration) => Configuration = configuration;

    /// <summary>
    ///     This method reads a <paramref name="file" /> into a configuration instance
    /// </summary>
    /// <param name="file">The file to populate the configuration instance with</param>
    public static IConfiguration Configure(string file) => Configure(new[] { file });

    /// <summary>
    ///     This method reads <paramref name="files" /> into the <paramref name="configurationBuilder" />
    ///     then sets the build configuration instance into the service
    /// </summary>
    /// <param name="files">The files to read into the service's configuration instance</param>
    /// <param name="configurationBuilder">Optional, existing builder to use</param>
    /// <returns>The built configuration the service is now using</returns>
    public static IConfiguration Configure(IEnumerable<string> files, ConfigurationBuilder configurationBuilder = null)
    {
        // Ensure we have a configuration builder
        if (configurationBuilder is null)
        {
            // Define our configuration builder
            configurationBuilder = new();

            // Add the environment to the configuration builder
            configurationBuilder.AddEnvironmentVariables(source =>
            {
                // We'll need a prefix
                source.Prefix = EnvironmentPrefix;
            });
        }

        // Iterate over the files
        foreach (string file in files)
        {
            // Check for XML and add the XML file
            if (file.ToLower().Trim().EndsWith(".xml"))
                configurationBuilder.AddXmlFile(source =>
                {
                    // This is not an optional file
                    source.Optional = false;

                    // Set the path to the file we need to read
                    source.Path = file.Trim();

                    // Reload the values when the file changes
                    source.ReloadOnChange = true;
                });

            // Check for JSON and add the JSON file
            if (file.ToLower().Trim().EndsWith(".json"))
                configurationBuilder.AddJsonFile(source =>
                {
                    // This is not an optional file
                    source.Optional = false;

                    // Set the path to the file we need to read
                    source.Path = file.Trim();

                    // Reload the values when the file changes
                    source.ReloadOnChange = true;
                });
        }

        // We're done, return the configuration
        return configurationBuilder.Build();
    }

    /// <summary>
    ///     This method reads <paramref name="files" /> into the service's configuration instance
    /// </summary>
    /// <param name="files">The files to read into the configuration</param>
    /// <returns>The built configuration the service is now using</returns>
    public static IConfiguration Configure(params string[] files) => Configure(files.ToList());

    /// <summary>
    ///     This method returns the <paramref name="environmentVariableName" /> value as a string
    /// </summary>
    /// <param name="environmentVariableName">The name of the environment variable to return</param>
    /// <returns>The string value of <paramref name="environmentVariableName" /></returns>
    public static string GetEnvironmentVariableValue(string environmentVariableName) => ReplaceVariableReferences(
        ReplaceEnvironmentVariableReferences(Environment.GetEnvironmentVariable(
            NormalizeVariableName(!environmentVariableName.StartsWith(EnvironmentPrefix)
                ? $"{EnvironmentPrefix}{environmentVariableName}"
                : environmentVariableName))));

    /// <summary>
    ///     This method returns the <typeparamref name="TValue" /> typed value
    ///     of <paramref name="environmentVariableName" /> from the environment
    /// </summary>
    /// <param name="environmentVariableName">The variable name, to pull the value from the environment</param>
    /// <param name="format">Optional, serialization format used</param>
    /// <typeparam name="TValue">The expected type of the resulting object</typeparam>
    /// <returns><typeparamref name="TValue" /> typed value of <paramref name="environmentVariableName" /> from the environment</returns>
    public static TValue GetEnvironmentVariableValue<TValue>(string environmentVariableName,
        SerializerFormat format = SerializerFormat.None) =>
        GetTypedValue<TValue>(GetEnvironmentVariableValue(environmentVariableName));

    /// <summary>
    ///     This method converts the <paramref name="source" /> value string to it's <typeparamref name="TValue" /> type
    /// </summary>
    /// <param name="source">The value string to type</param>
    /// <param name="format">Optional, serialization format used</param>
    /// <typeparam name="TValue">The expected resulting value</typeparam>
    /// <returns><typeparamref name="TValue" /> typed object of <paramref name="source" /></returns>
    public static TValue GetTypedValue<TValue>(string source, SerializerFormat format = SerializerFormat.None)
    {
        // Try to deserialize the value
        try
        {
            // We're done deserialize and return the value
            return SerializerService.Deserialize<TValue>(source, format);
        }
        catch (Exception)
        {
            // We're done, change the type of the value and return it
            return (TValue)Convert.ChangeType(source, typeof(TValue));
        }
    }

    /// <summary>
    ///     This method returns the string value of <paramref name="variableName" />
    /// </summary>
    /// <param name="variableName">The name of the configuration key for which to return the value</param>
    /// <returns>The string value of <paramref name="variableName" /></returns>
    public static string GetValue(string variableName) =>
        ReplaceVariableReferences(
            ReplaceEnvironmentVariableReferences(Configuration[NormalizeVariableName(variableName)]));

    /// <summary>
    ///     This method returns the <typeparamref name="TValue" /> typed value
    ///     of <paramref name="variableName" /> from the service's configuration
    /// </summary>
    /// <param name="variableName">The variable name, to pull the value from the service's configuration</param>
    /// <param name="format">Optional, serialization format used</param>
    /// <typeparam name="TValue">The expected type of the resulting object</typeparam>
    /// <returns><typeparamref name="TValue" /> typed value of <paramref name="variableName" /> from the service's configuration</returns>
    public static TValue GetValue<TValue>(string variableName, SerializerFormat format = SerializerFormat.None) =>
        GetTypedValue<TValue>(GetValue(variableName), format);

    /// <summary>
    ///     This method normalizes <paramref name="variableName" />
    ///     and replaces any environment or variable references
    /// </summary>
    /// <param name="variableName">The environment variable name to normalize</param>
    /// <returns>The normalized <paramref name="variableName" /></returns>
    public static string NormalizeVariableName(string variableName) => ReplaceVariableReferences(
        ReplaceEnvironmentVariableReferences(Regex.Replace(variableName, @"\/|::|-\>", "_",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline)));

    /// <summary>
    ///     This method replaces environment variable references in <paramref name="source" />
    /// </summary>
    /// <param name="source">The key string to make replacements in</param>
    /// <returns>The <paramref name="source" /> with environment variable reference value replacements</returns>
    public static string ReplaceEnvironmentVariableReferences(string source)
    {
        // Localize the environment variable reference regular expression
        Regex regex = new Regex(@"\$\{env:(.*)\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // Iterate over the environment variable reference matches and replace the reference
        foreach (Match match in regex.Matches(source))
            source = source.Replace(match.Groups[0].Value, GetEnvironmentVariableValue(match.Groups[1].Value));

        // We're done, return the finalized source
        return source.Trim();
    }

    /// <summary>
    ///     This method replaces variable references with their values in <paramref name="source" />
    /// </summary>
    /// <param name="source">The string to make the replacements in</param>
    /// <returns>The <paramref name="source" /> with variable reference value replacements</returns>
    public static string ReplaceVariableReferences(string source)
    {
        // Localize the variable reference regular expression
        Regex regex = new Regex(@"\$\{(.*)\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // Iterate over the variable reference matches and replace the reference
        foreach (Match match in regex.Matches(source))
            source = source.Replace(match.Groups[0].Value, GetValue(match.Groups[1].Value));

        // We're done, return the finalized source
        return source.Trim();
    }
}
