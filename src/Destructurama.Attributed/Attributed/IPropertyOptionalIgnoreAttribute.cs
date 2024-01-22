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

namespace Destructurama.Attributed;

/// <summary>
/// Base interface for all Destructurama attributes that determine should a property be ignored.
/// </summary>
public interface IPropertyOptionalIgnoreAttribute
{
    /// <summary>
    /// Determine should a property be ignored
    /// </summary>
    /// <param name="name">The current property name.</param>
    /// <param name="value">The current property value.</param>
    /// <param name="type">The current property type.</param>
    /// <returns></returns>
    bool ShouldPropertyBeIgnored(string name, object? value, Type type);
}
