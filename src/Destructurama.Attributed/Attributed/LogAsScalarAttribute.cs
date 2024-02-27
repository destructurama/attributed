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

using Serilog.Core;
using Serilog.Events;

namespace Destructurama.Attributed;

/// <summary>
/// Specified that the type or property it is applied to should never be
/// destructured; instead it should be logged as an atomic value.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property)]
public class LogAsScalarAttribute : Attribute, ITypeDestructuringAttribute, IPropertyDestructuringAttribute
{
    private readonly bool _isMutable;

    /// <summary>
    /// Construct a <see cref="LogAsScalarAttribute"/>.
    /// </summary>
    /// <param name="isMutable">
    /// Whether the scalar value should be converted into a string before
    /// being passed down the (asynchronous) logging pipeline. For mutable
    /// types, specify <code>true</code>, otherwise leave as false.
    /// </param>
    public LogAsScalarAttribute(bool isMutable = false)
    {
        _isMutable = isMutable;
    }

    /// <inheritdoc/>
    public LogEventPropertyValue CreateLogEventPropertyValue(object? value, ILogEventPropertyValueFactory propertyValueFactory)
    {
        var v = _isMutable ? value?.ToString() : value;
        return v == null ? ScalarValue.Null : new ScalarValue(v);
    }

    /// <inheritdoc/>
    public bool TryCreateLogEventProperty(string name, object? value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventProperty property)
    {
        property = new(name, CreateLogEventPropertyValue(value, propertyValueFactory));
        return true;
    }
}
