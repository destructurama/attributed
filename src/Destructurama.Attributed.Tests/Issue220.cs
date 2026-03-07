using System.Diagnostics;
using Destructurama.Attributed.Tests.Support;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Shouldly;

namespace Destructurama.Attributed.Tests;

// https://github.com/destructurama/attributed/issues/220
public class Issue220
{
    [Test]
    public void Destructuring_Should_Be_Configured_Via_Json_File()
    {
        Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("issue220.json", optional: false, reloadOnChange: true)
            .Build();

        LogEvent evt = null!;

        var log = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Create an object with null properties
        var testObject = new Person
        {
            Name = "John Doe",
            Age = 30,
            Email = null,  // This should be ignored in the log
            Phone = null   // This should be ignored in the log
        };

        log.Information("Here is {@Customized}", testObject);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.Count.ShouldBe(2);
        props["Name"].LiteralValue().ShouldBe("John Doe");
        props["Age"].LiteralValue().ShouldBe(30);
    }

    internal class Person
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
