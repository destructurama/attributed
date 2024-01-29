// Copyright 2015-2018 Destructurama Contributors, Serilog Contributors
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

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Destructurama.Util;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Destructurama.Attributed;

internal class AttributedDestructuringPolicy : IDestructuringPolicy
{
    private static readonly ConcurrentDictionary<Type, CacheEntry> _cache = new();
    private readonly AttributedDestructuringPolicyOptions _options;

    public AttributedDestructuringPolicy()
    {
        _options = new AttributedDestructuringPolicyOptions();
    }

    public AttributedDestructuringPolicy(Action<AttributedDestructuringPolicyOptions> configure)
        : this()
    {
        configure?.Invoke(_options);
    }

    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventPropertyValue? result)
    {
        var cached = _cache.GetOrAdd(value.GetType(), CreateCacheEntry);
        result = cached.DestructureFunc(value, propertyValueFactory);
        return cached.CanDestructure;
    }

    private static IEnumerable<PropertyInfo> GetPropertiesRecursive(Type type)
    {
        var seenNames = new HashSet<string>();

        while (type != typeof(object))
        {
            var unseenProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(p => p.CanRead && p.GetMethod.IsPublic && p.GetIndexParameters().Length == 0 && !seenNames.Contains(p.Name));

            foreach (var propertyInfo in unseenProperties)
            {
                seenNames.Add(propertyInfo.Name);
                yield return propertyInfo;
            }

            type = type.BaseType;
        }
    }

    private CacheEntry CreateCacheEntry(Type type)
    {
        static T GetCustomAttribute<T>(PropertyInfo propertyInfo) => propertyInfo.GetCustomAttributes().OfType<T>().FirstOrDefault();

        var classDestructurer = type.GetCustomAttributes().OfType<ITypeDestructuringAttribute>().FirstOrDefault();
        if (classDestructurer != null)
            return new(classDestructurer.CreateLogEventPropertyValue);

        var properties = GetPropertiesRecursive(type).ToList();
        if (!_options.IgnoreNullProperties && properties.All(pi =>
            GetCustomAttribute<IPropertyDestructuringAttribute>(pi) == null
            && GetCustomAttribute<IPropertyOptionalIgnoreAttribute>(pi) == null))
        {
            return CacheEntry.Ignore;
        }

        var optionalIgnoreAttributes = properties
            .Select(pi => new { pi, Attribute = GetCustomAttribute<IPropertyOptionalIgnoreAttribute>(pi) })
            .Where(o => o.Attribute != null)
            .ToDictionary(o => o.pi, o => o.Attribute);

        var destructuringAttributes = properties
            .Select(pi => new { pi, Attribute = GetCustomAttribute<IPropertyDestructuringAttribute>(pi) })
            .Where(o => o.Attribute != null)
            .ToDictionary(o => o.pi, o => o.Attribute);

        if (_options.IgnoreNullProperties && !optionalIgnoreAttributes.Any() && !destructuringAttributes.Any() && typeof(IEnumerable).IsAssignableFrom(type))
            return CacheEntry.Ignore;

        var propertiesWithAccessors = properties.Select(p => (p, Compile(p))).ToList();
        return new CacheEntry((o, f) => MakeStructure(o, propertiesWithAccessors, optionalIgnoreAttributes, destructuringAttributes, f, type));

        static Func<object, object> Compile(PropertyInfo property)
        {
            var objParameterExpr = Expression.Parameter(typeof(object), "instance");
            var instanceExpr = Expression.Convert(objParameterExpr, property.DeclaringType);
            var propertyExpr = Expression.Property(instanceExpr, property);
            var propertyObjExpr = Expression.Convert(propertyExpr, typeof(object));
            return Expression.Lambda<Func<object, object>>(propertyObjExpr, objParameterExpr).Compile();
        }
    }

    private LogEventPropertyValue MakeStructure(
        object o,
        List<(PropertyInfo Property, Func<object, object> Accessor)> loggedProperties,
        Dictionary<PropertyInfo, IPropertyOptionalIgnoreAttribute> optionalIgnoreAttributes,
        Dictionary<PropertyInfo, IPropertyDestructuringAttribute> destructuringAttributes,
        ILogEventPropertyValueFactory propertyValueFactory,
        Type type)
    {
        var structureProperties = new List<LogEventProperty>();
        foreach (var (pi, accessor) in loggedProperties)
        {
            object propValue;
            try
            {
                propValue = accessor(o);
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("The property accessor {0} threw exception {1}", pi, ex);
                propValue = $"The property accessor threw an exception: {ex.GetType().Name}";
            }

            if (optionalIgnoreAttributes.TryGetValue(pi, out var optionalIgnoreAttribute) && optionalIgnoreAttribute.ShouldPropertyBeIgnored(pi.Name, propValue, pi.PropertyType))
                continue;

            if (_options.IgnoreNullProperties && NotLoggedIfNullAttribute.Instance.ShouldPropertyBeIgnored(pi.Name, propValue, pi.PropertyType))
                continue;

            if (destructuringAttributes.TryGetValue(pi, out var destructuringAttribute))
            {
                if (destructuringAttribute.TryCreateLogEventProperty(pi.Name, propValue, propertyValueFactory, out var property))
                    structureProperties.Add(property);
            }
            else
            {
                structureProperties.Add(new(pi.Name, propertyValueFactory.CreatePropertyValue(propValue, true)));
            }
        }

        return new StructureValue(structureProperties, type.Name);
    }

    internal static void Clear()
    {
        _cache.Clear();
    }
}
