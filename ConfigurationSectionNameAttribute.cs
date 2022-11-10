// Define our namespace
namespace SyncStream.Configuration;

/// <summary>
///     This class maintains the attribute structure for our Configuration Section Name
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
public class ConfigurationSectionNameAttribute : Attribute
{
    /// <summary>
    ///     This property contains the configuration section name the class represents
    /// </summary>
    public readonly string SectionName;

    /// <summary>
    ///     This method instantiates the attribute with a <paramref name="sectionName" />
    /// </summary>
    /// <param name="sectionName">The name of the section in the configuration that represents the class</param>
    public ConfigurationSectionNameAttribute(string sectionName) => SectionName = sectionName;
}
