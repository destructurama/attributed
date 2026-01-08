using System.Reflection;
using Destructurama.Attributed;

namespace Destructurama.Filters;

/// <summary>
/// TODO:
/// </summary>
public interface IPropertyFilterAttribute
{
    /// <summary>
    /// TODO:
    /// </summary>
    /// <param name="property"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    bool ShouldBeLogged(PropertyInfo property, AttributedDestructuringPolicyOptions options);
}
