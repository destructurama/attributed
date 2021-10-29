using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using System.Linq;

namespace Destructurama.Attributed.Tests
{
    public class CustomizedRegexLogs
    {
        const string RegexWithVerticalBars = @"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)";

        /// <summary>
        /// 123|456|789 results in "***|456|789"
        /// </summary>
        [LogReplaced(RegexWithVerticalBars, "***|$2|$3")]
        public string? RegexReplaceFirst { get; set; }

        /// <summary>
        /// 123|456|789 results in "123|***|789"
        /// </summary>
        [LogReplaced(RegexWithVerticalBars, "$1|***|$3")]
        public string? RegexReplaceSecond { get; set; }

        /// <summary>
        /// 123|456|789 results in "123|456|***"
        /// </summary>
        [LogReplaced(RegexWithVerticalBars, "$1|$2|***")]
        public string? RegexReplaceThird { get; set; }

        /// <summary>
        /// 123|456|789 results in "***|456|****"
        /// </summary>
        [LogReplaced(RegexWithVerticalBars, "***|$2|****")]
        public string? RegexReplaceFirstThird { get; set; }
    }

    [TestFixture]
    public class ReplacedAttributeTests
    {
        [Test]
        public void LogReplacedAttribute_Replaces_First()
        {
            // [LogReplaced(@"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)", "***|$2|$3")]
            // 123|456|789 -> "***|456|789"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedRegexLogs
            {
                RegexReplaceFirst = "123|456|789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("RegexReplaceFirst"));
            Assert.AreEqual("***|456|789", props["RegexReplaceFirst"].LiteralValue());
        }

        [Test]
        public void LogReplacedAttribute_Replaces_Second()
        {
            // [LogReplaced(@"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)", "$1|***|$3")]
            // 123|456|789 -> "123|***|789"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedRegexLogs
            {
                RegexReplaceSecond = "123|456|789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("RegexReplaceSecond"));
            Assert.AreEqual("123|***|789", props["RegexReplaceSecond"].LiteralValue());
        }

        [Test]
        public void LogReplacedAttribute_Replaces_Third()
        {
            // [LogReplaced(@"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)", "$1|$2|***")]
            // 123|456|789 -> "123|456|***"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedRegexLogs
            {
                RegexReplaceThird = "123|456|789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("RegexReplaceThird"));
            Assert.AreEqual("123|456|***", props["RegexReplaceThird"].LiteralValue());
        }

        [Test]
        public void LogReplacedAttribute_Replaces_FirstThird()
        {
            // [LogReplaced(@"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)", "***|$2|****")]
            // 123|456|789 -> "***|456|****"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedRegexLogs
            {
                RegexReplaceFirstThird = "123|456|789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("RegexReplaceFirstThird"));
            Assert.AreEqual("***|456|****", props["RegexReplaceFirstThird"].LiteralValue());
        }

        [Test]
        public void LogReplacedAttribute_Replaces_First_And_Third()
        {
            // [LogReplaced(@"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)", "***|$2|$3")]
            // 123|456|789 -> "***|456|789"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedRegexLogs
            {
                RegexReplaceFirst = "123|456|789",
                RegexReplaceThird = "123|456|789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("RegexReplaceFirst"));
            Assert.AreEqual("***|456|789", props["RegexReplaceFirst"].LiteralValue());
            Assert.IsTrue(props.ContainsKey("RegexReplaceThird"));
            Assert.AreEqual("123|456|***", props["RegexReplaceThird"].LiteralValue());
        }
    }
}