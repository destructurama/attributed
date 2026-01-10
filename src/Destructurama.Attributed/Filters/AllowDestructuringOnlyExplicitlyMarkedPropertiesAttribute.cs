using System.Reflection;
using Destructurama.Attributed;

namespace Destructurama.Filters;

/// <summary>
/// This attribute allows a property to be considered for destructuring only if it is marked
/// with one of Destructurama attributes - <see cref="IPropertyDestructuringAttribute"/> or
/// <see cref="IPropertyOptionalIgnoreAttribute"/>.
/// </summary>
/// <see href="https://github.com/destructurama/attributed/issues/171" />
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class AllowDestructuringOnlyExplicitlyMarkedPropertiesAttribute : Attribute, IPropertyFilterAttribute
{
    /// <inheritdoc />
    public bool AllowDestructuring(PropertyInfo property, AttributedDestructuringPolicyOptions options)
    {
        return property.GetCustomAttributesEx(options.RespectMetadataTypeAttribute).Any(attr => attr is IPropertyDestructuringAttribute || attr is IPropertyOptionalIgnoreAttribute);
    }
}
