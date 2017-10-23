using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;

namespace Destructurama.Attributed.Tests
{
    public class CustomizedMaskedLogs
    {
        /// <summary>
        /// 123456789 results in "*********"
        /// </summary>
        [LogMasked]
        public string DefaultMasked { get; set; }

        /// <summary>
        ///  123456789 results in "#########"
        /// </summary>
        [LogMasked(Mask: '#')]
        public string CustomMasked { get; set; }

        /// <summary>
        ///  123456789 results in "123******"
        /// </summary>
        [LogMasked(ShowFirst: 3)]
        public string ShowFirstThreeThenDefaultMasked { get; set; }

        /// <summary>
        /// 123456789 results in "******789"
        /// </summary>
        [LogMasked(ShowLast: 3)]
        public string ShowLastThreeThenDefaultMasked { get; set; }

        /// <summary>
        ///  123456789 results in "123######"
        /// </summary>
        [LogMasked(Mask: '#', ShowFirst: 3)]
        public string ShowFirstThreeThenCustomMask { get; set; }

        /// <summary>
        ///  123456789 results in "######789"
        /// </summary>
        [LogMasked(Mask: '#', ShowLast: 3)]
        public string ShowLastThreeThenCustomMask { get; set; }

        /// <summary>
        /// 123456789 results in "123***789"
        /// </summary>
        [LogMasked(ShowFirst: 3, ShowLast: 3)]
        public string ShowFirstAndLastThreeAndDefaultMaskeInTheMiddle { get; set; }

        /// <summary>
        ///  123456789 results in "123###789"
        /// </summary>
        [LogMasked(Mask: '#', ShowFirst: 3, ShowLast: 3)]
        public string ShowFirstAndLastThreeAndCustomMaskInTheMiddle { get; set; }

        //[LogMasked(Mask: '#', ShowFirst: 3, ShowLast: 3, MaxMask:10)]
        //public string ShowLastAndFirstThreeAndCustomMaskeInTheMiddleMaxMask { get; set; }
    }

    [TestFixture]
    public class MaskedAttributeTests
    {
        [Test]
        public void LogMaskedAttribute_Replaces_Value_With_DefaultStars_Mask()
        {
            // [LogMasked]
            // 123456789 -> "*********"

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                DefaultMasked = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("DefaultMasked"));
            Assert.AreEqual("*********", props["DefaultMasked"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Replaces_Value_With_Provided_Mask()
        {
            //  [LogMasked(Mask: '#')]
            //   123456789 -> "#########"

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                CustomMasked = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("CustomMasked"));
            Assert.AreEqual("#########", props["CustomMasked"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_With_Custom_Mask()
        {
            // [LogMasked(Mask: '#', ShowFirst = 3)]
            // -> "123######"

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstThreeThenCustomMask = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstThreeThenCustomMask"));
            Assert.AreEqual("123######", props["ShowFirstThreeThenCustomMask"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Replaces_Value_With_Default_Mask()
        {
            // [LogMasked(ShowFirst = 3, ShowLast = 3)]
            // -> "1234*********4321"

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstAndLastThreeAndDefaultMaskeInTheMiddle = "12345678987654321"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstAndLastThreeAndDefaultMaskeInTheMiddle"));
            Assert.AreEqual("123***********321", props["ShowFirstAndLastThreeAndDefaultMaskeInTheMiddle"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Then_Replaces_All_Other_Chars_With_Provided_Mask()
        {
            // [LogMasked(Mask: '#', ShowFirst = 3, ShowLast = 3)]
            // 12345678987654321 -> "123###########321"

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstAndLastThreeAndCustomMaskInTheMiddle = "12345678987654321"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstAndLastThreeAndCustomMaskInTheMiddle"));
            Assert.AreEqual("123###########321", props["ShowFirstAndLastThreeAndCustomMaskInTheMiddle"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_Other_Chars_With_Default_Mask()
        {
            //  [LogMasked(ShowLast = 3)]
            //  123456789 -> "123******"

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstThreeThenDefaultMasked = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstThreeThenDefaultMasked"));
            Assert.AreEqual("123******", props["ShowFirstThreeThenDefaultMasked"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask()
        {
            //  [LogMasked(Mask: '#', ShowLast = 3)]
            //  123456789 -> "######789"

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowLastThreeThenCustomMask = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowLastThreeThenCustomMask"));
            Assert.AreEqual("######789", props["ShowLastThreeThenCustomMask"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Provided_Mask()
        {
            //  [LogMasked(Text = "####", ShowLast = 4)]
            // -> "("*****6789"

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowLastThreeThenDefaultMasked = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowLastThreeThenDefaultMasked"));
            Assert.AreEqual("******789", props["ShowLastThreeThenDefaultMasked"].LiteralValue());
        }

        /// <summary>
        /// This was just a thought. Maby the user wants to limit the lenght of masked value to some max value.
        /// E.g JWT token... maby you just want to log the first 20 and last 20 but not everything.
        /// </summary>
        //[Test]
        public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Then_Replaces_All_Other_Chars_With_Provided_Mask_But_Only_Max_Mask()
        {
            // [LogMasked(Text = "#####", ShowFirst = 3, ShowLast = 3, MaxMask = 10)]
            // -> "123##########321"

            throw new NotImplementedException("Just a thought to limit the mask to some number");

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                // ShowLastAndFirstThreeAndCustomMaskeInTheMiddleMaxMask = "12345678987654321123456789876543211234567898765432112345678987654321"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowLastAndFirstThreeAndCustomMaskeInTheMiddleMaxMask"));
            Assert.AreEqual("123##########321", props["ShowLastAndFirstThreeAndCustomMaskeInTheMiddleMaxMask"].LiteralValue());
        }
    }
}


