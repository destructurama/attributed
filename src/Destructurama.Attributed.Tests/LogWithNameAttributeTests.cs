using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog.Events;
using Shouldly;

namespace Destructurama.Attributed.Tests;

[TestFixture]
public class LogWithNameAttributeTests
{
    [Test]
    public void AttributesAreConsultedWhenDestructuring()
    {
        var personalData = new PersonalData
        {
            Name = "John Doe"
        };

        var evt = DelegatingSink.Execute(personalData);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        var literalValue = props["FullName"].LiteralValue();
        literalValue.ShouldBe("John Doe");
    }

    #region LogWithName
    public class PersonalData
    {
        [LogWithName("FullName")]
        public string? Name { get; set; }
    }
    #endregion
}
