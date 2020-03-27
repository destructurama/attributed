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
using Serilog.Core;
using Serilog.Events;
#if NETSTANDARD1_1
using System.Reflection;
#endif

namespace Destructurama.Attributed
{
    /// <summary>
    /// Specified that a property with default value for its type should not be included when destructuring an object for logging.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotLoggedIfDefaultAttribute : Attribute, IPropertyDestructuringAttribute
    {
        public bool TryCreateLogEventProperty(string name, object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventProperty property)
        {
            if (value != null)
            {
                var type = value.GetType();
                object typeDefaultValue = null;

#if NETSTANDARD1_1
                if (type.GetTypeInfo().IsValueType)
#else
                if (type.IsValueType)
#endif
                    typeDefaultValue = Activator.CreateInstance(type);

                if (!value.Equals(typeDefaultValue))
                {
                    property = new LogEventProperty(name, new ScalarValue(value));
                    return true;
                }
            }

            property = null;
            return false;
        }
    }
}
