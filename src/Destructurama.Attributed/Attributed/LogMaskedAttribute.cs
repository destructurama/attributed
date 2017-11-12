using System;

namespace Destructurama.Attributed
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LogMaskedAttribute : Attribute
    {
        private const string DefaultMask = "***";

        public LogMaskedAttribute(string Text = DefaultMask, int ShowFirst = 0, int ShowLast = 0, bool PreserveLength = false)
        {
            this.Text = Text;
            this.ShowFirst = ShowFirst;
            this.ShowLast = ShowLast;
            this.PreserveLength = PreserveLength;
        }

        /// <summary>
        /// Check to see if custom Text has been provided.
        /// If true PreserveLength is ignored.
        /// </summary>
        /// <returns></returns>
        internal bool IsDefaultMask()
        {
            return Text == DefaultMask;
        }

        public string Text { get; }
        public int ShowFirst { get; }
        public int ShowLast { get; }
        public bool PreserveLength { get; }
        public bool PreserveWhitespace { get; }
    }
}
