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

using System.Diagnostics.CodeAnalysis;
using Serilog.Core;
using Serilog.Events;

namespace Destructurama.Attributed;

/// <summary>
/// Apply to a property to use a custom name when that property is logged.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class LogWithNameAttribute : Attribute, IPropertyDestructuringAttribute
{
    private readonly string _newName;

    /// <summary>
    /// Construct a <see cref="LogWithNameAttribute"/>.
    /// </summary>
    /// <param name="newName">The new name to use when logging the target property.</param>
    public LogWithNameAttribute(string newName)
    {
        _newName = newName;
    }

    /// <inheritdoc/>
    public bool TryCreateLogEventProperty(string name, object? value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventProperty? property)
    {
        property = new LogEventProperty(_newName, propertyValueFactory.CreatePropertyValue(value, destructureObjects: true));
        return true;
    }
}
