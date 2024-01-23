using System.ComponentModel.DataAnnotations;
using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Shouldly;

namespace Destructurama.Attributed.Tests;

[TestFixture]
public class MetadataTypeTests
{
    [Test]
    public void MetadataType_Should_Be_Respected()
    {
        LogEvent evt = null!;

        var log = new LoggerConfiguration()
            .Destructure.UsingAttributes()
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        var customized = new Dto
        {
            Private = "secret",
            Public = "not_Secret"
        };

        log.Information("Here is {@Customized}", customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.Count.ShouldBe(1);
        props["Public"].LiteralValue().ShouldBe("not_Secret");
    }

    [MetadataType(typeof(DtoMetadata))]
    public partial class Dto
    {
        public string Private { get; set; }

        public string Public { get; set; }
    }

    internal class DtoMetadata
    {
        [NotLogged]
        public object Private { get; set; }
    }
}
