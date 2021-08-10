using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using System.Linq;

namespace Destructurama.Attributed.Tests
{

    public class PersonalData
    {
        [LogWithName("FullName")]
        public string Name { get; set; }
    }


    [TestFixture]
    public class LogWithNameAttributedTests
    {
        [Test]
        public void AttributesAreConsultedWhenDestructuring()
        {
            LogEvent evt = null;
            
            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var personalData = new PersonalData
            {
                Name = "John Doe"
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

            log.Information("Here is {@PersonData}", personalData);

            var sv = (StructureValue)evt.Properties["PersonData"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.AreEqual("John Doe", props["FullName"].LiteralValue());
        }
    }
}
