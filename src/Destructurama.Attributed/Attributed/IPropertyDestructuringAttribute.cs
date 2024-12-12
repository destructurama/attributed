// Copyright 2018 Destructurama Contributors, Serilog Contributors
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
/// Base interface for all Destructurama attributes that overrides how a property is destructured.
/// </summary>
public interface IPropertyDestructuringAttribute
{
    /// <summary>
    /// Attempt to create a replacement <see cref="LogEventProperty"/> for a property.
    /// </summary>
    /// <param name="name">The current property name.</param>
    /// <param name="value">The current property value.</param>
    /// <param name="propertyValueFactory">The current <see cref="ILogEventPropertyValueFactory"/>.</param>
    /// <param name="property">The <see cref="LogEventProperty"/> to use as a replacement.</param>
    /// <returns><code>true</code>If a replacement <see cref="LogEventProperty"/> has been derived.</returns>
    bool TryCreateLogEventProperty(string name, object? value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventProperty? property);
}
