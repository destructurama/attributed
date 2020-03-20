// Copyright 2015-2020 Destructurama Contributors, Serilog Contributors
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
using System.Text.RegularExpressions;
using Serilog.Core;
using Serilog.Events;

namespace Destructurama.Attributed
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LogReplacedAttribute : Attribute, IPropertyDestructuringAttribute
    {
        private readonly string pattern;
        private readonly string replacement;
        private readonly RegexOptions regexOptions;

        public LogReplacedAttribute(string pattern, string replacement)
            : this(pattern, replacement, RegexOptions.None)
        {
        }

        public LogReplacedAttribute(string pattern, string replacement, RegexOptions regexOptions)
        {
            this.pattern = pattern;
            this.replacement = replacement;
            this.regexOptions = regexOptions;
        }

        public bool TryCreateLogEventProperty(string name, object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventProperty property)
        {
            if (value == null)
            {
                property = new LogEventProperty(name, new ScalarValue(value));
                return true;
            }

            try
            {
                if (value is string s)
                {
                    var replacement = Regex.Replace(s, pattern, this.replacement, regexOptions);
                    property = new LogEventProperty(name, new ScalarValue(replacement));
                    return true;
                }
                else
                {
                    property = null;
                    return false;
                }
            }
            catch
            {
                property = null;
                return false;
            }
        }
    }
}
