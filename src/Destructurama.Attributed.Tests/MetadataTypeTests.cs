using System.ComponentModel.DataAnnotations;
using Destructurama.Attributed.Tests.Support;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Serilog.Events;
using Shouldly;

namespace Destructurama.Attributed.Tests;

[TestFixture]
public class MetadataTypeTests
{
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
    public void MetadataType_Should_Not_Be_Respected()
    {
        var customized = new Dto
        {
            Private = "secret",
            Public = "not_Secret"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.Count.ShouldBe(2);
        props["Public"].LiteralValue().ShouldBe("not_Secret");
        props["Private"].LiteralValue().ShouldBe("secret");
    }

    [Test]
    public void MetadataType_Should_Be_Respected()
    {
        var customized = new Dto
        {
            Private = "secret",
            Public = "not_Secret"
        };

        var evt = DelegatingSink.Execute(customized, configure: opt => opt.RespectMetadataTypeAttribute = true);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.Count.ShouldBe(1);
        props["Public"].LiteralValue().ShouldBe("not_Secret");
    }

    [Test]
    public void MetadataTypeWithDerived_Should_Be_Respected()
    {
        var customized = new DtoWithDerived
        {
            Private = "secret",
            Public = "not_Secret"
        };

        var evt = DelegatingSink.Execute(customized, configure: opt => opt.RespectMetadataTypeAttribute = true);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.Count.ShouldBe(1);
        props["Public"].LiteralValue().ShouldBe("not_Secret");
    }

    [Test]
    public void WithMask_NotLoggedIfNull_Initialized()
    {
        var customized = new AttributedWithMask
        {
            String = "Foo[Masked]",
            Object = "Bar[Masked]",
        };

        var evt = DelegatingSink.Execute(customized, configure: x =>
        {
            x.IgnoreNullProperties = true;
            x.RespectMetadataTypeAttribute = true;
        });

        var sv = (StructureValue)evt!.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("String").ShouldBeTrue();
        props.ContainsKey("Object").ShouldBeTrue();

        props["String"].LiteralValue().ShouldBe("Foo***");
        props["Object"].LiteralValue().ShouldBe("Bar***");
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
            Ignored2 = "Hello, there again",
            ScalarAnyway = new(),
            AuthData = new()
            {
                Username = "This is a username",
                Password = "This is a password"
            }
        };

        var evt = DelegatingSink.Execute(customized, configure: opt =>
        {
            opt.RespectLogPropertyIgnoreAttribute = true;
            opt.RespectMetadataTypeAttribute = true;
        });

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ShouldNotContainKey("Ignored");
        props.ShouldNotContainKey("Ignored2");

        props["ImmutableScalar"].LiteralValue().ShouldBeOfType<ImmutableScalar>();
        props["MutableScalar"].LiteralValue().ShouldBe(new MutableScalar().ToString());
        props["NotAScalar"].ShouldBeOfType<StructureValue>();
        props.ContainsKey("Ignored").ShouldBeFalse();
        props["ScalarAnyway"].LiteralValue().ShouldBeOfType<NotAScalar>();
        props["Struct1"].LiteralValue().ShouldBeOfType<Struct1>();
        props["Struct2"].LiteralValue().ShouldBeOfType<Struct2>();
        props["StructReturningNull"].LiteralValue().ShouldBeNull();
        props["StructNull"].LiteralValue().ShouldBeNull();

        var str = sv.ToString();
        str.Contains("This is a username").ShouldBeTrue();
        str.Contains("This is a password").ShouldBeFalse();
    }
    [Test]
    public void AttributesAreConsultedWhenDestructuringWithMeta()
    {
        var customized = new CustomizedWithMeta
        {
            ImmutableScalar = new(),
            MutableScalar = new(),
            NotAScalar = new(),
            Ignored = "Hello, there",
            Ignored2 = "Hello, there again",
            ScalarAnyway = new(),
            AuthData = new()
            {
                Username = "This is a username",
                Password = "This is a password"
            }
        };

        var evt = DelegatingSink.Execute(customized, configure: opt =>
        {
            opt.RespectLogPropertyIgnoreAttribute = true;
            opt.RespectMetadataTypeAttribute = true;
        });

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ShouldNotContainKey("Ignored");
        props.ShouldNotContainKey("Ignored2");

        props["ImmutableScalar"].LiteralValue().ShouldBeOfType<ImmutableScalar>();
        props["MutableScalar"].LiteralValue().ShouldBe(new MutableScalar().ToString());
        props["NotAScalar"].ShouldBeOfType<StructureValue>();
        props.ContainsKey("Ignored").ShouldBeFalse();
        props["ScalarAnyway"].LiteralValue().ShouldBeOfType<NotAScalar>();
        props["Struct1"].LiteralValue().ShouldBeOfType<Struct1>();
        props["Struct2"].LiteralValue().ShouldBeOfType<Struct2>();
        props["StructReturningNull"].LiteralValue().ShouldBeNull();
        props["StructNull"].LiteralValue().ShouldBeNull();

        var str = sv.ToString();
        str.Contains("This is a username").ShouldBeTrue();
        str.Contains("This is a password").ShouldBeFalse();
    }

