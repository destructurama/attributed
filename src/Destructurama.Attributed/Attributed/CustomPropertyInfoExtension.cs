using System.Reflection;

namespace Destructurama.Attributed;

internal static class CustomPropertyInfoExtension
{
    /// <summary>
    /// Returns a list of custom attributes from the specified property or from corresponding property from another
    /// class if System.ComponentModel.DataAnnotations.MetadataTypeAttribute is used.
    /// </summary>
    public static IEnumerable<Attribute> GetCustomAttributesEx(this MemberInfo memberInfo, bool respectMetadata)
    {
        if (!respectMetadata)
        {
            return memberInfo.GetCustomAttributes();
        }

        // Get the type in which property is declared to look whether MetadataTypeAttribute is specified.
        // If so, get the class, find the property with same name and if exists, return its custom attributes.
        var type = memberInfo.DeclaringType;

        // Do not check attribute explicitly to not take dependency from System.ComponentModel.Annotations package.
        var metadataTypeAttribute = type.GetCustomAttributes(true).Where(t => t.GetType().FullName == "System.ComponentModel.DataAnnotations.MetadataTypeAttribute").FirstOrDefault();
        if (metadataTypeAttribute != null)
        {
            var metadataType = (Type)metadataTypeAttribute.GetType().GetProperty("MetadataClassType").GetValue(metadataTypeAttribute, null);
            var metaDataProperty = metadataType.GetProperty(memberInfo.Name);

            if (metaDataProperty != null)
            {
                return metaDataProperty.GetCustomAttributes().ToList();

                // Property specified in Metadata class does not have any attributes specified, look at original property
            }

            // Property was not declared in MetadataClassType, or does not have an attribute specified
            // fall through and return attributes from original property.
        }

        return memberInfo.GetCustomAttributes();
    }

}
