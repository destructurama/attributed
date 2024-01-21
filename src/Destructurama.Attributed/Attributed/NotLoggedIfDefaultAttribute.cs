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

using System.Collections.Concurrent;

namespace Destructurama.Attributed;

internal abstract class CachedValue
{
    public abstract bool IsDefaultValue(object value);
}

internal class CachedValue<T> : CachedValue where T : notnull
{
    private T Value { get; set; }

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
public class NotLoggedIfDefaultAttribute : Attribute, IPropertyOptionalIgnoreAttribute
{
    private static readonly ConcurrentDictionary<Type, CachedValue> _cache = new();

    bool IPropertyOptionalIgnoreAttribute.ShouldPropertyBeIgnored(string name, object? value, Type type)
    {
        if (value != null)
        {

            if (type.IsValueType)
            {
                if (!_cache.TryGetValue(type, out CachedValue cachedValue))
                {
                    var cachedValueType = typeof(CachedValue<>).MakeGenericType(type);
                    var defaultValue = Activator.CreateInstance(type);
                    cachedValue = (CachedValue)Activator.CreateInstance(cachedValueType, defaultValue);

                    _cache.TryAdd(type, cachedValue);
                }

                if (cachedValue.IsDefaultValue(value))
                {
                    return true;
                }
            }

            return false;
        }

        return true;
    }
}
