using System;

namespace Destructurama.Attributed
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LogMaskedAttribute : Attribute
    {
        public LogMaskedAttribute(char Mask = '*', int ShowFirst = 0, int ShowLast = 0)
        {
            this.Mask = Mask;
            this.ShowFirst = ShowFirst;
            this.ShowLast = ShowLast;
        }

        public char Mask { get; }
        public int ShowFirst { get; }
        public int ShowLast { get; }
    }
}
