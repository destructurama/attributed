using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace Destructurama.Attributed.Tests;

[TestFixture]
public class AttributedDestructuringTests
{
    [Test]
    public void AttributesAreConsultedWhenDestructuring()
    {
        LogEvent evt = null!;

        var log = new LoggerConfiguration()
            .Destructure.UsingAttributes()
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        var customized = new Customized
        {
            ImmutableScalar = new(),
            MutableScalar = new(),
            NotAScalar = new(),
            Ignored = "Hello, there",
            ScalarAnyway = new(),
            AuthData = new()
            {
                Username = "This is a username",
                Password = "This is a password"
            }
        };

        log.Information("Here is {@Customized}", customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        Assert.IsInstanceOf<ImmutableScalar>(props["ImmutableScalar"].LiteralValue());
        Assert.AreEqual(new MutableScalar().ToString(), props["MutableScalar"].LiteralValue());
        Assert.IsInstanceOf<StructureValue>(props["NotAScalar"]);
        Assert.IsFalse(props.ContainsKey("Ignored"));
        Assert.IsInstanceOf<NotAScalar>(props["ScalarAnyway"].LiteralValue());
        Assert.IsInstanceOf<Struct1>(props["Struct1"].LiteralValue());
        Assert.IsInstanceOf<Struct2>(props["Struct2"].LiteralValue());

        var str = sv.ToString();
        Assert.That(str.Contains("This is a username"));
        Assert.False(str.Contains("This is a password"));
    }

    [LogAsScalar]
    public class ImmutableScalar
    {
    }

    [LogAsScalar(isMutable: true)]
    public class MutableScalar
    {
    }

    public class NotAScalar
    {
    }

    public class Customized
    {
        public ImmutableScalar? ImmutableScalar { get; set; }
        public MutableScalar? MutableScalar { get; set; }
        public NotAScalar? NotAScalar { get; set; }

        [NotLogged]
        public string? Ignored { get; set; }

        [LogAsScalar]
        public NotAScalar? ScalarAnyway { get; set; }

        public UserAuthData? AuthData { get; set; }

        [LogAsScalar]
        public Struct1 Struct1 { get; set; }

        public Struct2 Struct2 { get; set; }
    }

    public class UserAuthData
    {
        public string? Username { get; set; }

        [NotLogged] public string? Password { get; set; }
    }

    public struct Struct1
    {
        public override string ToString() => "AAA";
    }

    [LogAsScalar]
    public struct Struct2
    {
        public override string ToString() => "BBB";
    }
}
