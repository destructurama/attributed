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

namespace Destructurama.Util;

internal readonly struct CacheEntry
{
    public CacheEntry(Func<object, ILogEventPropertyValueFactory, LogEventPropertyValue> destructureFunc)
    {
        CanDestructure = true;
        DestructureFunc = destructureFunc;
    }

    private CacheEntry(bool canDestructure, Func<object, ILogEventPropertyValueFactory, LogEventPropertyValue?> destructureFunc)
    {
        CanDestructure = canDestructure;
        DestructureFunc = destructureFunc;
    }

    public bool CanDestructure { get; }

    public Func<object, ILogEventPropertyValueFactory, LogEventPropertyValue?> DestructureFunc { get; }

    public static CacheEntry Ignore { get; } = new(false, (_, _) => null);
}