    [Test]
    public void Private_Property_Should_Be_Handled()
    {
        var customized = new ClassWithPrivateProperty();

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        sv.Properties.Count.ShouldBe(0);
    }
    #region Simple Metadata
    /// <summary>
    /// Simple Metadata Sample
    /// </summary>
    [MetadataType(typeof(DtoMetadata))]
    private partial class Dto
    {
        public string Private { get; set; }

        public string Public { get; set; }
    }

    private class DtoMetadata
    {
        [NotLogged]
        public object Private { get; set; }
    }
    #endregion

    #region Metadata with derived subclass
    /// <summary>
    /// Metadata Sample with derived subclass
    /// </summary>
    [MetadataType(typeof(DtoMetadataDerived))]
    private partial class DtoWithDerived
    {
        public string Private { get; set; }

        public string Public { get; set; }
    }

    private class DtoMetadataBase
    {
        public object Public { get; set; }
    }

    private class DtoMetadataDerived : DtoMetadataBase
    {
        [NotLogged]
        public object Private { get; set; }
    }
    #endregion

    #region Attributed With Mask in MetadataType
    [MetadataType(typeof(AttributedWithMaskMetaData))]
    private class AttributedWithMask
    {
        public string? String { get; set; }

        public object? Object { get; set; }
    }

    private class AttributedWithMaskMetaData
    {
        [LogMasked(ShowFirst = 3)]
        public object String { get; set; }

        [LogMasked(ShowFirst = 3)]
        public object Object { get; set; }
    }
    #endregion

    #region All Attributes visited 
    /// <summary>
    /// Attribute on class in Metadatatype
    /// </summary>
    [LogAsScalar]
    public class ImmutableScalarMeta
    {
        public ImmutableScalarMeta()
        {
        }
    }
    [MetadataType(typeof(ImmutableScalarMeta))]
    public class ImmutableScalar
    {
    }
    [LogAsScalar(isMutable: true)]
    public class MutableScalarMeta
    {
    }

    [MetadataType(typeof(MutableScalarMeta))]
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

        [LogPropertyIgnore]
        public string? Ignored2 { get; set; }

        [LogAsScalar]
        public NotAScalar? ScalarAnyway { get; set; }

        public UserAuthData? AuthData { get; set; }

        [LogAsScalar]
        public Struct1 Struct1 { get; set; }

        public Struct2 Struct2 { get; set; }

        [LogAsScalar(isMutable: true)]
        public StructReturningNull StructReturningNull { get; set; }

        [LogAsScalar(isMutable: true)]
        public StructReturningNull? StructNull { get; set; }
    }
    [MetadataType(typeof(CustomizedMeta))]
    public class CustomizedWithMeta
    {
        public ImmutableScalar? ImmutableScalar { get; set; }
        public MutableScalar? MutableScalar { get; set; }
        public NotAScalar? NotAScalar { get; set; }
        public string? Ignored { get; set; }
        public string? Ignored2 { get; set; }
        public NotAScalar? ScalarAnyway { get; set; }
        public UserAuthData? AuthData { get; set; }
        public Struct1 Struct1 { get; set; }
        public Struct2 Struct2 { get; set; }
        public StructReturningNull StructReturningNull { get; set; }
        public StructReturningNull? StructNull { get; set; }
    }

    public class CustomizedMeta
    {
        public ImmutableScalar? ImmutableScalar { get; set; }
        public MutableScalar? MutableScalar { get; set; }
        public NotAScalar? NotAScalar { get; set; }

        [NotLogged]
        public object Ignored { get; set; }

        [LogPropertyIgnore]
        public object Ignored2 { get; set; }

        [LogAsScalar]
        public object ScalarAnyway { get; set; }
        public UserAuthData? AuthData { get; set; }

        [LogAsScalar]
        public object Struct1 { get; set; }

        public object Struct2 { get; set; }

        [LogAsScalar(isMutable: true)]
        public object StructReturningNull { get; set; }

        [LogAsScalar(isMutable: true)]
        public object StructNull { get; set; }
    }

    public class UserAuthDataMeta
    {
        public object Username { get; set; }

        [NotLogged]
        public object Password { get; set; }
    }
    [MetadataType(typeof(UserAuthDataMeta))]
    public class UserAuthData
    {
        public string? Username { get; set; }
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

    public struct StructReturningNull
    {
        public int SomeProperty { get; set; }
        public override string ToString() => null!;
    }
    #endregion

    #region Private
    public class ClassWithPrivatePropertyMeta
    {
        [LogMasked]
        private object Name { get; set; }
    }
    [MetadataType(typeof(ClassWithPrivatePropertyMeta))]
    public class ClassWithPrivateProperty
    {
        private string? Name { get; set; } = "Tom";
    }
    #endregion
}
