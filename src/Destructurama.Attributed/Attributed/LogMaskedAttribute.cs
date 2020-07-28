﻿// Copyright 2015-2018 Destructurama Contributors, Serilog Contributors
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
    [AttributeUsage(AttributeTargets.Property)]
    public class LogMaskedAttribute : Attribute, IPropertyDestructuringAttribute
    {
        const string DefaultMask = "***";

        public string Text { get; set; } = DefaultMask;
        public int ShowFirst { get; set; }
        public int ShowLast { get; set; }
        public bool PreserveLength { get; set; }

        /// <summary>
        /// Check to see if custom Text has been provided.
        /// If true PreserveLength is ignored.
        /// </summary>
        /// <returns></returns>
        internal bool IsDefaultMask()
        {
            return Text == DefaultMask;
        }

        private object FormatMaskedValue(string val)
        {
            if (string.IsNullOrEmpty(val))
                return val;

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
                    mask = new string(Text[0], val.Length - ShowFirst);

                return first + mask;

            }

            if (ShowFirst == 0 && ShowLast > 0)
            {
                var last = ShowLast > val.Length ? val : val.Substring(val.Length - ShowLast);

                if (!PreserveLength || !IsDefaultMask())
                    return Text + last;

                var mask = "";
                if (ShowLast <= val.Length)
                    mask = new string(Text[0], val.Length - ShowLast);

                return mask + last;
            }

            if (ShowFirst > 0 && ShowLast > 0)
            {
                if (ShowFirst + ShowLast >= val.Length)
                    return val;

                var first = val.Substring(0, ShowFirst);
                var last = val.Substring(val.Length - ShowLast);

                return first + Text + last;
            }

            return val;
        }

        public bool TryCreateLogEventProperty(string name, object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventProperty property)
        {
            property = new LogEventProperty(name, CreateValue(value));
            return true;
        }

        private LogEventPropertyValue CreateValue(object value)
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
