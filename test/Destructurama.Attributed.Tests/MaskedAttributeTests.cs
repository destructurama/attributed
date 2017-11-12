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
        /// 123456789 results in "***"
        /// </summary>
        [LogMasked]
        public string DefaultMasked { get; set; }

        /// <summary>
        /// 123456789 results in "*********"
        /// </summary>
        [LogMasked(PreserveLength: true)]
        public string DefaultMaskedPreserved { get; set; }

        /// <summary>
        ///  123456789 results in "#"
        /// </summary>
        [LogMasked(Text: "_REMOVED_")]
        public string CustomMasked { get; set; }

        /// <summary>
        ///  123456789 results in "#########"
        /// </summary>
        [LogMasked(Text: "#", PreserveLength: true)]
        public string CustomMaskedPreservedLenght { get; set; }

        /// <summary>
        ///  123456789 results in "123******"
        /// </summary>
        [LogMasked(ShowFirst: 3)]
        public string ShowFirstThreeThenDefaultMasked { get; set; }

        /// <summary>
        /// 123456789 results in "123******"
        /// </summary>
        [LogMasked(ShowFirst: 3, PreserveLength: true)]
        public string ShowFirstThreeThenDefaultMaskedPreservedLenght { get; set; }

        /// <summary>
        /// 123456789 results in "***789"
        /// </summary>
        [LogMasked(ShowLast: 3)]
        public string ShowLastThreeThenDefaultMasked { get; set; }

        /// <summary>
        /// 123456789 results in "******789"
        /// </summary>
        [LogMasked(ShowLast: 3, PreserveLength: true)]
        public string ShowLastThreeThenDefaultMaskedPreservedLenght { get; set; }

        /// <summary>
        ///  123456789 results in "123REMOVED"
        /// </summary>
        [LogMasked(Text: "_REMOVED_", ShowFirst: 3)]
        public string ShowFirstThreeThenCustomMask { get; set; }

        /// <summary>
        ///  123456789 results in "123_REMOVED_"
        /// </summary>
        [LogMasked(Text: "_REMOVED_", ShowFirst: 3, PreserveLength: true)]
        public string ShowFirstThreeThenCustomMaskPreservedLenghtIgnored { get; set; }

        /// <summary>
        ///  123456789 results in "_REMOVED_789"
        /// </summary>
        [LogMasked(Text: "_REMOVED_", ShowLast: 3)]
        public string ShowLastThreeThenCustomMask { get; set; }

        /// <summary>
        ///  123456789 results in "_REMOVED_789"
        /// </summary>
        [LogMasked(Text: "_REMOVED_", ShowLast: 3, PreserveLength: true)]
        public string ShowLastThreeThenCustomMaskPreservedLenghtIgnored { get; set; }

        /// <summary>
        /// 123456789 results in "123***789"
        /// </summary>
        [LogMasked(ShowFirst: 3, ShowLast: 3)]
        public string ShowFirstAndLastThreeAndDefaultMaskeInTheMiddle { get; set; }

        /// <summary>
        ///  123456789 results in "123_REMOVED_789                                                                                                                                                                                                                                                                                                                <               °                       °    °                                         °                     789"
        /// </summary>
        [LogMasked(Text: "_REMOVED_", ShowFirst: 3, ShowLast: 3)]
        public string ShowFirstAndLastThreeAndCustomMaskInTheMiddle { get; set; }

        /// <summary>
        ///  123456789 results in "123_REMOVED_789". PreserveLenght is ignored                                                                                                                                                                                                                                                                                                          <               °                       °    °                                         °                     789"
        /// </summary>
        [LogMasked(Text: "_REMOVED_", ShowFirst: 3, ShowLast: 3, PreserveLength: true)]
        public string ShowFirstAndLastThreeAndCustomMaskInTheMiddlePreservedLenghtIgnored { get; set; }
    }

    [TestFixture]
    public class MaskedAttributeTests
    {
        [Test]
        public void LogMaskedAttribute_Replaces_Value_With_DefaultStars_Mask()
        {
            // [LogMasked]
            // 123456789 -> "***"

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
            Assert.AreEqual("***", props["DefaultMasked"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Replaces_Value_With_DefaultStars_Mask_And_PreservedLenght()
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
                DefaultMaskedPreserved = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("DefaultMaskedPreserved"));
            Assert.AreEqual("*********", props["DefaultMaskedPreserved"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Replaces_Value_With_Provided_Mask()
        {
            //  [LogMasked(Text: "#")]
            //   123456789 -> "_REMOVED_"

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
            Assert.AreEqual("_REMOVED_", props["CustomMasked"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Replaces_Value_With_Provided_Mask_And_PreservedLenght()
        {
            //  [LogMasked(Text: "#")]
            //   123456789 -> "#########"

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                CustomMaskedPreservedLenght = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("CustomMaskedPreservedLenght"));
            Assert.AreEqual("#########", props["CustomMaskedPreservedLenght"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_With_Custom_Mask()
        {
            // [LogMasked(Text: "REMOVED", ShowFirst = 3)]
            // -> "123_REMOVED_"

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
            Assert.AreEqual("123_REMOVED_", props["ShowFirstThreeThenCustomMask"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_With_Custom_Mask_PreservedLenght_Ignored()
        {
            // [LogMasked(Text: "REMOVED", ShowFirst = 3,PreserveLenght = true)]
            // -> "123_REMOVED_"

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstThreeThenCustomMaskPreservedLenghtIgnored = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstThreeThenCustomMaskPreservedLenghtIgnored"));
            Assert.AreEqual("123_REMOVED_", props["ShowFirstThreeThenCustomMaskPreservedLenghtIgnored"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Replaces_Value_With_Default_StarMask()
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
            Assert.AreEqual("123***321", props["ShowFirstAndLastThreeAndDefaultMaskeInTheMiddle"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask()
        {
            // [LogMasked(Text: "REMOVED", ShowFirst = 3, ShowLast = 3)]
            // 12345678987654321 -> 123_REMOVED_321     

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
            Assert.AreEqual("123_REMOVED_321", props["ShowFirstAndLastThreeAndCustomMaskInTheMiddle"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask_And_PreservedLenght()
        {
            // [LogMasked(Text: "#", ShowFirst = 3, ShowLast = 3)]
            // 12345678987654321 -> "123_REMOVED_321"

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
            Assert.AreEqual("123_REMOVED_321", props["ShowFirstAndLastThreeAndCustomMaskInTheMiddle"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask()
        {
            //  [LogMasked(ShowLast = 3)]
            //  123456789 -> "123***"

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
            Assert.AreEqual("123***", props["ShowFirstThreeThenDefaultMasked"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask()
        {
            //  [LogMasked(Text: "_REMOVED_", ShowLast = 3)]
            //  123456789 -> "_REMOVED_789"

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
            Assert.AreEqual("_REMOVED_789", props["ShowLastThreeThenCustomMask"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask_PreserveLength_Ignored()
        {
            //  [LogMasked(Text: "_REMOVED_", ShowLast = 3, PreserveLength = true)]
            //  123456789 -> "_REMOVED_789"

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowLastThreeThenCustomMaskPreservedLenghtIgnored = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowLastThreeThenCustomMaskPreservedLenghtIgnored"));
            Assert.AreEqual("_REMOVED_789", props["ShowLastThreeThenCustomMaskPreservedLenghtIgnored"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask()
        {
            //  [LogMasked(ShowLast = 3)]
            //  123456789 -> "***789"

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
            Assert.AreEqual("***789", props["ShowLastThreeThenDefaultMasked"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLenght()
        {
            //  [LogMasked(ShowFirst = 3,PreserveLength: true))]
            // -> "123******"

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstThreeThenDefaultMaskedPreservedLenght = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstThreeThenDefaultMaskedPreservedLenght"));
            Assert.AreEqual("123******", props["ShowFirstThreeThenDefaultMaskedPreservedLenght"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLenght()
        {
            //  [LogMasked(ShowLast = 3,PreserveLength: true))]
            // -> "******789"

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowLastThreeThenDefaultMaskedPreservedLenght = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowLastThreeThenDefaultMaskedPreservedLenght"));
            Assert.AreEqual("******789", props["ShowLastThreeThenDefaultMaskedPreservedLenght"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask_And_PreservedLenght_That_Gives_Warning()
        {
            // [LogMasked(Text: "REMOVED", ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
            // 12345678987654321 -> 123_REMOVED_321 

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstAndLastThreeAndCustomMaskInTheMiddlePreservedLenghtIgnored = "12345678987654321"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstAndLastThreeAndCustomMaskInTheMiddlePreservedLenghtIgnored"));
            Assert.AreEqual("123_REMOVED_321", props["ShowFirstAndLastThreeAndCustomMaskInTheMiddlePreservedLenghtIgnored"].LiteralValue());
        }
    }
}


