using System.Reflection;
using Destructurama.Attributed;

namespace Destructurama.Filters;

/// <summary>
/// Base interface for all Destructurama attributes that determine should a property be
/// considered for destructuring. By default all properties that do not have any
/// <see cref="IPropertyDestructuringAttribute"/> attributes are just passed as is to
/// the underlying serilog pipeline. By defining attributes derived from that interface
/// and marking your classes you may filter out those properties that you do not want to
/// be passed (logged).
/// </summary>
public interface IPropertyFilterAttribute
{
    /// <summary>
    /// Allow or disallow the specified property to be destructured.
    /// </summary>
    /// <param name="property"><see cref="PropertyInfo"/></param>
    /// <param name="options"><see cref="AttributedDestructuringPolicyOptions"/></param>
    /// <returns></returns>
    bool AllowDestructuring(PropertyInfo property, AttributedDestructuringPolicyOptions options);
}
