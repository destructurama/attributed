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
/// Specified that a property with <see langword="null"/> value should not be included when destructuring an object for logging.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class NotLoggedIfNullAttribute : Attribute, IPropertyOptionalIgnoreAttribute
{
    internal static readonly IPropertyOptionalIgnoreAttribute Instance = new NotLoggedIfNullAttribute();

    bool IPropertyOptionalIgnoreAttribute.ShouldPropertyBeIgnored(string name, object? value, Type type)
        => value == null;
}
