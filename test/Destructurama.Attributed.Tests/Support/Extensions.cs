using Serilog.Events;

namespace Destructurama.Attributed.Tests.Support
{
    public static class Extensions
    {
        public static object? LiteralValue(this LogEventPropertyValue @this) =>
            ((ScalarValue)@this).Value;
    }
}
