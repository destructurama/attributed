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
using System.Reflection;
using Destructurama.Util;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Destructurama.Attributed
{
    class AttributedDestructuringPolicy : IDestructuringPolicy
    {
        readonly static ConcurrentDictionary<Type, CacheEntry> _cache = new();
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

        private CacheEntry CreateCacheEntry(Type type)
        {
            var ti = type.GetTypeInfo();
            var classDestructurer = ti.GetCustomAttribute<ITypeDestructuringAttribute>();
            if (classDestructurer != null)
                return new((o, f) => classDestructurer.CreateLogEventPropertyValue(o, f));

            var properties = type.GetPropertiesRecursive().ToList();
            if (!_options.IgnoreNullProperties 
                && properties.All(pi => 
                    pi.GetCustomAttribute<IPropertyDestructuringAttribute>() == null
                    && pi.GetCustomAttribute<IPropertyOptionalIgnoreAttribute>() == null))
            {
                return CacheEntry.Ignore;
            }

            var optionalIgnoreAttributes = properties
                .Select(pi => new { pi, Attribute = pi.GetCustomAttribute<IPropertyOptionalIgnoreAttribute>() })
                .Where(o => o.Attribute != null)
                .ToDictionary(o => o.pi, o => o.Attribute);

            var destructuringAttributes = properties
                .Select(pi => new { pi, Attribute = pi.GetCustomAttribute<IPropertyDestructuringAttribute>() })
                .Where(o => o.Attribute != null)
                .ToDictionary(o => o.pi, o => o.Attribute);

            if (_options.IgnoreNullProperties && !optionalIgnoreAttributes.Any() && !destructuringAttributes.Any())
            {
                if (typeof(IEnumerable).IsAssignableFrom(type))
                    return CacheEntry.Ignore;
            }

            return new CacheEntry((o, f) => MakeStructure(o, properties, optionalIgnoreAttributes, destructuringAttributes, f, type));
        }

        private LogEventPropertyValue MakeStructure(
            object o, 
            IEnumerable<PropertyInfo> loggedProperties, 
            IDictionary<PropertyInfo, IPropertyOptionalIgnoreAttribute> optionalIgnoreAttributes, 
            IDictionary<PropertyInfo, IPropertyDestructuringAttribute> destructuringAttributes, 
            ILogEventPropertyValueFactory propertyValueFactory, 
            Type type)
        {
            var structureProperties = new List<LogEventProperty>();
            foreach (var pi in loggedProperties)
            {
                var propValue = SafeGetPropValue(o, pi);

                if (optionalIgnoreAttributes.TryGetValue(pi, out var optionalIgnoreAttribute))
                {
                    if (optionalIgnoreAttribute.ShouldPropertyBeIgnored(pi.Name, propValue, pi.PropertyType))
                        continue;
                }

                if (_options.IgnoreNullProperties)
                {
                    if (NotLoggedIfNullAttribute.Instance.ShouldPropertyBeIgnored(pi.Name, propValue, pi.PropertyType))
                        continue;
                }

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

        static object SafeGetPropValue(object o, PropertyInfo pi)
        {
            try
            {
                return pi.GetValue(o);
            }
            catch (TargetInvocationException ex)
            {
                SelfLog.WriteLine("The property accessor {0} threw exception {1}", pi, ex);
                return $"The property accessor threw an exception: {ex.InnerException!.GetType().Name}";
            }
        }

        internal static void Clear()
        {
            _cache.Clear();
        }
    }
}
