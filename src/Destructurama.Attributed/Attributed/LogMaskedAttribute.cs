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

        private string Text { get; }
        private int ShowFirst { get; }
        private int ShowLast { get; }
        private bool PreserveLength { get; }

        internal object FormatMaskedValue(object propValue)
        {
            var val = propValue as string;

            if (string.IsNullOrEmpty(val))
                return val;

            if (ShowFirst == 0 && ShowLast == 0)
            {
                if (PreserveLength)
                    return new String(Text[0], val.Length);

                return Text;
            }

            if (ShowFirst > 0 && ShowLast == 0)
            {
                var first = val.Substring(0, Math.Min(ShowFirst, val.Length));

                if (PreserveLength && IsDefaultMask())
                {
                    string mask;
                    if (ShowFirst > val.Length)
                        mask = "";
                    else
                        mask = new String(Text[0], val.Length - ShowFirst);
                    return first + mask;
                }

                return first + Text;
            }

            if (ShowFirst == 0 && ShowLast > 0)
            {
                string last;
                if (ShowLast > val.Length)
                    last = val;
                else
                    last = val.Substring(val.Length - ShowLast);

                if (PreserveLength && IsDefaultMask())
                {
                    var mask = "";
                    if (ShowLast <= val.Length)
                        mask = new String(Text[0], val.Length - ShowLast);

                    return mask + last;
                }

                return Text + last;
            }

            if (ShowFirst > 0 && ShowLast > 0)
            {
                if (ShowFirst + ShowLast >= val.Length)
                    return val;

                var first = val.Substring(0, ShowFirst);
                var last = val.Substring(val.Length - ShowLast);

                return first + Text + last;
            }

            return propValue;
        }
    }
}
