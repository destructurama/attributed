using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace Destructurama.Attributed.Tests
{
    [TestFixture]
    public class NotLoggedIfDefaultAttributeTests
    {
        struct NotLoggedIfDefaultStruct
        {
            public int Integer { get; set; }

            public DateTime DateTime { get; set; }
        }

        struct NotLoggedIfDefaultStructWithAttributes
        {
            [NotLoggedIfDefault]
            public int Integer { get; set; }

            [NotLoggedIfDefault]
            public DateTime DateTime { get; set; }

            public int IntegerLogged { get; set; }

            public DateTime DateTimeLogged { get; set; }
        }

        class NotLoggedIfDefaultCustomizedDefaultLogs
        {
            [NotLoggedIfDefault]
            public string? String { get; set; }

            [NotLoggedIfDefault]
            public int Integer { get; set; }

            [NotLoggedIfDefault]
            public int? NullableInteger { get; set; }

            [NotLoggedIfDefault]
            public object? Object { get; set; }

            [NotLoggedIfDefault]
            public object? IntegerAsObject { get; set; }

            [NotLoggedIfDefault]
            public DateTime DateTime { get; set; }

            [NotLoggedIfDefault]
            public NotLoggedIfDefaultStruct Struct { get; set; }

            public NotLoggedIfDefaultStructWithAttributes StructWithAttributes { get; set; }

            public string? StringLogged { get; set; }

            public int IntegerLogged { get; set; }

            public int? NullableIntegerLogged { get; set; }

            public object? ObjectLogged { get; set; }

            public DateTime DateTimeLogged { get; set; }

            [LogAsScalar]
            public NotLoggedIfDefaultStruct StructLogged { get; set; }
        }


        [Test]
        public void NotLoggedIfDefault_Uninitialized()
        {
            LogEvent? evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new NotLoggedIfDefaultCustomizedDefaultLogs();

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt!.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsFalse(props.ContainsKey("String"));
            Assert.IsFalse(props.ContainsKey("Integer"));
            Assert.IsFalse(props.ContainsKey("NullableInteger"));
            Assert.IsFalse(props.ContainsKey("Object"));
            Assert.IsFalse(props.ContainsKey("DateTime"));
            Assert.IsFalse(props.ContainsKey("Struct"));

            Assert.IsTrue(props.ContainsKey("StringLogged"));
            Assert.IsTrue(props.ContainsKey("IntegerLogged"));
            Assert.IsTrue(props.ContainsKey("NullableIntegerLogged"));
            Assert.IsTrue(props.ContainsKey("ObjectLogged"));
            Assert.IsTrue(props.ContainsKey("DateTimeLogged"));
            Assert.IsTrue(props.ContainsKey("StructLogged"));

            Assert.AreEqual(default(string), props["StringLogged"].LiteralValue());
            Assert.AreEqual(default(int), props["IntegerLogged"].LiteralValue());
            Assert.AreEqual(default(int?), props["NullableIntegerLogged"].LiteralValue());
            Assert.AreEqual(default, props["ObjectLogged"].LiteralValue());
            Assert.AreEqual(default(DateTime), props["DateTimeLogged"].LiteralValue());
            Assert.AreEqual(default(NotLoggedIfDefaultStruct), props["StructLogged"].LiteralValue());

            Assert.IsTrue(props.ContainsKey("StructWithAttributes"));
            Assert.IsTrue(props["StructWithAttributes"] is StructureValue);

            var structProps = ((StructureValue)props["StructWithAttributes"]).Properties
                .ToDictionary(p => p.Name, p => p.Value);

            Assert.IsFalse(structProps.ContainsKey("Integer"));
            Assert.IsFalse(structProps.ContainsKey("DateTime"));
            Assert.IsTrue(structProps.ContainsKey("IntegerLogged"));
            Assert.IsTrue(structProps.ContainsKey("DateTimeLogged"));
            Assert.AreEqual(default(int), structProps["IntegerLogged"].LiteralValue());
            Assert.AreEqual(default(DateTime), structProps["DateTimeLogged"].LiteralValue());
        }

        [Test]
        public void NotLoggedIfDefault_Initialized()
        {
            LogEvent? evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var dateTime = DateTime.UtcNow;
            var theStruct = new NotLoggedIfDefaultStruct
            {
                Integer = 20,
                DateTime = dateTime
            };

            var attributedStruct = new NotLoggedIfDefaultStructWithAttributes
            {
                Integer = 20,
                DateTime = dateTime
            };

            var customized = new NotLoggedIfDefaultCustomizedDefaultLogs
            {
                String = "Foo",
                Integer = 10,
                NullableInteger = 5,
                Object = "Bar",
                DateTime = dateTime,
                Struct = theStruct,
                StructWithAttributes = attributedStruct,
                IntegerAsObject = 0
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt!.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("String"));
            Assert.IsTrue(props.ContainsKey("Integer"));
            Assert.IsTrue(props.ContainsKey("NullableInteger"));
            Assert.IsTrue(props.ContainsKey("Object"));
            Assert.IsTrue(props.ContainsKey("DateTime"));
            Assert.IsTrue(props.ContainsKey("Struct"));
            Assert.IsTrue(props.ContainsKey("IntegerAsObject"));

            Assert.IsTrue(props.ContainsKey("StringLogged"));
            Assert.IsTrue(props.ContainsKey("IntegerLogged"));
            Assert.IsTrue(props.ContainsKey("NullableIntegerLogged"));
            Assert.IsTrue(props.ContainsKey("ObjectLogged"));
            Assert.IsTrue(props.ContainsKey("DateTimeLogged"));
            Assert.IsTrue(props.ContainsKey("StructLogged"));

            Assert.AreEqual("Foo", props["String"].LiteralValue());
            Assert.AreEqual(10, props["Integer"].LiteralValue());
            Assert.AreEqual(5, props["NullableInteger"].LiteralValue());
            Assert.AreEqual("Bar", props["Object"].LiteralValue());
            Assert.AreEqual(dateTime, props["DateTime"].LiteralValue());
            Assert.IsInstanceOf<StructureValue>(props["Struct"]);
            Assert.AreEqual(0, props["IntegerAsObject"].LiteralValue());

            Assert.AreEqual(default(string), props["StringLogged"].LiteralValue());
            Assert.AreEqual(default(int), props["IntegerLogged"].LiteralValue());
            Assert.AreEqual(default(int?), props["NullableIntegerLogged"].LiteralValue());
            Assert.AreEqual(default, props["ObjectLogged"].LiteralValue());
            Assert.AreEqual(default(DateTime), props["DateTimeLogged"].LiteralValue());
            Assert.AreEqual(default(NotLoggedIfDefaultStruct), props["StructLogged"].LiteralValue());

            Assert.IsTrue(props.ContainsKey("StructWithAttributes"));
            Assert.IsTrue(props["StructWithAttributes"] is StructureValue);

            var structProps = ((StructureValue)props["StructWithAttributes"]).Properties
                .ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(structProps.ContainsKey("Integer"));
            Assert.IsTrue(structProps.ContainsKey("DateTime"));
            Assert.IsTrue(structProps.ContainsKey("IntegerLogged"));
            Assert.IsTrue(structProps.ContainsKey("DateTimeLogged"));
            Assert.AreEqual(20, structProps["Integer"].LiteralValue());
            Assert.AreEqual(dateTime, structProps["DateTime"].LiteralValue());
            Assert.AreEqual(default(int), structProps["IntegerLogged"].LiteralValue());
            Assert.AreEqual(default(DateTime), structProps["DateTimeLogged"].LiteralValue());
        }
    }
}


