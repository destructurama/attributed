using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog.Events;
using Shouldly;

namespace Destructurama.Attributed.Tests;

[TestFixture]
public class AttributedDestructuringTests
{
    [Test]
    public void Throwing_Accessor_Should_Be_Handled()
    {
        // Setup
        var evt = DelegatingSink.Execute(new ClassWithThrowingAccessor());

        // Verify
        var sv = (StructureValue)evt.Properties["Customized"];
        sv.Properties.Count.ShouldBe(1);
        sv.Properties[0].Name.ShouldBe("BadProperty");
        sv.Properties[0].Value.ShouldBeOfType<ScalarValue>().Value.ShouldBe("***");
    }

    [Test]
    public void AttributesAreConsultedWhenDestructuring()
    {
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

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props["ImmutableScalar"].LiteralValue().ShouldBeOfType<ImmutableScalar>();
        props["MutableScalar"].LiteralValue().ShouldBe(new MutableScalar().ToString());
        props["NotAScalar"].ShouldBeOfType<StructureValue>();
        props.ContainsKey("Ignored").ShouldBeFalse();
        props["ScalarAnyway"].LiteralValue().ShouldBeOfType<NotAScalar>();
        props["Struct1"].LiteralValue().ShouldBeOfType<Struct1>();
        props["Struct2"].LiteralValue().ShouldBeOfType<Struct2>();

        var str = sv.ToString();
        str.Contains("This is a username").ShouldBeTrue();
        str.Contains("This is a password").ShouldBeFalse();
    }

    public class ClassWithThrowingAccessor
    {
        [LogMasked]
        public string? BadProperty => throw new FormatException("oops");
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

        [NotLogged]
        public string? Password { get; set; }
    }

    public struct Struct1
    {
        public int SomeProperty { get; set; }
        public override string ToString() => "AAA";
    }

    [LogAsScalar]
    public struct Struct2
    {
        public int SomeProperty { get; set; }
        public override string ToString() => "BBB";
    }
}
