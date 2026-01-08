using System.Reflection;
using Destructurama.Attributed;

namespace Destructurama.Filters;

/// <summary>
/// TODO:
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class OptInAttribute : Attribute, IPropertyFilterAttribute
{
    /// <summary>
    /// TODO:
    /// </summary>
    /// <param name="property"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public bool ShouldBeLogged(PropertyInfo property, AttributedDestructuringPolicyOptions options)
    {
        return property.GetCustomAttributesEx(options.RespectMetadataTypeAttribute).OfType<IPropertyDestructuringAttribute>().Any();
    }
}
