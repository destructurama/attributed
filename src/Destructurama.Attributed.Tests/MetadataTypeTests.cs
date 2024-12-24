using System.ComponentModel.DataAnnotations;
using Destructurama.Attributed.Tests.Support;
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

}
