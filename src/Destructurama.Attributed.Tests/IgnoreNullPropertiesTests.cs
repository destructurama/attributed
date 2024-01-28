using System.Collections;
using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog.Events;
using Shouldly;

namespace Destructurama.Attributed.Tests;

[TestFixture]
public class IgnoreNullPropertiesTests
{
    private struct NotLoggedIfNullStruct
    {
        public int Integer { get; set; }

        public int? NullableInteger { get; set; }

        public DateTime DateTime { get; set; }

        public DateTime? NullableDateTime { get; set; }

        public object? Object { get; set; }
    }

    private class NotLoggedIfNull
    {
        public string? String { get; set; }

        public int Integer { get; set; }

        public int? NullableInteger { get; set; }

        public object? Object { get; set; }

        public object? IntegerAsObject { get; set; }

        public DateTime DateTime { get; set; }

        public DateTime? NullableDateTime { get; set; }

        public NotLoggedIfNullStruct Struct { get; set; }

        public NotLoggedIfNullStruct? NullableStruct { get; set; }

        public NotLoggedIfNullStruct StructPartiallyInitialized { get; set; }
    }

    private class NotLoggedIfNullAttributed
    {
        [NotLoggedIfNull]
        public string? String { get; set; }

        [NotLoggedIfNull]
        public int Integer { get; set; }

        [NotLoggedIfNull]
        public int? NullableInteger { get; set; }

        [NotLoggedIfNull]
        public object? Object { get; set; }

        [NotLoggedIfNull]
        public object? IntegerAsObject { get; set; }

        [NotLoggedIfNull]
        public DateTime DateTime { get; set; }

        [NotLoggedIfNull]
        public DateTime? NullableDateTime { get; set; }

        [NotLoggedIfNull]
        public NotLoggedIfNullStruct Struct { get; set; }

        [NotLoggedIfNull]
        public NotLoggedIfNullStruct? NullableStruct { get; set; }

        [NotLoggedIfNull]
        public NotLoggedIfNullStruct StructPartiallyInitialized { get; set; }
    }

    private class AttributedWithMask
    {
        [LogMasked(ShowFirst = 3)]
        public string? String { get; set; }

        [LogMasked(ShowFirst = 3)]
        public object? Object { get; set; }
    }

    private class Dependency
    {
        public int Integer { get; set; }

        public int? NullableInteger { get; set; }
    }

    private class CustomEnumerableDestructionIgnored : IEnumerable<int>
    {
        public int Integer { get; set; }

        public Dependency? Dependency { get; set; }

