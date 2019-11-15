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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Destructurama.Util;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Destructurama.Attributed
{
    class AttributedDestructuringPolicy : IDestructuringPolicy
    {
        readonly static ConcurrentDictionary<Type, CacheEntry> _cache = new ConcurrentDictionary<Type, CacheEntry>();

        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            var cached = _cache.GetOrAdd(value.GetType(), CreateCacheEntry);
            result = cached.DestructureFunc(value, propertyValueFactory);
            return cached.CanDestructure;
        }

        static CacheEntry CreateCacheEntry(Type type)
        {
            var classDestructurer = type.GetTypeInfo().GetCustomAttribute<ITypeDestructuringAttribute>();
            if (classDestructurer != null)
                return new CacheEntry((o, f) => classDestructurer.CreateLogEventPropertyValue(o, f));
            
            var properties = type.GetPropertiesRecursive().ToList();
            if (properties.All(pi => pi.GetCustomAttribute<IPropertyDestructuringAttribute>() == null))
                return CacheEntry.Ignore;
            
            var destructuringAttributes = properties
                .Select(pi => new { pi, Attribute = pi.GetCustomAttribute<IPropertyDestructuringAttribute>() })
                .Where(o => o.Attribute != null)
                .ToDictionary(o => o.pi, o => o.Attribute);

            return new CacheEntry((o, f) => MakeStructure(o, properties, destructuringAttributes, f, type));
        }

        static LogEventPropertyValue MakeStructure(object o, IEnumerable<PropertyInfo> loggedProperties, IDictionary<PropertyInfo, IPropertyDestructuringAttribute> destructuringAttributes, ILogEventPropertyValueFactory propertyValueFactory, Type type)
        {
            var structureProperties = new List<LogEventProperty>();
            foreach (var pi in loggedProperties)
            {
                var propValue = SafeGetPropValue(o, pi);

                if (destructuringAttributes.TryGetValue(pi, out var destructuringAttribute))
                {
                    if (destructuringAttribute.TryCreateLogEventProperty(pi.Name, propValue, propertyValueFactory, out var property))
                        structureProperties.Add(property);
                }
                else
                {
                    structureProperties.Add(new LogEventProperty(pi.Name, propertyValueFactory.CreatePropertyValue(propValue, true)));
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
                return "The property accessor threw an exception: " + ex.InnerException.GetType().Name;
            }
        }
    }
}
