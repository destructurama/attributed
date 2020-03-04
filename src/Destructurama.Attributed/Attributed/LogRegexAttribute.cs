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
    public class LogRegexAttribute : Attribute, IPropertyDestructuringAttribute
    {
        // Everything except blank characters
        const string DefaultPattern = @"^(?!\s*$).+";
        const string DefaultReplacement = "***";

        public RegexOptions RegexOptions { get; set; }
        public string Pattern = DefaultPattern;
        public string Replacement = DefaultReplacement;

        public bool TryCreateLogEventProperty(string name, object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventProperty property)
        {
            if (value == null)
            {
                property = new LogEventProperty(name, new ScalarValue(value));
                return true;
            }

            try
            {
                var replacement = Regex.Replace(value as string, Pattern, Replacement, RegexOptions);
                property = new LogEventProperty(name, new ScalarValue(replacement));
                return true;
            }
            catch
            {
                property = null;
                return false;
            }
        }
    }
}
