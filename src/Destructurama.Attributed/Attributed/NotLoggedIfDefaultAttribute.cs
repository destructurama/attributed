// Copyright 2020 Destructurama Contributors, Serilog Contributors
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
using Serilog.Core;
using Serilog.Events;
#if NETSTANDARD1_1
using System.Reflection;
#endif

namespace Destructurama.Attributed
{
    abstract class CachedValue
    {
        public abstract bool IsDefaultValue(object value);
    }

    class CachedValue<T> : CachedValue
    {
        T Value { get; set; }

        public CachedValue(T value)
        {
            Value = value;
        }

        public override bool IsDefaultValue(object value)
        {
            return Value.Equals(value);
        }
    }

    /// <summary>
    /// Specified that a property with default value for its type should not be included when destructuring an object for logging.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotLoggedIfDefaultAttribute : Attribute, IPropertyDestructuringAttribute
    {
        readonly static ConcurrentDictionary<Type, CachedValue> _cache = new ConcurrentDictionary<Type, CachedValue>();

        public bool TryCreateLogEventProperty(string name, object value, Type type, ILogEventPropertyValueFactory propertyValueFactory, out LogEventProperty property)
        {
            if (value != null)
            {

#if NETSTANDARD1_1
                if (type.GetTypeInfo().IsValueType)
#else
                if (type.IsValueType)
#endif
                {
                    CachedValue cachedValue;

                    if (!_cache.TryGetValue(type, out cachedValue))
                    {
                        var cachedValueType = typeof(CachedValue<>).MakeGenericType(type);
                        var defaultValue = Activator.CreateInstance(type);
                        cachedValue = (CachedValue)Activator.CreateInstance(cachedValueType, defaultValue);

                        _cache.TryAdd(type, cachedValue);
                    }

                    if (cachedValue.IsDefaultValue(value))
                    {
                        property = null;
                        return false;
                    }
                }

                property = new LogEventProperty(name, new ScalarValue(value));
                return true;
            }

            property = null;
            return false;
        }
    }
}
