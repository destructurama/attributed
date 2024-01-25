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

using BenchmarkDotNet.Attributes;
using Destructurama;
using Destructurama.Attributed;
using Serilog;
using Serilog.Core;

namespace Benchmarks;

public class AttributedBenchmarks
{
    private class LogAsScalarClass
    {
        [LogAsScalar]
        public string? Name { get; set; }

        [LogAsScalar]
        public LogAsScalarClass? Inner { get; set; }

        [LogAsScalar(isMutable: true)]
        public LogAsScalarClass? Inner2 { get; set; }
    }

    private class LogMaskedClass
    {
        [LogMasked]
        public string? Password1 { get; set; }

        [LogMasked(ShowFirst = 3)]
        public string? Password2 { get; set; }

        [LogMasked(ShowLast = 3)]
        public string? Password3 { get; set; }

        [LogMasked(ShowFirst = 3, ShowLast = 3)]
        public string? Password4 { get; set; }
    }

    private class LogReplacedClass
    {
        private const string REGEX_WITH_VERTICAL_BARS = @"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)";

        /// <summary>
        /// 123|456|789 results in "***|456|789"
        /// </summary>
        [LogReplaced(REGEX_WITH_VERTICAL_BARS, "***|$2|$3")]
        public string? RegexReplaceFirst { get; set; }
    }

    private class LogWithNameClass
    {
        [LogWithName("OtherName1")]
        public string? Name { get; set; }

        [LogWithName("OtherName2")]
        public LogWithNameClass? Inner { get; set; }
    }

    private class NotLoggedClass
    {
        [NotLogged]
        public string? Name { get; set; }

        [NotLogged]
        public NotLoggedClass? Inner { get; set; }
    }

    private class NotLoggedIfDefaultClass
    {
        [NotLoggedIfDefault]
        public string? Name { get; set; }

        [NotLoggedIfDefault]
        public int Age { get; set; }
    }

    private class NotLoggedIfNullClass
    {
        [NotLoggedIfNull]
        public string? Name { get; set; }

        [NotLoggedIfNull]
        public int Age { get; set; }
    }

    private readonly LogAsScalarClass _logAsScalar = new()
    {
        Name = "Tom",
        Inner = new LogAsScalarClass(),
        Inner2 = new LogAsScalarClass(),
    };

    private readonly LogMaskedClass _logMasked = new()
    {
        Password1 = "abcdef123456",
        Password2 = "abcdef123456",
        Password3 = "abcdef123456",
        Password4 = "abcdef123456",
    };

    private readonly LogReplacedClass _logReplaced = new()
    {
        RegexReplaceFirst = "123|456|789",
    };

    private readonly LogWithNameClass _logWithName = new()
    {
        Name = "Tome",
        Inner = new LogWithNameClass(),
    };

    private readonly NotLoggedClass _notLogged = new()
    {
        Name = "Tom",
        Inner = new NotLoggedClass(),
    };

    private readonly NotLoggedIfDefaultClass _notLoggedIfDefault = new()
    {
    };

    private readonly NotLoggedIfNullClass _notLoggedIfNull = new()
    {
    };

    private ILogEventPropertyValueFactory _factory = null!;
    private IDestructuringPolicy _policy = null!;

    [GlobalSetup]
    public void Setup()
    {
        (_policy, _factory) = Build(c => c.Destructure.UsingAttributes());
    }

    private static (IDestructuringPolicy, ILogEventPropertyValueFactory) Build(Func<LoggerConfiguration, LoggerConfiguration> configure)
    {
        var configuration = new LoggerConfiguration();
        var logger = configure(configuration).CreateLogger();

        var processor = logger.GetType().GetField("_messageTemplateProcessor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(logger)!;
        var converter = processor.GetType().GetField("_propertyValueConverter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(processor)!;
        var factory = (ILogEventPropertyValueFactory)converter;
        var policies = (IDestructuringPolicy[])converter.GetType().GetField("_destructuringPolicies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(converter)!;
        var policy = policies.First(p => p is AttributedDestructuringPolicy);
        return (policy, factory);
    }

    [Benchmark]
    public void LogAsScalar()
    {
        _policy.TryDestructure(_logAsScalar, _factory, out _);
    }

    [Benchmark]
    public void LogMasked()
    {
        _policy.TryDestructure(_logMasked, _factory, out _);
    }

    [Benchmark]
    public void LogReplaced()
    {
        _policy.TryDestructure(_logReplaced, _factory, out _);
    }

    [Benchmark]
    public void LogWithName()
    {
        _policy.TryDestructure(_logWithName, _factory, out _);
    }

    [Benchmark]
    public void NotLogged()
    {
        _policy.TryDestructure(_notLogged, _factory, out _);
    }

    [Benchmark]
    public void NotLoggedIfDefault()
    {
        _policy.TryDestructure(_notLoggedIfDefault, _factory, out _);
    }

    [Benchmark]
    public void NotLoggedIfNull()
    {
        _policy.TryDestructure(_notLoggedIfNull, _factory, out _);
    }
}
