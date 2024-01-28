using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog.Events;
using Shouldly;

namespace Destructurama.Attributed.Tests;

[TestFixture]
public class NotLoggedIfDefaultAttributeTests
{
    private struct NotLoggedIfDefaultStruct
    {
        public int Integer { get; set; }

        public DateTime DateTime { get; set; }
    }

    private struct NotLoggedIfDefaultStructWithAttributes
    {
        [NotLoggedIfDefault]
        public int Integer { get; set; }

        [NotLoggedIfDefault]
        public DateTime DateTime { get; set; }

        public int IntegerLogged { get; set; }

        public DateTime DateTimeLogged { get; set; }
    }

    private class NotLoggedIfDefaultCustomizedDefaultLogs
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
        var customized = new NotLoggedIfDefaultCustomizedDefaultLogs();

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt!.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("String").ShouldBeFalse();
        props.ContainsKey("Integer").ShouldBeFalse();
        props.ContainsKey("NullableInteger").ShouldBeFalse();
        props.ContainsKey("Object").ShouldBeFalse();
        props.ContainsKey("DateTime").ShouldBeFalse();
        props.ContainsKey("Struct").ShouldBeFalse();

        props.ContainsKey("StringLogged").ShouldBeTrue();
        props.ContainsKey("IntegerLogged").ShouldBeTrue();
        props.ContainsKey("NullableIntegerLogged").ShouldBeTrue();
        props.ContainsKey("ObjectLogged").ShouldBeTrue();
        props.ContainsKey("DateTimeLogged").ShouldBeTrue();
        props.ContainsKey("StructLogged").ShouldBeTrue();

        props["StringLogged"].LiteralValue().ShouldBe(default(string));
        props["IntegerLogged"].LiteralValue().ShouldBe(default(int));
        props["NullableIntegerLogged"].LiteralValue().ShouldBe(default(int?));
        props["ObjectLogged"].LiteralValue().ShouldBe(default);
        props["DateTimeLogged"].LiteralValue().ShouldBe(default(DateTime));
        props["StructLogged"].LiteralValue().ShouldBe(default(NotLoggedIfDefaultStruct));

        props.ContainsKey("StructWithAttributes").ShouldBeTrue();
        props["StructWithAttributes"].ShouldBeOfType<StructureValue>();

        var structProps = ((StructureValue)props["StructWithAttributes"]).Properties.ToDictionary(p => p.Name, p => p.Value);

        structProps.ContainsKey("Integer").ShouldBeFalse();
        structProps.ContainsKey("DateTime").ShouldBeFalse();
        structProps.ContainsKey("IntegerLogged").ShouldBeTrue();
        structProps.ContainsKey("DateTimeLogged").ShouldBeTrue();
        structProps["IntegerLogged"].LiteralValue().ShouldBe(default(int));
        structProps["DateTimeLogged"].LiteralValue().ShouldBe(default(DateTime));
    }

    [Test]
    public void NotLoggedIfDefault_Initialized()
    {
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

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt!.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("String").ShouldBeTrue();
        props.ContainsKey("Integer").ShouldBeTrue();
        props.ContainsKey("NullableInteger").ShouldBeTrue();
        props.ContainsKey("Object").ShouldBeTrue();
        props.ContainsKey("DateTime").ShouldBeTrue();
        props.ContainsKey("Struct").ShouldBeTrue();
        props.ContainsKey("IntegerAsObject").ShouldBeTrue();

        props.ContainsKey("StringLogged").ShouldBeTrue();
        props.ContainsKey("IntegerLogged").ShouldBeTrue();
        props.ContainsKey("NullableIntegerLogged").ShouldBeTrue();
        props.ContainsKey("ObjectLogged").ShouldBeTrue();
        props.ContainsKey("DateTimeLogged").ShouldBeTrue();
        props.ContainsKey("StructLogged").ShouldBeTrue();

        props["String"].LiteralValue().ShouldBe("Foo");
        props["Integer"].LiteralValue().ShouldBe(10);
        props["NullableInteger"].LiteralValue().ShouldBe(5);
        props["Object"].LiteralValue().ShouldBe("Bar");
        props["DateTime"].LiteralValue().ShouldBe(dateTime);
        props["Struct"].ShouldBeOfType<StructureValue>();
        props["IntegerAsObject"].LiteralValue().ShouldBe(0);

        props["StringLogged"].LiteralValue().ShouldBe(default(string));
        props["IntegerLogged"].LiteralValue().ShouldBe(default(int));
        props["NullableIntegerLogged"].LiteralValue().ShouldBe(default(int?));
        props["ObjectLogged"].LiteralValue().ShouldBe(default);
        props["DateTimeLogged"].LiteralValue().ShouldBe(default(DateTime));
        props["StructLogged"].LiteralValue().ShouldBe(default(NotLoggedIfDefaultStruct));

        props.ContainsKey("StructWithAttributes").ShouldBeTrue();
        props["StructWithAttributes"].ShouldBeOfType<StructureValue>();

        var structProps = ((StructureValue)props["StructWithAttributes"]).Properties.ToDictionary(p => p.Name, p => p.Value);

        structProps.ContainsKey("Integer").ShouldBeTrue();
        structProps.ContainsKey("DateTime").ShouldBeTrue();
        structProps.ContainsKey("IntegerLogged").ShouldBeTrue();
        structProps.ContainsKey("DateTimeLogged").ShouldBeTrue();
        structProps["Integer"].LiteralValue().ShouldBe(20);
        structProps["DateTime"].LiteralValue().ShouldBe(dateTime);
        structProps["IntegerLogged"].LiteralValue().ShouldBe(default(int));
        structProps["DateTimeLogged"].LiteralValue().ShouldBe(default(DateTime));
    }
}


