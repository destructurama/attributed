using System.Reflection;

namespace Destructurama.Attributed;

internal static class CustomPropertyInfoExtension
{
    /// <summary>
    /// Returns a list of custom attributes from the specified property or from corresponding property from another
    /// class if System.ComponentModel.DataAnnotations.MetadataTypeAttribute is used.
    /// </summary>
    public static IEnumerable<Attribute> GetCustomAttributesEx(this PropertyInfo propertyInfo, bool respectMetadata)
    {
        if (!respectMetadata)
        {
            return propertyInfo.GetCustomAttributes();
        }

        // Get the type in which property is declared to look whether MetadataTypeAttribute is specified.
        // If so, get the class, find the property with same name and if exists, return its custom attributes.
        var type = propertyInfo.DeclaringType;

        // Do not check attribute explicitly to not take dependency from System.ComponentModel.Annotations package.
        var metadataTypeAttribute = type.GetCustomAttributes(true).Where(t => t.GetType().FullName == "System.ComponentModel.DataAnnotations.MetadataTypeAttribute").FirstOrDefault();
        if (metadataTypeAttribute != null)
        {
            var metadataType = (Type)metadataTypeAttribute.GetType().GetProperty("MetadataClassType").GetValue(metadataTypeAttribute, null);
            var metadataProperty = metadataType.GetProperty(propertyInfo.Name);

            if (metadataProperty != null)
            {
                return metadataProperty.GetCustomAttributes();
            }

            // Property was not declared in MetadataClassType, fall through and return attributes from original property.
        }

        return propertyInfo.GetCustomAttributes();
    }
}
