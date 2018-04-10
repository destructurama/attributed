using System;
using Serilog.Events;

namespace Destructurama.Attributed
{
    public abstract class DestructuringAttribute : Attribute
    {
        protected internal abstract bool TryCreateLogEventProperty(string name, object value, out LogEventProperty property);
    }
}