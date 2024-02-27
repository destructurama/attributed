using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Destructurama.Attributed.Tests.Support;

internal sealed class DelegatingSink : ILogEventSink
{
    private readonly Action<LogEvent> _write;

    public DelegatingSink(Action<LogEvent> write)
    {
        _write = write ?? throw new ArgumentNullException("write");
    }

    public void Emit(LogEvent logEvent) => _write(logEvent);

    public static LogEvent Execute(object obj, string messageTemplate = "Here is {@Customized}", Action<AttributedDestructuringPolicyOptions>? configure = null)
    {
        LogEvent evt = null!;

        var cfg = new LoggerConfiguration();
        cfg = configure == null ? cfg.Destructure.UsingAttributes() : cfg.Destructure.UsingAttributes(configure);
        cfg = cfg.WriteTo.Sink(new DelegatingSink(e => evt = e));

        var log = cfg.CreateLogger();
        log.Information(messageTemplate, obj);

        return evt;
    }
}
