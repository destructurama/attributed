using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog.Events;
using Shouldly;

namespace Destructurama.Attributed.Tests;

[TestFixture]
public class LogWithNameAttributeTests
{
    [TestCase("John Doe")]
    [TestCase(null)]
    public void AttributesAreConsultedWhenDestructuring(string name)
    {
        var customized = new PersonalData
        {
            Name = name
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        var literalValue = props["FullName"].LiteralValue();
        literalValue.ShouldBe(name);
    }

    // https://github.com/destructurama/attributed/issues/65
    [Test]
    public void Issue65()
    {
        var customized = new MessageBase
        {
            Context = new ContextClass(),
        };
        var evt = DelegatingSink.Execute(customized);
        var sv = (StructureValue)evt.Properties["Customized"];
        sv.Properties.Count.ShouldBe(1);
        sv.Properties[0].Name.ShouldBe("messageContext");
        var sv2 = sv.Properties[0].Value.ShouldBeOfType<StructureValue>();
        sv2.Properties.Count.ShouldBe(2);
        sv2.Properties[0].Name.ShouldBe("Foo");
        sv2.Properties[1].Name.ShouldBe("Bar");
        sv2.Properties[0].Value.LiteralValue().ShouldBe("MyFoo");
        sv2.Properties[1].Value.LiteralValue().ShouldBe("MyBar");
    }

    public class MessageBase
    {
        [LogWithName("messageContext")]
        public ContextClass? Context { get; set; }
    }

    public class ContextClass
    {
        public string Foo { get; set; } = "MyFoo";

        public string Bar { get; set; } = "MyBar";

        public override string ToString() => "ContextClass ToString Output";
    }

    #region LogWithName
    public class PersonalData
    {
        [LogWithName("FullName")]
        public string? Name { get; set; }
    }
    #endregion
}
