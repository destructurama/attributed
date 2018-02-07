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

using System;
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
        readonly object _cacheLock = new object();
        readonly HashSet<Type> _ignored = new HashSet<Type>();
        readonly Dictionary<Type, Func<object, ILogEventPropertyValueFactory, LogEventPropertyValue>> _cache = new Dictionary<Type, Func<object, ILogEventPropertyValueFactory, LogEventPropertyValue>>();

        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            var t = value.GetType();
            lock (_cacheLock)
            {
                if (_ignored.Contains(t))
                {
                    result = null;
                    return false;
                }

                Func<object, ILogEventPropertyValueFactory, LogEventPropertyValue> cached;
                if (_cache.TryGetValue(t, out cached))
                {
                    result = cached(value, propertyValueFactory);
                    return true;
                }
            }

            var ti = t.GetTypeInfo();

            var logAsScalar = ti.GetCustomAttribute<LogAsScalarAttribute>();
            if (logAsScalar != null)
            {
                lock (_cacheLock)
                    _cache[t] = (o, f) => MakeScalar(o, logAsScalar.IsMutable);

            }
            else
            {
                var properties = t.GetPropertiesRecursive()
                    .ToList();
                if (properties.Any(pi =>
                    pi.GetCustomAttribute<LogAsScalarAttribute>() != null
                    || pi.GetCustomAttribute<NotLoggedAttribute>() != null
                    || pi.GetCustomAttribute<LogMaskedAttribute>() != null))
                {
                    var loggedProperties = properties
                        .Where(pi => pi.GetCustomAttribute<NotLoggedAttribute>() == null)
                        .ToList();

                    var scalars = loggedProperties
                        .Where(pi => pi.GetCustomAttribute<LogAsScalarAttribute>() != null)
                        .ToDictionary(pi => pi, pi => pi.GetCustomAttribute<LogAsScalarAttribute>().IsMutable);

                    lock (_cacheLock)
                        _cache[t] = (o, f) => MakeStructure(o, loggedProperties, scalars, f, t);
                }
                else
                {
                    lock (_cacheLock)
                        _ignored.Add(t);
                }
            }

            return TryDestructure(value, propertyValueFactory, out result);
        }

        static LogEventPropertyValue MakeStructure(object value, IEnumerable<PropertyInfo> loggedProperties, Dictionary<PropertyInfo, bool> scalars, ILogEventPropertyValueFactory propertyValueFactory, Type type)
        {
            var structureProperties = new List<LogEventProperty>();
            foreach (var pi in loggedProperties)
            {
                object propValue;
                try
                {
                    propValue = pi.GetValue(value);
                }
                catch (TargetInvocationException ex)
                {
                    SelfLog.WriteLine("The property accessor {0} threw exception {1}", pi, ex);
                    propValue = "The property accessor threw an exception: " + ex.InnerException.GetType().Name;
                }

                var maskedAttribute = pi.GetCustomAttribute<LogMaskedAttribute>();
                if (maskedAttribute != null)
                {
                    // Only for string values
                    if (propValue is string)
                    {
                        FormatMaskedValue(ref propValue, maskedAttribute);
                    }
                }

                LogEventPropertyValue pv;
                bool stringify;

                if (propValue == null)
                {
                    pv = new ScalarValue(null);
                }
                else if (scalars.TryGetValue(pi, out stringify))
                {
                    pv = MakeScalar(propValue, stringify);
                }
                else
                {
                    pv = propertyValueFactory.CreatePropertyValue(propValue, true);
                }

                structureProperties.Add(new LogEventProperty(pi.Name, pv));
            }
            return new StructureValue(structureProperties, type.Name);
        }

        static ScalarValue MakeScalar(object value, bool stringify)
        {
            return new ScalarValue(stringify ? value.ToString() : value);
        }

        private static void FormatMaskedValue(ref object propValue, LogMaskedAttribute attribute)
        {
            var val = propValue as string;

            if (string.IsNullOrEmpty(val))
            {
                propValue = val;
            }
            else
            if (attribute.ShowFirst == 0 && attribute.ShowLast == 0)
            {
                if (attribute.PreserveLength)
                {
                    propValue = new String(attribute.Text[0], val.Length);
                }
                else
                {
                    propValue = attribute.Text;
                }
            }
            else if (attribute.ShowFirst > 0 && attribute.ShowLast == 0)
            {
                var first = val.Substring(0, Math.Min(attribute.ShowFirst, val.Length));

                if (attribute.PreserveLength && attribute.IsDefaultMask())
                {
                    string mask;
                    if (attribute.ShowFirst > val.Length)
                        mask = "";
                    else
                        mask = new String(attribute.Text[0], val.Length - attribute.ShowFirst);
                    propValue = first + mask;
                }
                else
                {
                    propValue = first + attribute.Text;
                }
            }
            else if (attribute.ShowFirst == 0 && attribute.ShowLast > 0)
            {
                string last;
                if (attribute.ShowLast > val.Length)
                    last = val;
                else
                    last = val.Substring(val.Length - attribute.ShowLast);

                if (attribute.PreserveLength && attribute.IsDefaultMask())
                {
                    string mask = "";
                    if (attribute.ShowLast <= val.Length)
                        mask = new String(attribute.Text[0], val.Length - attribute.ShowLast);

                    propValue = mask + last;
                }
                else
                {
                    propValue = attribute.Text + last;
                }
            }
            else if (attribute.ShowFirst > 0 && attribute.ShowLast > 0)
            {
                var first = val.Substring(0, attribute.ShowFirst);
                var last = val.Substring(val.Length - attribute.ShowLast);

                propValue = first + attribute.Text + last;
            }
        }
    }
}
