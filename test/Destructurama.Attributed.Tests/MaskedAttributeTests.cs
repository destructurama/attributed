using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;

namespace Destructurama.Attributed.Tests
{
    #region CustomizedMaskedLogs

    public class CustomizedMaskedLogs
    {
        /// <summary>
        /// 123456789 results in "***"
        /// </summary>
        [LogMasked]
        public string? DefaultMasked { get; set; }

        /// <summary>
        /// [123456789,123456789,123456789] results in [***,***,***]
        /// </summary>
        [LogMasked]
        public string[]? DefaultMaskedArray { get; set; }

        /// <summary>
        /// 123456789 results in "*********"
        /// </summary>
        [LogMasked(PreserveLength = true)]
        public string? DefaultMaskedPreserved { get; set; }

        /// <summary>
        /// "" results in "***"
        /// </summary>
        [LogMasked]
        public string? DefaultMaskedNotPreservedOnEmptyString { get; set; }

        /// <summary>
        ///  123456789 results in "#"
        /// </summary>
        [LogMasked(Text = "_REMOVED_")]
        public string? CustomMasked { get; set; }

        /// <summary>
        ///  123456789 results in "#"
        /// </summary>
        [LogMasked(Text = "")]
        public string? CustomMaskedWithEmptyString { get; set; }

        /// <summary>
        ///  123456789 results in "#########"
        /// </summary>
        [LogMasked(Text = "#", PreserveLength = true)]
        public string? CustomMaskedPreservedLength { get; set; }

        /// <summary>
        ///  123456789 results in "123******"
        /// </summary>
        [LogMasked(ShowFirst = 3)]
        public string? ShowFirstThreeThenDefaultMasked { get; set; }

        /// <summary>
        /// 123456789 results in "123******"
        /// </summary>
        [LogMasked(ShowFirst = 3, PreserveLength = true)]
        public string? ShowFirstThreeThenDefaultMaskedPreservedLength { get; set; }

        /// <summary>
        /// 123456789 results in "***789"
        /// </summary>
        [LogMasked(ShowLast = 3)]
        public string? ShowLastThreeThenDefaultMasked { get; set; }

        /// <summary>
        /// 123456789 results in "******789"
        /// </summary>
        [LogMasked(ShowLast = 3, PreserveLength = true)]
        public string? ShowLastThreeThenDefaultMaskedPreservedLength { get; set; }

        /// <summary>
        ///  123456789 results in "123REMOVED"
        /// </summary>
        [LogMasked(Text = "_REMOVED_", ShowFirst = 3)]
        public string? ShowFirstThreeThenCustomMask { get; set; }

        /// <summary>
        ///  123456789 results in "123_REMOVED_"
        /// </summary>
        [LogMasked(Text = "_REMOVED_", ShowFirst = 3, PreserveLength = true)]
        public string? ShowFirstThreeThenCustomMaskPreservedLengthIgnored { get; set; }

        /// <summary>
        ///  123456789 results in "_REMOVED_789"
        /// </summary>
        [LogMasked(Text = "_REMOVED_", ShowLast = 3)]
        public string? ShowLastThreeThenCustomMask { get; set; }

        /// <summary>
        ///  123456789 results in "_REMOVED_789"
        /// </summary>
        [LogMasked(Text = "_REMOVED_", ShowLast = 3, PreserveLength = true)]
        public string? ShowLastThreeThenCustomMaskPreservedLengthIgnored { get; set; }

        /// <summary>
        /// 123456789 results in "123***789"
        /// </summary>
        [LogMasked(ShowFirst = 3, ShowLast = 3)]
        public string? ShowFirstAndLastThreeAndDefaultMaskInTheMiddle { get; set; }

        /// <summary>
        /// 123456789 results in "123***789"
        /// </summary>
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        public string? ShowFirstAndLastThreeAndDefaultMaskInTheMiddlePreservedLength { get; set; }

        /// <summary>
        ///  123456789 results in "123_REMOVED_789"
        /// </summary>
        [LogMasked(Text = "_REMOVED_", ShowFirst = 3, ShowLast = 3)]
        public string? ShowFirstAndLastThreeAndCustomMaskInTheMiddle { get; set; }

