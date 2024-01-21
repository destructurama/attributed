using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Destructurama.Attributed.Tests.Support;

public class DelegatingSink : ILogEventSink
{
    private readonly Action<LogEvent> _write;

    public DelegatingSink(Action<LogEvent> write)
    {
        _write = write ?? throw new ArgumentNullException("write");
    }

    public void Emit(LogEvent logEvent)
    {
        _write(logEvent);
    }

    public static LogEvent GetLogEvent(Action<ILogger> writeAction)
    {
        LogEvent result = null!;
        var l = new LoggerConfiguration()
            .WriteTo.Sink(new DelegatingSink(le => result = le))
            .CreateLogger();

        writeAction(l);
        return result;
    }
}
