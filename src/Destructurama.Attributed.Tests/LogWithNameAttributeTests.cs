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

    #region LogWithName
    public class PersonalData
    {
        [LogWithName("FullName")]
        public string? Name { get; set; }
    }
    #endregion
}
