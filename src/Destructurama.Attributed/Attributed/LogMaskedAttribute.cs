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
using System.Collections.Generic;
using System.Linq;
using Serilog.Core;
using Serilog.Events;

namespace Destructurama.Attributed
{
    /// <summary>
    /// Apply to a property to apply a mask to the logged value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LogMaskedAttribute : Attribute, IPropertyDestructuringAttribute
    {
        const string DefaultMask = "***";

        /// <summary>
        /// If set, the property value will be set to this text.
        /// </summary>
        public string Text { get; set; } = DefaultMask;
        /// <summary>
        /// Shows the first x characters in the property value.
        /// </summary>
        public int ShowFirst { get; set; }
        /// <summary>
        /// Shows the last x characters in the property value.
        /// </summary>
        public int ShowLast { get; set; }
        /// <summary>
        /// If set, it will swap out each character with the default value. Note that this
        /// property will be ignored if <see cref="Text"/> has been set to custom value.
        /// </summary>
        public bool PreserveLength { get; set; }

        private bool IsDefaultMask()
        {
            return Text == DefaultMask;
        }

        private object FormatMaskedValue(string val)
        {
            if (string.IsNullOrEmpty(val))
                return PreserveLength ? val : Text;

            if (ShowFirst == 0 && ShowLast == 0)
            {
                if (PreserveLength)
                    return new string(Text[0], val.Length);

                return Text;
            }

            if (ShowFirst > 0 && ShowLast == 0)
            {
                var first = val.Substring(0, Math.Min(ShowFirst, val.Length));

                if (!PreserveLength || !IsDefaultMask())
                    return first + Text;

                var mask = "";
                if (ShowFirst <= val.Length)
                    mask = new(Text[0], val.Length - ShowFirst);

                return first + mask;

            }

            if (ShowFirst == 0 && ShowLast > 0)
            {
                var last = ShowLast > val.Length ? val : val.Substring(val.Length - ShowLast);

                if (!PreserveLength || !IsDefaultMask())
                    return Text + last;

                var mask = "";
                if (ShowLast <= val.Length)
                    mask = new(Text[0], val.Length - ShowLast);

                return mask + last;
            }

            if (ShowFirst > 0 && ShowLast > 0)
            {
                if (ShowFirst + ShowLast >= val.Length)
                    return val;

                var first = val.Substring(0, ShowFirst);
                var last = val.Substring(val.Length - ShowLast);

                string? mask = null;
                if (PreserveLength && IsDefaultMask())
                    mask = new string(Text[0], val.Length - ShowFirst - ShowLast);

                return first + (mask ?? Text) + last;
            }

            return val;
        }

        /// <inheritdoc/>
        public bool TryCreateLogEventProperty(string name, object? value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventProperty? property)
        {
            property = new LogEventProperty(name, CreateValue(value));
            return true;
        }

        private LogEventPropertyValue CreateValue(object? value)
        {
            return value switch
            {
                IEnumerable<string> strings => new SequenceValue(strings.Select(s => new ScalarValue(FormatMaskedValue(s)))),
                string s => new ScalarValue(FormatMaskedValue(s)),
                _ => new ScalarValue(null)
            };
        }
    }
}