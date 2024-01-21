using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using System.Linq;

namespace Destructurama.Attributed.Tests
{
    [TestFixture]
    public class LogWithNameAttributedTests
    {

        [Test]
        public void AttributesAreConsultedWhenDestructuring()
        {
            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var personalData = new PersonalData
            {
                Name = "John Doe"
            };

            log.Information("Here is {@PersonData}", personalData);

            var sv = (StructureValue)evt.Properties["PersonData"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            var literalValue = props["FullName"].LiteralValue();
            Assert.AreEqual("John Doe", literalValue);
        }

        #region LogWithName
        public class PersonalData
        {
            [LogWithName("FullName")]
            public string? Name { get; set; }
        }
        #endregion
    }
}