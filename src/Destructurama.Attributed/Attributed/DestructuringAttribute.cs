using System;
using Serilog.Events;

namespace Destructurama.Attributed
{
    /// <summary>
    /// The base class for all destructuring attributes.
    /// Inherit from this class to extend this library.
    /// </summary>
    public abstract class DestructuringAttribute : Attribute
    {
        protected internal abstract bool TryCreateLogEventProperty(string name, object value, out LogEventProperty property);
    }
}