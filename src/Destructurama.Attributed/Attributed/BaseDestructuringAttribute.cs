using System;
using Serilog.Events;

namespace Destructurama.Attributed
{
    public abstract class BaseDestructuringAttribute : Attribute
    {
        internal abstract LogEventPropertyValue CreateLogEventPropertyValue(object value);
    }
}