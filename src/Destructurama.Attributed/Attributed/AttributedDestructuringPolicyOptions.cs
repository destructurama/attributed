using Serilog.Events;

namespace Destructurama.Attributed;

/// <summary>
/// Global destructuring options.
/// </summary>
public class AttributedDestructuringPolicyOptions
{
    /// <summary>
    /// By setting this property to <see langword="true"/> no need to set <see cref="NotLoggedIfNullAttribute"/>
    /// for every logged property. Custom types implementing IEnumerable, will be destructed as <see cref="StructureValue"/>
    /// and affected by this property only in case at least one property (or the type itself) has Destructurama attribute applied.
    /// </summary>
    public bool IgnoreNullProperties { get; set; }

    /// <summary>
    /// Respect Microsoft.Extensions.Logging.LogPropertyIgnoreAttribute to not include property when destructuring an object for logging.
    /// This works the same as when applying <see cref="NotLoggedAttribute"/> to the property but may help if you have no access to it's source code.
    /// </summary>
    public bool RespectLogPropertyIgnoreAttribute { get; set; }
}
