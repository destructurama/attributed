// Copyright 2014-2018 Destructurama Contributors, Serilog Contributors
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

using Destructurama.Attributed;
using Serilog;
using Serilog.Configuration;
using System;
using Serilog.Core;
using System.Runtime.CompilerServices;

#if (DEBUG || TEST)
[assembly: InternalsVisibleTo("Destructurama.Attributed.Tests, PublicKey = 0024000004800000940000000602000000240000525341310004000001000100638a43140e8a1271c1453df1379e64b40b67a1f333864c1aef5ac318a0fa2008545c3d35a82ef005edf0de1ad1e1ea155722fe289df0e462f78c40a668cbc96d7be1d487faef5714a54bb4e57909c86b3924c2db6d55ccf59939b99eb0cab6e8a91429ba0ce630c08a319b323bddcbbd509f1afe4ae77a6cbb8b447f588febc3")]
#endif

namespace Destructurama
{
    /// <summary>
    /// Adds the Destructure.UsingAttributes() extension to <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static class LoggerConfigurationAppSettingsExtensions
    {
        /// <summary>
        /// Adds a custom <see cref="IDestructuringPolicy"/> to enable manipulation of how objects
        /// are logged to Serilog using attributes.
        /// </summary>
        /// <param name="configuration">The logger configuration to apply configuration to.</param>
        /// <returns>An object allowing configuration to continue.</returns>
        public static LoggerConfiguration UsingAttributes(this LoggerDestructuringConfiguration configuration) =>
            configuration.With<AttributedDestructuringPolicy>();


        /// <summary>
        /// </summary>
        /// <param name="configuration">The logger configuration to apply configuration to.</param>
        /// <param name="configure">Configure Destructurama options</param>
        /// <returns>An object allowing configuration to continue.</returns>
        public static LoggerConfiguration UsingAttributes(this LoggerDestructuringConfiguration configuration,
            Action<AttributedDestructuringPolicyOptions> configure)
        {
            var policy = new AttributedDestructuringPolicy(configure);
            return configuration.With(policy);
        }
    }
}