        public IEnumerator<int> GetEnumerator()
        {
            yield return 1;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    /// <summary>
    /// At least one attribute from Destructurma.Attributed is enough to ignore all
    /// default properties on IEnumerable, when IgnoreNullProperties is true.
    /// </summary>
    private class CustomEnumerableAttributed : IEnumerable<int>
    {
        [NotLogged]
        public bool Dummy { get; set; }

        public int Integer { get; set; }

        public int? NullableInteger { get; set; }

        public Dependency? Dependency { get; set; }

        public IEnumerator<int> GetEnumerator()
        {
            yield return 1;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [SetUp]
    public void SetUp()
    {
        AttributedDestructuringPolicy.Clear();
    }

    [TearDown]
    public void TearDown()
    {
        AttributedDestructuringPolicy.Clear();
    }

    [Test]
    public void NotLoggedIfNull_Uninitialized()
    {
        var customized = new NotLoggedIfNull();

        var evt = DelegatingSink.Execute(customized, configure: x => x.IgnoreNullProperties = true);

        var sv = (StructureValue)evt!.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("Integer").ShouldBeTrue();
        props.ContainsKey("DateTime").ShouldBeTrue();
        props.ContainsKey("Struct").ShouldBeTrue();
        props.ContainsKey("StructPartiallyInitialized").ShouldBeTrue();

        props.ContainsKey("String").ShouldBeFalse();
        props.ContainsKey("NullableInteger").ShouldBeFalse();
        props.ContainsKey("IntegerAsObject").ShouldBeFalse();
        props.ContainsKey("Object").ShouldBeFalse();
        props.ContainsKey("NullableDateTime").ShouldBeFalse();
        props.ContainsKey("NullableStruct").ShouldBeFalse();
    }

    [Test]
    public void NotLoggedIfNull_Initialized()
    {
        var dateTime = DateTime.UtcNow;
        var theStruct = new NotLoggedIfNullStruct
        {
            Integer = 20,
            NullableInteger = 15,
            DateTime = dateTime,
            NullableDateTime = dateTime,
            Object = "Bar",
        };

        var theStructPartiallyUnitialized = new NotLoggedIfNullStruct
        {
            Integer = 20,
            NullableInteger = 15,
            DateTime = dateTime,
            NullableDateTime = dateTime,
            Object = null,
        };

        var customized = new NotLoggedIfNull
        {
            String = "Foo",
            Integer = 10,
            NullableInteger = 5,
            Object = "Bar",
            IntegerAsObject = 0,
            DateTime = dateTime,
            NullableDateTime = dateTime,
            Struct = theStruct,
            NullableStruct = theStruct,
            StructPartiallyInitialized = theStructPartiallyUnitialized,
        };

        var evt = DelegatingSink.Execute(customized, configure: x => x.IgnoreNullProperties = true);

        var sv = (StructureValue)evt!.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("String").ShouldBeTrue();
        props.ContainsKey("Integer").ShouldBeTrue();
        props.ContainsKey("NullableInteger").ShouldBeTrue();
        props.ContainsKey("Object").ShouldBeTrue();
        props.ContainsKey("IntegerAsObject").ShouldBeTrue();
        props.ContainsKey("DateTime").ShouldBeTrue();
        props.ContainsKey("NullableDateTime").ShouldBeTrue();
        props.ContainsKey("Struct").ShouldBeTrue();
        props.ContainsKey("NullableStruct").ShouldBeTrue();
        props.ContainsKey("StructPartiallyInitialized").ShouldBeTrue();

        props["String"].LiteralValue().ShouldBe("Foo");
        props["Integer"].LiteralValue().ShouldBe(10);
        props["NullableInteger"].LiteralValue().ShouldBe(5);
        props["Object"].LiteralValue().ShouldBe("Bar");
        props["IntegerAsObject"].LiteralValue().ShouldBe(0);
        props["DateTime"].LiteralValue().ShouldBe(dateTime);
        props["NullableDateTime"].LiteralValue().ShouldBe(dateTime);
        props["Struct"].ShouldBeOfType<StructureValue>();
        props["NullableStruct"].ShouldBeOfType<StructureValue>();
        props["StructPartiallyInitialized"].ShouldBeOfType<StructureValue>();

        var structProps = ((StructureValue)props["Struct"]).Properties.ToDictionary(p => p.Name, p => p.Value);

        structProps.ContainsKey("Integer").ShouldBeTrue();
        structProps.ContainsKey("NullableInteger").ShouldBeTrue();
        structProps.ContainsKey("DateTime").ShouldBeTrue();
        structProps.ContainsKey("NullableDateTime").ShouldBeTrue();
        structProps.ContainsKey("Object").ShouldBeTrue();
        structProps["Integer"].LiteralValue().ShouldBe(20);
        structProps["NullableInteger"].LiteralValue().ShouldBe(15);
        structProps["DateTime"].LiteralValue().ShouldBe(dateTime);
        structProps["NullableDateTime"].LiteralValue().ShouldBe(dateTime);
        structProps["Object"].LiteralValue().ShouldBe("Bar");

        var partiallyItitializedProps = ((StructureValue)props["StructPartiallyInitialized"]).Properties.ToDictionary(p => p.Name, p => p.Value);

        partiallyItitializedProps.ContainsKey("Integer").ShouldBeTrue();
        partiallyItitializedProps.ContainsKey("NullableInteger").ShouldBeTrue();
        partiallyItitializedProps.ContainsKey("DateTime").ShouldBeTrue();
        partiallyItitializedProps.ContainsKey("NullableDateTime").ShouldBeTrue();
        partiallyItitializedProps.ContainsKey("Object").ShouldBeFalse();
        partiallyItitializedProps["Integer"].LiteralValue().ShouldBe(20);
        partiallyItitializedProps["NullableInteger"].LiteralValue().ShouldBe(15);
        partiallyItitializedProps["DateTime"].LiteralValue().ShouldBe(dateTime);
        partiallyItitializedProps["NullableDateTime"].LiteralValue().ShouldBe(dateTime);
    }

    [Test]
    public void WithMask_NotLoggedIfNull_Uninitialized()
    {
        var customized = new AttributedWithMask();

        var evt = DelegatingSink.Execute(customized, configure: x => x.IgnoreNullProperties = true);

        var sv = (StructureValue)evt!.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("String").ShouldBeFalse();
        props.ContainsKey("Object").ShouldBeFalse();
    }

    [Test]
    public void WithMask_NotLoggedIfNull_Initialized()
    {
        var customized = new AttributedWithMask
        {
            String = "Foo[Masked]",
            Object = "Bar[Masked]",
        };

        var evt = DelegatingSink.Execute(customized, configure: x => x.IgnoreNullProperties = true);

        var sv = (StructureValue)evt!.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("String").ShouldBeTrue();
        props.ContainsKey("Object").ShouldBeTrue();

        props["String"].LiteralValue().ShouldBe("Foo***");
        props["Object"].LiteralValue().ShouldBe("Bar***");
    }

    [Test]
    public void EnumerableIgnored()
    {
        var customized = new CustomEnumerableDestructionIgnored()
        {
            Integer = 0,
            Dependency = new Dependency
            {
                Integer = 0,
            }
        };

        var evt = DelegatingSink.Execute(customized, configure: x => x.IgnoreNullProperties = true);

        var sv = evt!.Properties["Customized"];
        sv.ShouldBeOfType<SequenceValue>();
    }

    [Test]
    public void EnumerableDestructedAsStruct()
    {
        var customized = new CustomEnumerableAttributed
        {
            Integer = 0,
            NullableInteger = null,
            Dependency = new Dependency
            {
                Integer = 0,
            },
        };

        var evt = DelegatingSink.Execute(customized, configure: x => x.IgnoreNullProperties = true);

        var sv = (StructureValue)evt!.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("Integer").ShouldBeTrue();
        props.ContainsKey("NullableInteger").ShouldBeFalse();
        props.ContainsKey("Dependency").ShouldBeTrue();

        var dependencyProps = ((StructureValue)props["Dependency"]).Properties.ToDictionary(p => p.Name, p => p.Value);

        dependencyProps.ContainsKey("Integer").ShouldBeTrue();
        dependencyProps.ContainsKey("NullableInteger").ShouldBeFalse();
    }

    [Test]
    public void NotLoggedIfNullAttribute_Uninitialized()
    {
        var customized = new NotLoggedIfNullAttributed();

        var evt = DelegatingSink.Execute(customized, configure: x => x.IgnoreNullProperties = false);

        var sv = (StructureValue)evt!.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("Integer").ShouldBeTrue();
        props.ContainsKey("DateTime").ShouldBeTrue();
        props.ContainsKey("Struct").ShouldBeTrue();
        props.ContainsKey("StructPartiallyInitialized").ShouldBeTrue();

        props.ContainsKey("String").ShouldBeFalse();
        props.ContainsKey("NullableInteger").ShouldBeFalse();
        props.ContainsKey("IntegerAsObject").ShouldBeFalse();
        props.ContainsKey("Object").ShouldBeFalse();
        props.ContainsKey("NullableDateTime").ShouldBeFalse();
        props.ContainsKey("NullableStruct").ShouldBeFalse();
    }

    [Test]
    public void NotLoggedIfNullAttribute_Initialized()
    {
        var dateTime = DateTime.UtcNow;
        var theStruct = new NotLoggedIfNullStruct
        {
            Integer = 20,
            NullableInteger = 15,
            DateTime = dateTime,
            NullableDateTime = dateTime,
            Object = "Bar",
        };

        var theStructPartiallyUnitialized = new NotLoggedIfNullStruct
        {
            Integer = 20,
            NullableInteger = 15,
            DateTime = dateTime,
            NullableDateTime = dateTime,
            Object = null,
        };

        var customized = new NotLoggedIfNullAttributed
        {
            String = "Foo",
            Integer = 10,
            NullableInteger = 5,
            Object = "Bar",
            IntegerAsObject = 0,
            DateTime = dateTime,
            NullableDateTime = dateTime,
            Struct = theStruct,
            NullableStruct = theStruct,
            StructPartiallyInitialized = theStructPartiallyUnitialized,
        };

        var evt = DelegatingSink.Execute(customized, configure: x => x.IgnoreNullProperties = false);

        var sv = (StructureValue)evt!.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("String").ShouldBeTrue();
        props.ContainsKey("Integer").ShouldBeTrue();
        props.ContainsKey("NullableInteger").ShouldBeTrue();
        props.ContainsKey("Object").ShouldBeTrue();
        props.ContainsKey("IntegerAsObject").ShouldBeTrue();
        props.ContainsKey("DateTime").ShouldBeTrue();
        props.ContainsKey("NullableDateTime").ShouldBeTrue();
        props.ContainsKey("Struct").ShouldBeTrue();
        props.ContainsKey("NullableStruct").ShouldBeTrue();
        props.ContainsKey("StructPartiallyInitialized").ShouldBeTrue();

        props["String"].LiteralValue().ShouldBe("Foo");
        props["Integer"].LiteralValue().ShouldBe(10);
        props["NullableInteger"].LiteralValue().ShouldBe(5);
        props["Object"].LiteralValue().ShouldBe("Bar");
        props["IntegerAsObject"].LiteralValue().ShouldBe(0);
        props["DateTime"].LiteralValue().ShouldBe(dateTime);
        props["NullableDateTime"].LiteralValue().ShouldBe(dateTime);
        props["Struct"].ShouldBeOfType<StructureValue>();
        props["NullableStruct"].ShouldBeOfType<StructureValue>();
        props["StructPartiallyInitialized"].ShouldBeOfType<StructureValue>();

        var structProps = ((StructureValue)props["Struct"]).Properties.ToDictionary(p => p.Name, p => p.Value);

        structProps.ContainsKey("Integer").ShouldBeTrue();
        structProps.ContainsKey("NullableInteger").ShouldBeTrue();
        structProps.ContainsKey("DateTime").ShouldBeTrue();
        structProps.ContainsKey("NullableDateTime").ShouldBeTrue();
        structProps.ContainsKey("Object").ShouldBeTrue();
        structProps["Integer"].LiteralValue().ShouldBe(20);
        structProps["NullableInteger"].LiteralValue().ShouldBe(15);
        structProps["DateTime"].LiteralValue().ShouldBe(dateTime);
        structProps["NullableDateTime"].LiteralValue().ShouldBe(dateTime);
        structProps["Object"].LiteralValue().ShouldBe("Bar");

        var partiallyItitializedProps = ((StructureValue)props["StructPartiallyInitialized"]).Properties.ToDictionary(p => p.Name, p => p.Value);

        partiallyItitializedProps.ContainsKey("Integer").ShouldBeTrue();
        partiallyItitializedProps.ContainsKey("NullableInteger").ShouldBeTrue();
        partiallyItitializedProps.ContainsKey("DateTime").ShouldBeTrue();
        partiallyItitializedProps.ContainsKey("NullableDateTime").ShouldBeTrue();
        partiallyItitializedProps.ContainsKey("Object").ShouldBeTrue();
        partiallyItitializedProps["Integer"].LiteralValue().ShouldBe(20);
        partiallyItitializedProps["NullableInteger"].LiteralValue().ShouldBe(15);
        partiallyItitializedProps["DateTime"].LiteralValue().ShouldBe(dateTime);
        partiallyItitializedProps["NullableDateTime"].LiteralValue().ShouldBe(dateTime);
    }
}


