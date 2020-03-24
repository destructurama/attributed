using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;

namespace Destructurama.Attributed.Tests
{
    class NotLoggedIfDefaultCustomizedDefaultLogs
    {
        [NotLoggedIfDefault]
        public string String { get; set; }

        [NotLoggedIfDefault]
        public int Integer { get; set; }

        [NotLoggedIfDefault]
        public int? NullableInteger { get; set; }

        [NotLoggedIfDefault]
        public object Object { get; set; }

        [NotLoggedIfDefault]
        public DateTime DateTime { get; set; }

        public string StringLogged { get; set; }

        public int IntegerLogged { get; set; }

        public int? NullableIntegerLogged { get; set; }

        public object ObjectLogged { get; set; }

        public DateTime DateTimeLogged { get; set; }
    }

    [TestFixture]
    public class NotLoggedIfDefaultAttributeTests
    {
        [Test]
        public void NotLoggedIfDefault_Uninitialized()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new NotLoggedIfDefaultCustomizedDefaultLogs();

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsFalse(props.ContainsKey("String"));
            Assert.IsFalse(props.ContainsKey("Integer"));
            Assert.IsFalse(props.ContainsKey("NullableInteger"));
            Assert.IsFalse(props.ContainsKey("Object"));
            Assert.IsFalse(props.ContainsKey("DateTime"));

            Assert.IsTrue(props.ContainsKey("StringLogged"));
            Assert.IsTrue(props.ContainsKey("IntegerLogged"));
            Assert.IsTrue(props.ContainsKey("NullableIntegerLogged"));
            Assert.IsTrue(props.ContainsKey("ObjectLogged"));
            Assert.IsTrue(props.ContainsKey("DateTimeLogged"));

            Assert.AreEqual(default(string), props["StringLogged"].LiteralValue());
            Assert.AreEqual(default(int), props["IntegerLogged"].LiteralValue());
            Assert.AreEqual(default(int?), props["NullableIntegerLogged"].LiteralValue());
            Assert.AreEqual(default, props["ObjectLogged"].LiteralValue());
            Assert.AreEqual(default(DateTime), props["DateTimeLogged"].LiteralValue());
        }

        [Test]
        public void NotLoggedIfDefault_Initialized()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var dateTime = DateTime.UtcNow;

            var customized = new NotLoggedIfDefaultCustomizedDefaultLogs
            {
                String = "Foo",
                Integer = 10,
                NullableInteger = 5,
                Object = "Bar",
                DateTime = dateTime
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("String"));
            Assert.IsTrue(props.ContainsKey("Integer"));
            Assert.IsTrue(props.ContainsKey("NullableInteger"));
            Assert.IsTrue(props.ContainsKey("Object"));
            Assert.IsTrue(props.ContainsKey("DateTime"));

            Assert.IsTrue(props.ContainsKey("StringLogged"));
            Assert.IsTrue(props.ContainsKey("IntegerLogged"));
            Assert.IsTrue(props.ContainsKey("NullableIntegerLogged"));
            Assert.IsTrue(props.ContainsKey("ObjectLogged"));
            Assert.IsTrue(props.ContainsKey("DateTimeLogged"));

            Assert.AreEqual("Foo", props["String"].LiteralValue());
            Assert.AreEqual(10, props["Integer"].LiteralValue());
            Assert.AreEqual(5, props["NullableInteger"].LiteralValue());
            Assert.AreEqual("Bar", props["Object"].LiteralValue());
            Assert.AreEqual(dateTime, props["DateTime"].LiteralValue());

            Assert.AreEqual(default(string), props["StringLogged"].LiteralValue());
            Assert.AreEqual(default(int), props["IntegerLogged"].LiteralValue());
            Assert.AreEqual(default(int?), props["NullableIntegerLogged"].LiteralValue());
            Assert.AreEqual(default, props["ObjectLogged"].LiteralValue());
            Assert.AreEqual(default(DateTime), props["DateTimeLogged"].LiteralValue());
        }


    }
}


