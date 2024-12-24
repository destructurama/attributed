using System.Reflection;

namespace Destructurama.Attributed;
internal static class CustomPropertyInfoExtension
{
    /// <summary>
    /// Returns a list of CustomAttributes from MemberdataClassType if exists, otherwise from memberinfo
    /// </summary>
    /// <param name="memberInfo">MemberInfo</param>
    /// <param name="respectMetadata">true: respect Custom Atrributes in MetadataClassType</param>
    /// <returns>List of CustomAttributes either from MetadataClass or data class</returns>
    public static IEnumerable<Attribute> GetCustomAttributesFromMetadataClass(this MemberInfo memberInfo, bool respectMetadata)
    {
        if (memberInfo.MemberType != MemberTypes.Property)
        {
            return memberInfo.GetCustomAttributes();
        }
        if (respectMetadata)
        {
            var type = memberInfo.DeclaringType;
            var metaDataType = type.GetCustomAttributes(true).Where(t => t.GetType().FullName == "System.ComponentModel.DataAnnotations.MetadataTypeAttribute").FirstOrDefault();
            if (metaDataType != null)
            {
                var metaClass = (Type)metaDataType.GetType().GetProperty("MetadataClassType").GetValue(metaDataType, null);
                var metaProp = metaClass.GetProperty(memberInfo.Name);
                var attr = metaProp.GetCustomAttributes();
                return attr;
            }

        }
        return memberInfo.GetCustomAttributes();
    }
}
