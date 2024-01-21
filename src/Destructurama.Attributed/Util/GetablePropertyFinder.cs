// Copyright 2015 Destructurama Contributors, Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Reflection;

namespace Destructurama.Util;

internal static class GetablePropertyFinder
{
    public static IEnumerable<PropertyInfo> GetPropertiesRecursive(this Type type)
    {
        var seenNames = new HashSet<string>();

        var currentTypeInfo = type.GetTypeInfo();

        while (currentTypeInfo.AsType() != typeof(object))
        {
            var unseenProperties = currentTypeInfo.DeclaredProperties
                .Where(p => p.CanRead &&
                            p.GetMethod.IsPublic &&
                            !p.GetMethod.IsStatic &&
                            (p.Name != "Item" || p.GetIndexParameters().Length == 0) &&
                            !seenNames.Contains(p.Name));

            foreach (var propertyInfo in unseenProperties)
            {
                seenNames.Add(propertyInfo.Name);
                yield return propertyInfo;
            }

            currentTypeInfo = currentTypeInfo.BaseType.GetTypeInfo();
        }
    }
}
