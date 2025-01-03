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
        if (!respectMetadata || memberInfo.MemberType != MemberTypes.Property)
        {
            return memberInfo.GetCustomAttributes();
        }

        var type = memberInfo.DeclaringType;

        // Do not check attribute explicitly to not take dependency from System.ComponentModel.Annotations package.
        // Check for direct declared Custom Attributes
        var metadataTypeAttribute = type.GetCustomAttributes(true).Where(t => t.GetType().FullName == "System.ComponentModel.DataAnnotations.MetadataTypeAttribute").FirstOrDefault();
        if (metadataTypeAttribute != null)
        {
            var metadataType = (Type)metadataTypeAttribute.GetType().GetProperty("MetadataClassType").GetValue(metadataTypeAttribute, null);
            var metaDataProperty = metadataType.GetProperty(memberInfo.Name);

            if (metaDataProperty != null)
            {
                var metaProps = metaDataProperty.GetCustomAttributes().ToList();
                // because there seem to be no definition to force metadata property to be of object,
                // lookup Metadata specified at type of property
                var metaPropType = metaDataProperty.PropertyType;
                var metaPropTypeType = metaPropType.GetCustomAttributes(true).Where(t => t.GetType().FullName == "System.ComponentModel.DataAnnotations.MetadataTypeAttribute").FirstOrDefault();
                if (metaPropTypeType != null)
                {
                    var metaPropTypeMetadataType = (Type)metaPropTypeType.GetType().GetProperty("MetadataClassType").GetValue(metaPropTypeType, null);
                    var propertyAttr = metaPropTypeMetadataType.GetCustomAttributes();
                    metaProps.AddRange(propertyAttr);
                }
                if (metaProps != null && metaProps.Any())
                {
                    return metaProps;
                }
                // Property specified in Metadata class does not have any attributes specified, look at original property
            }

            // Property was not declared in MetadataClassType, or does not have an attribute specified
            // fall through and return attributes from original property.
        }
        // if there is no MetadataType Attribute at declaringtype look whether the propertytype has a metadatatype attribute
        var propertyType = type.GetProperty(memberInfo.Name).PropertyType;
        var propertyTypeMeta = propertyType.GetCustomAttributes(true).Where(t => t.GetType().FullName == "System.ComponentModel.DataAnnotations.MetadataTypeAttribute").FirstOrDefault();
        if (propertyTypeMeta != null)
        {
            var propertyMetadataType = (Type)propertyTypeMeta.GetType().GetProperty("MetadataClassType").GetValue(propertyTypeMeta, null);
            var propertyAttr = propertyMetadataType.GetCustomAttributes();
            return propertyAttr;
        }
        // PropertyType has no MetadataClassType, fall through and return attributes from original property.

        return memberInfo.GetCustomAttributes();
    }

}
