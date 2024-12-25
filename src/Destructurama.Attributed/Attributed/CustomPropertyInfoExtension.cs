using System.Reflection;

namespace Destructurama.Attributed;

internal static class CustomPropertyInfoExtension
{
    /// <summary>
    /// Returns a list of custom attributes from the specified member or from corresponding member from another
    /// class if System.ComponentModel.DataAnnotations.MetadataTypeAttribute is used.
    /// </summary>
    public static IEnumerable<Attribute> GetCustomAttributesEx(this MemberInfo memberInfo, bool respectMetadata)
    {
        if (memberInfo.MemberType != MemberTypes.Property || !respectMetadata)
        {
            return memberInfo.GetCustomAttributes();
        }

        var type = memberInfo.DeclaringType;
        // Do not check attribute explicitly to not take dependency from System.ComponentModel.Annotations package.
        var metadataTypeAttribute = type.GetCustomAttributes(true).Where(t => t.GetType().FullName == "System.ComponentModel.DataAnnotations.MetadataTypeAttribute").FirstOrDefault();
        if (metadataTypeAttribute != null)
        {
            var metadataType = (Type)metadataTypeAttribute.GetType().GetProperty("MetadataClassType").GetValue(metadataTypeAttribute, null);
            return metadataType.GetProperty(memberInfo.Name).GetCustomAttributes();
        }
        return memberInfo.GetCustomAttributes();
    }
}