        /// <summary>
        ///  123456789 results in "123_REMOVED_789". PreserveLength is ignored"
        /// </summary>
        [LogMasked(Text = "_REMOVED_", ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        public string? ShowFirstAndLastThreeAndCustomMaskInTheMiddlePreservedLengthIgnored { get; set; }

        /// <summary>
        ///  NOTE When applied on non-string types value will be converted to string with InvariantCulture
        ///  123456789 results in "123***"
        /// </summary>
        [LogMasked(ShowFirst = 3)]
        public int IntMasked { get; set; }

        /// <summary>
        ///  When applied on non-string types value will be converted to string with InvariantCulture
        ///  new DateTime(2000, 1, 2, 3, 4, 5) results in "01/02/2000 ***"
        /// </summary>
        [LogMasked(ShowFirst = 11)]
        public DateTime DateTimeMasked { get; set; }
    }

    #endregion

    [TestFixture]
    public class MaskedAttributeTests
    {
        [Test]
        public void LogMaskedAttribute_Replaces_Value_With_DefaultStars_Mask()
        {
            // [LogMasked]
            // 123456789 -> "***"

            LogEvent evt = null!;

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
        public void LogMaskedAttribute_Replaces_Array_Value_With_DefaultStars_Mask()
        {
            // [LogMasked]
            // [123456789,123456789,123456789] results in [***,***,***]

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                DefaultMaskedArray = new[] { "123456789", "123456789", "123456789" }
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("DefaultMaskedArray"));
            var seq = props["DefaultMaskedArray"] as SequenceValue;
            foreach (var elem in seq!.Elements)
                Assert.AreEqual("***", elem.LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Replaces_Value_With_DefaultStars_Mask_And_PreservedLength()
        {
            // [LogMasked]
            // 123456789 -> "*********"

            LogEvent evt = null!;

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
        public void LogMaskedAttribute_Replaces_Value_With_DefaultStars_Mask_And_Not_Preserve_Length_On_Empty_String()
        {
            // [LogMasked]
            // "" -> "***"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                DefaultMaskedNotPreservedOnEmptyString = ""
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("DefaultMaskedNotPreservedOnEmptyString"));
            Assert.AreEqual("***", props["DefaultMaskedNotPreservedOnEmptyString"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Replaces_Value_With_Provided_Mask()
        {
            //  [LogMasked(Text = "#")]
            //   123456789 -> "_REMOVED_"

            LogEvent evt = null!;

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
        public void LogMaskedAttribute_Replaces_Value_With_Provided_Empty_Mask()
        {
            //  [LogMasked(Text = "#")]
            //   123456789 -> "_REMOVED_"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                CustomMaskedWithEmptyString = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("CustomMasked"));
            Assert.AreEqual("", props["CustomMaskedWithEmptyString"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Replaces_Value_With_Provided_Mask_And_PreservedLength()
        {
            //  [LogMasked(Text = "#")]
            //   123456789 -> "#########"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                CustomMaskedPreservedLength = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("CustomMaskedPreservedLength"));
            Assert.AreEqual("#########", props["CustomMaskedPreservedLength"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_With_Custom_Mask()
        {
            // [LogMasked(Text = "REMOVED", ShowFirst = 3)]
            // -> "123_REMOVED_"

            LogEvent evt = null!;

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
        public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_With_Custom_Mask_PreservedLength_Ignored()
        {
            // [LogMasked(Text = "REMOVED", ShowFirst = 3,PreserveLength = true)]
            // -> "123_REMOVED_"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstThreeThenCustomMaskPreservedLengthIgnored = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstThreeThenCustomMaskPreservedLengthIgnored"));
            Assert.AreEqual("123_REMOVED_", props["ShowFirstThreeThenCustomMaskPreservedLengthIgnored"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Replaces_Value_With_Default_StarMask()
        {
            // [LogMasked(ShowFirst = 3, ShowLast = 3)]
            // -> "123***321"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstAndLastThreeAndDefaultMaskInTheMiddle = "12345678987654321"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstAndLastThreeAndDefaultMaskInTheMiddle"));
            Assert.AreEqual("123***321", props["ShowFirstAndLastThreeAndDefaultMaskInTheMiddle"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Replaces_Value_With_Default_StarMask_PreserveLength()
        {
            // [LogMasked(ShowFirst = 3, ShowLast = 3)]
            // -> "123***********321"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstAndLastThreeAndDefaultMaskInTheMiddlePreservedLength = "12345678987654321"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstAndLastThreeAndDefaultMaskInTheMiddlePreservedLength"));
            Assert.AreEqual("123***********321", props["ShowFirstAndLastThreeAndDefaultMaskInTheMiddlePreservedLength"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Replaces_Value_With_Default_StarMask_Single_PreserveLength()
        {
            // [LogMasked(ShowFirst = 3, ShowLast = 3)]
            // -> "123*456"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstAndLastThreeAndDefaultMaskInTheMiddlePreservedLength = "123x456"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstAndLastThreeAndDefaultMaskInTheMiddlePreservedLength"));
            Assert.AreEqual("123*456", props["ShowFirstAndLastThreeAndDefaultMaskInTheMiddlePreservedLength"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask()
        {
            // [LogMasked(Text = "REMOVED", ShowFirst = 3, ShowLast = 3)]
            // 12345678987654321 -> 123_REMOVED_321

            LogEvent evt = null!;

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
        public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask_And_PreservedLength()
        {
            // [LogMasked(Text = "#", ShowFirst = 3, ShowLast = 3)]
            // 12345678987654321 -> "123_REMOVED_321"

            LogEvent evt = null!;

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
        public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask_And_PreservedLength_Even_When_Input_Length_Is_Less_Than_ShowFirst()
        {
            // [LogMasked(Text = "#", ShowFirst = 3, ShowLast = 3)]
            // 12 -> "12"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstAndLastThreeAndCustomMaskInTheMiddle = "12"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstAndLastThreeAndCustomMaskInTheMiddle"));
            Assert.AreEqual("12", props["ShowFirstAndLastThreeAndCustomMaskInTheMiddle"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask_And_PreservedLength_Even_When_Input_Length_Is_Less_Than_ShowFirst_Plus_ShowLast()
        {
            // [LogMasked(Text = "#", ShowFirst = 3, ShowLast = 3)]
            // 1234 -> "1234"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstAndLastThreeAndCustomMaskInTheMiddle = "1234"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstAndLastThreeAndCustomMaskInTheMiddle"));
            Assert.AreEqual("1234", props["ShowFirstAndLastThreeAndCustomMaskInTheMiddle"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask()
        {
            //  [LogMasked(ShowLast = 3)]
            //  123456789 -> "123***"

            LogEvent evt = null!;

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
            //  [LogMasked(Text = "_REMOVED_", ShowLast = 3)]
            //  123456789 -> "_REMOVED_789"

            LogEvent evt = null!;

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
            //  [LogMasked(Text = "_REMOVED_", ShowLast = 3, PreserveLength = true)]
            //  123456789 -> "_REMOVED_789"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowLastThreeThenCustomMaskPreservedLengthIgnored = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowLastThreeThenCustomMaskPreservedLengthIgnored"));
            Assert.AreEqual("_REMOVED_789", props["ShowLastThreeThenCustomMaskPreservedLengthIgnored"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask()
        {
            //  [LogMasked(ShowLast = 3)]
            //  123456789 -> "***789"

            LogEvent evt = null!;

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
        public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLength()
        {
            //  [LogMasked(ShowFirst = 3,PreserveLength = true))]
            // -> "123******"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstThreeThenDefaultMaskedPreservedLength = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstThreeThenDefaultMaskedPreservedLength"));
            Assert.AreEqual("123******", props["ShowFirstThreeThenDefaultMaskedPreservedLength"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLength_Even_For_An_Empty_Input()
        {
            //  [LogMasked(ShowFirst = 3,PreserveLength = true))]
            // -> ""

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstThreeThenDefaultMaskedPreservedLength = ""
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];

            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstThreeThenDefaultMaskedPreservedLength"));
            Assert.AreEqual("", props["ShowFirstThreeThenDefaultMaskedPreservedLength"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLength_Even_For_An_Input_With_Same_Length_As_ShowFirst()
        {
            //  [LogMasked(ShowFirst = 3,PreserveLength = true))]
            // -> "123"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstThreeThenDefaultMaskedPreservedLength = "123"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];

            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstThreeThenDefaultMaskedPreservedLength"));
            Assert.AreEqual("123", props["ShowFirstThreeThenDefaultMaskedPreservedLength"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLength_Even_For_An_Input_Shorter_Than_ShowFirst()
        {
            //  [LogMasked(ShowFirst = 3,PreserveLength = true))]
            // -> "12"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstThreeThenDefaultMaskedPreservedLength = "12"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];

            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstThreeThenDefaultMaskedPreservedLength"));
            Assert.AreEqual("12", props["ShowFirstThreeThenDefaultMaskedPreservedLength"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLength()
        {
            //  [LogMasked(ShowLast = 3,PreserveLength = true))]
            // -> "******789"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowLastThreeThenDefaultMaskedPreservedLength = "123456789"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowLastThreeThenDefaultMaskedPreservedLength"));
            Assert.AreEqual("******789", props["ShowLastThreeThenDefaultMaskedPreservedLength"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLength_Even_For_An_Input_With_Same_Length_As_ShowLast()
        {
            //  [LogMasked(ShowLast = 3,PreserveLength = true))]
            // -> "123"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowLastThreeThenDefaultMaskedPreservedLength = "123"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowLastThreeThenDefaultMaskedPreservedLength"));
            Assert.AreEqual("123", props["ShowLastThreeThenDefaultMaskedPreservedLength"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLength_Even_For_An_Input_Shorter_Than_ShowLast()
        {
            //  [LogMasked(ShowLast = 3,PreserveLength = true))]
            // -> "12"

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowLastThreeThenDefaultMaskedPreservedLength = "12"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowLastThreeThenDefaultMaskedPreservedLength"));
            Assert.AreEqual("12", props["ShowLastThreeThenDefaultMaskedPreservedLength"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask_And_PreservedLength_That_Gives_Warning()
        {
            // [LogMasked(Text = "REMOVED", ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
            // 12345678987654321 -> 123_REMOVED_321

            LogEvent evt = null!;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                ShowFirstAndLastThreeAndCustomMaskInTheMiddlePreservedLengthIgnored = "12345678987654321"
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("ShowFirstAndLastThreeAndCustomMaskInTheMiddlePreservedLengthIgnored"));
            Assert.AreEqual("123_REMOVED_321", props["ShowFirstAndLastThreeAndCustomMaskInTheMiddlePreservedLengthIgnored"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_ShowFirst_On_Int()
        {
            // <summary>
            //  NOTE When applied on non-string types value will be converted to string with InvariantCulture
            //  123456789 results in "123***"
            // </summary>
            // [LogMasked(ShowFirst = 3)]
            // public int IntMasked { get; set; }

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                IntMasked = 123456789
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("IntMasked"));
            Assert.AreEqual("123***", props["IntMasked"].LiteralValue());
        }

        [Test]
        public void LogMaskedAttribute_ShowFirst_On_DateTime()
        {
            // <summary>
            //  When applied on non-string types value will be converted to string with InvariantCulture
            //  new DateTime(2000, 1, 2, 3, 4, 5) results in "01/02/2000 ***"
            // </summary>
            // [LogMasked(ShowFirst = 11)]
            // public DateTime DateTimeMasked { get; set; }

            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new CustomizedMaskedLogs
            {
                DateTimeMasked = new DateTime(2000, 1, 2, 3, 4, 5)
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("DateTimeMasked"));
            Assert.AreEqual("01/02/2000 ***", props["DateTimeMasked"].LiteralValue());
        }

    }
}