// Copyright 2017 Serilog Contributors
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

using NUnit.Framework;
using PublicApiGenerator;
using Shouldly;

namespace Destructurama.Attributed.Tests;

/// <summary>Tests for checking changes to the public API.</summary>
[TestFixture]
public class ApiApprovalTests
{
    /// <summary>Check for changes to the public APIs.</summary>
    /// <param name="type">The type used as a marker for the assembly whose public API change you want to check.</param>
    [TestCase(typeof(LoggerConfigurationAttributedExtensions))]
    public void PublicApi_Should_Not_Change_Unintentionally(Type type)
    {
        string publicApi = type.Assembly.GeneratePublicApi(new()
        {
            IncludeAssemblyAttributes = false,
            AllowNamespacePrefixes = ["System", "Microsoft.Extensions.DependencyInjection"],
            ExcludeAttributes = ["System.Diagnostics.DebuggerDisplayAttribute"],
        });

        publicApi.ShouldMatchApproved(options => options.NoDiff().WithFilenameGenerator((testMethodInfo, discriminator, fileType, fileExtension) => $"{type.Assembly.GetName().Name!}.{fileType}.{fileExtension}"));
    }
}
