using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog.Events;
using Shouldly;

namespace Destructurama.Attributed.Tests;

public class CustomizedRegexLogs
{
    private const string REGEX_WITH_VERTICAL_BARS = @"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)";

    /// <summary>
    /// 123|456|789 results in "***|456|789"
    /// </summary>
    [LogReplaced(REGEX_WITH_VERTICAL_BARS, "***|$2|$3")]
    public string? RegexReplaceFirst { get; set; }

    /// <summary>
    /// 123|456|789 results in "123|***|789"
    /// </summary>
    [LogReplaced(REGEX_WITH_VERTICAL_BARS, "$1|***|$3")]
    public string? RegexReplaceSecond { get; set; }

    /// <summary>
    /// 123|456|789 results in "123|456|***"
    /// </summary>
    [LogReplaced(REGEX_WITH_VERTICAL_BARS, "$1|$2|***")]
    public string? RegexReplaceThird { get; set; }

    /// <summary>
    /// 123|456|789 results in "***|456|****"
    /// </summary>
    [LogReplaced(REGEX_WITH_VERTICAL_BARS, "***|$2|****")]
    public string? RegexReplaceFirstThird { get; set; }

    /// <summary>
    /// LogReplaced works only for string properties.
    /// </summary>
    [LogReplaced("does not matter", "does not matter")]
    public int RegexReplaceForInt { get; set; }
}

[TestFixture]
public class ReplacedAttributeTests
{
    [Test]
    public void LogReplacedAttribute_Replaces_First()
    {
        // [LogReplaced(@"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)", "***|$2|$3")]
        // 123|456|789 -> "***|456|789"
        var customized = new CustomizedRegexLogs
        {
            RegexReplaceFirst = "123|456|789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("RegexReplaceFirst").ShouldBeTrue();
        props["RegexReplaceFirst"].LiteralValue().ShouldBe("***|456|789");
    }

    [Test]
    public void LogReplacedAttribute_Replaces_Second()
    {
        // [LogReplaced(@"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)", "$1|***|$3")]
        // 123|456|789 -> "123|***|789"
        var customized = new CustomizedRegexLogs
        {
            RegexReplaceSecond = "123|456|789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("RegexReplaceSecond").ShouldBeTrue();
        props["RegexReplaceSecond"].LiteralValue().ShouldBe("123|***|789");
    }

    [Test]
    public void LogReplacedAttribute_Replaces_Third()
    {
        // [LogReplaced(@"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)", "$1|$2|***")]
        // 123|456|789 -> "123|456|***"
        var customized = new CustomizedRegexLogs
        {
            RegexReplaceThird = "123|456|789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("RegexReplaceThird").ShouldBeTrue();
        props["RegexReplaceThird"].LiteralValue().ShouldBe("123|456|***");
    }

    [Test]
    public void LogReplacedAttribute_Replaces_FirstThird()
    {
        // [LogReplaced(@"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)", "***|$2|****")]
        // 123|456|789 -> "***|456|****"
        var customized = new CustomizedRegexLogs
        {
            RegexReplaceFirstThird = "123|456|789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("RegexReplaceFirstThird").ShouldBeTrue();
        props["RegexReplaceFirstThird"].LiteralValue().ShouldBe("***|456|****");
    }

    [Test]
    public void LogReplacedAttribute_Replaces_First_And_Third()
    {
        // [LogReplaced(@"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)", "***|$2|$3")]
        // 123|456|789 -> "***|456|789"
        var customized = new CustomizedRegexLogs
        {
            RegexReplaceFirst = "123|456|789",
            RegexReplaceThird = "123|456|789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("RegexReplaceFirst").ShouldBeTrue();
        props["RegexReplaceFirst"].LiteralValue().ShouldBe("***|456|789");
        props.ContainsKey("RegexReplaceThird").ShouldBeTrue();
        props["RegexReplaceThird"].LiteralValue().ShouldBe("123|456|***");
    }

    [Test]
    public void LogReplacedAttribute_Should_Work_Only_For_String_Properties()
    {
        var customized = new CustomizedRegexLogs
        {
            RegexReplaceForInt = 42,
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("RegexReplaceForInt").ShouldBeFalse();
    }
}
