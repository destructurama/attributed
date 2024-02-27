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

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Serilog.Core;
using Serilog.Events;

namespace Destructurama.Attributed;

/// <summary>
/// Apply to a property to replace the current value.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class LogReplacedAttribute : Attribute, IPropertyDestructuringAttribute
{
    private readonly string _pattern;
    private readonly string _replacement;

    /// <summary>
    /// The options that will be applied. Defaults to <see cref="RegexOptions.None"/>
    /// </summary>
    public RegexOptions Options { get; set; }

    /// <summary>
    /// A time-out interval to evaluate regular expression. Defaults to <see cref="Regex.InfiniteMatchTimeout"/>
    /// </summary>
    public TimeSpan Timeout { get; set; } = Regex.InfiniteMatchTimeout;

    /// <summary>
    /// Construct a <see cref="LogWithNameAttribute"/>.
    /// </summary>
    /// <param name="pattern">The pattern that should be applied on value.</param>
    /// <param name="replacement">The pattern that should be applied on value.</param>
    public LogReplacedAttribute(string pattern, string replacement)
    {
        _pattern = pattern;
        _replacement = replacement;
    }

    /// <inheritdoc/>
    public bool TryCreateLogEventProperty(string name, object? value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventProperty? property)
    {
        if (value == null)
        {
            property = new(name, ScalarValue.Null);
            return true;
        }

        if (value is string s)
        {
            var replacement = Regex.Replace(s, _pattern, _replacement, Options, Timeout);

            property = new(name, new ScalarValue(replacement));
            return true;
        }

        property = null;
        return false;
    }
}
