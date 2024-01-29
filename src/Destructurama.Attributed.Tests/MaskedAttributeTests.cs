using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog.Events;
using Shouldly;

namespace Destructurama.Attributed.Tests;

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
    /// 123456789 results in "123456789", no mask applied
    /// </summary>
    [LogMasked(ShowFirst = -1, ShowLast = -1)]
    public string? ShowFirstAndLastInvalidValues { get; set; }

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
        var customized = new CustomizedMaskedLogs
        {
            DefaultMasked = "123456789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("DefaultMasked").ShouldBeTrue();
        props["DefaultMasked"].LiteralValue().ShouldBe("***");
    }

    [Test]
    public void LogMaskedAttribute_Replaces_Array_Value_With_DefaultStars_Mask()
    {
        // [LogMasked]
        // [123456789,123456789,123456789] results in [***,***,***]
        var customized = new CustomizedMaskedLogs
        {
            DefaultMaskedArray = ["123456789", "123456789", "123456789"]
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("DefaultMaskedArray").ShouldBeTrue();
        var seq = props["DefaultMaskedArray"] as SequenceValue;
        foreach (var elem in seq!.Elements)
            elem.LiteralValue().ShouldBe("***");
    }

    [Test]
    public void LogMaskedAttribute_Replaces_Value_With_DefaultStars_Mask_And_PreservedLength()
    {
        // [LogMasked]
        // 123456789 -> "*********"
        var customized = new CustomizedMaskedLogs
        {
            DefaultMaskedPreserved = "123456789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("DefaultMaskedPreserved").ShouldBeTrue();
        props["DefaultMaskedPreserved"].LiteralValue().ShouldBe("*********");
    }

    [Test]
    public void LogMaskedAttribute_Replaces_Value_With_DefaultStars_Mask_And_Not_Preserve_Length_On_Empty_String()
    {
        // [LogMasked]
        // "" -> "***"
        var customized = new CustomizedMaskedLogs
        {
            DefaultMaskedNotPreservedOnEmptyString = ""
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("DefaultMaskedNotPreservedOnEmptyString").ShouldBeTrue();
        props["DefaultMaskedNotPreservedOnEmptyString"].LiteralValue().ShouldBe("***");
    }

    [Test]
    public void LogMaskedAttribute_Replaces_Value_With_Provided_Mask()
    {
        //  [LogMasked(Text = "#")]
        //   123456789 -> "_REMOVED_"
        var customized = new CustomizedMaskedLogs
        {
            CustomMasked = "123456789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("CustomMasked").ShouldBeTrue();
        props["CustomMasked"].LiteralValue().ShouldBe("_REMOVED_");
    }

    [Test]
    public void LogMaskedAttribute_Replaces_Value_With_Provided_Empty_Mask()
    {
        //  [LogMasked(Text = "#")]
        //   123456789 -> "_REMOVED_"
        var customized = new CustomizedMaskedLogs
        {
            CustomMaskedWithEmptyString = "123456789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("CustomMasked").ShouldBeTrue();
        props["CustomMaskedWithEmptyString"].LiteralValue().ShouldBe("");
    }

    [Test]
    public void LogMaskedAttribute_Replaces_Value_With_Provided_Mask_And_PreservedLength()
    {
        //  [LogMasked(Text = "#")]
        //   123456789 -> "#########"
        var customized = new CustomizedMaskedLogs
        {
            CustomMaskedPreservedLength = "123456789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("CustomMaskedPreservedLength").ShouldBeTrue();
        props["CustomMaskedPreservedLength"].LiteralValue().ShouldBe("#########");
    }

    [Test]
    public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_With_Custom_Mask()
    {
        // [LogMasked(Text = "REMOVED", ShowFirst = 3)]
        // -> "123_REMOVED_"
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstThreeThenCustomMask = "123456789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstThreeThenCustomMask").ShouldBeTrue();
        props["ShowFirstThreeThenCustomMask"].LiteralValue().ShouldBe("123_REMOVED_");
    }

    [Test]
    public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_With_Custom_Mask_PreservedLength_Ignored()
    {
        // [LogMasked(Text = "REMOVED", ShowFirst = 3,PreserveLength = true)]
        // -> "123_REMOVED_"
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstThreeThenCustomMaskPreservedLengthIgnored = "123456789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstThreeThenCustomMaskPreservedLengthIgnored").ShouldBeTrue();
        props["ShowFirstThreeThenCustomMaskPreservedLengthIgnored"].LiteralValue().ShouldBe("123_REMOVED_");
    }

    [Test]
    public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Replaces_Value_With_Default_StarMask()
    {
        // [LogMasked(ShowFirst = 3, ShowLast = 3)]
        // -> "123***321"
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstAndLastThreeAndDefaultMaskInTheMiddle = "12345678987654321"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstAndLastThreeAndDefaultMaskInTheMiddle").ShouldBeTrue();
        props["ShowFirstAndLastThreeAndDefaultMaskInTheMiddle"].LiteralValue().ShouldBe("123***321");
    }

    [Test]
    public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Replaces_Value_With_Default_StarMask_PreserveLength()
    {
        // [LogMasked(ShowFirst = 3, ShowLast = 3)]
        // -> "123***********321"
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstAndLastThreeAndDefaultMaskInTheMiddlePreservedLength = "12345678987654321"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstAndLastThreeAndDefaultMaskInTheMiddlePreservedLength").ShouldBeTrue();
        props["ShowFirstAndLastThreeAndDefaultMaskInTheMiddlePreservedLength"].LiteralValue().ShouldBe("123***********321");
    }

    [Test]
    public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Replaces_Value_With_Default_StarMask_Single_PreserveLength()
    {
        // [LogMasked(ShowFirst = 3, ShowLast = 3)]
        // -> "123*456"
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstAndLastThreeAndDefaultMaskInTheMiddlePreservedLength = "123x456"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstAndLastThreeAndDefaultMaskInTheMiddlePreservedLength").ShouldBeTrue();
        props["ShowFirstAndLastThreeAndDefaultMaskInTheMiddlePreservedLength"].LiteralValue().ShouldBe("123*456");
    }

    [Test]
    public void LogMaskedAttribute_With_Invalid_Values_Should_Return_Value_As_Is()
    {
        // [LogMasked(ShowFirst = -1, ShowLast = -1)]
        // -> "123456789", no mask applied
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstAndLastInvalidValues = "123456789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstAndLastInvalidValues").ShouldBeTrue();
        props["ShowFirstAndLastInvalidValues"].LiteralValue().ShouldBe("123456789");
    }

    [Test]
    public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask()
    {
        // [LogMasked(Text = "REMOVED", ShowFirst = 3, ShowLast = 3)]
        // 12345678987654321 -> 123_REMOVED_321
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstAndLastThreeAndCustomMaskInTheMiddle = "12345678987654321"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstAndLastThreeAndCustomMaskInTheMiddle").ShouldBeTrue();
        props["ShowFirstAndLastThreeAndCustomMaskInTheMiddle"].LiteralValue().ShouldBe("123_REMOVED_321");
    }

    [Test]
    public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask_And_PreservedLength()
    {
        // [LogMasked(Text = "#", ShowFirst = 3, ShowLast = 3)]
        // 12345678987654321 -> "123_REMOVED_321"
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstAndLastThreeAndCustomMaskInTheMiddle = "12345678987654321"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstAndLastThreeAndCustomMaskInTheMiddle").ShouldBeTrue();
        props["ShowFirstAndLastThreeAndCustomMaskInTheMiddle"].LiteralValue().ShouldBe("123_REMOVED_321");
    }

    [Test]
    public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask_And_PreservedLength_Even_When_Input_Length_Is_Less_Than_ShowFirst()
    {
        // [LogMasked(Text = "#", ShowFirst = 3, ShowLast = 3)]
        // 12 -> "12"
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstAndLastThreeAndCustomMaskInTheMiddle = "12"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstAndLastThreeAndCustomMaskInTheMiddle").ShouldBeTrue();
        props["ShowFirstAndLastThreeAndCustomMaskInTheMiddle"].LiteralValue().ShouldBe("12");
    }

    [Test]
    public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask_And_PreservedLength_Even_When_Input_Length_Is_Less_Than_ShowFirst_Plus_ShowLast()
    {
        // [LogMasked(Text = "#", ShowFirst = 3, ShowLast = 3)]
        // 1234 -> "1234"
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstAndLastThreeAndCustomMaskInTheMiddle = "1234"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstAndLastThreeAndCustomMaskInTheMiddle").ShouldBeTrue();
        props["ShowFirstAndLastThreeAndCustomMaskInTheMiddle"].LiteralValue().ShouldBe("1234");
    }

    [Test]
    public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask()
    {
        //  [LogMasked(ShowLast = 3)]
        //  123456789 -> "123***"
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstThreeThenDefaultMasked = "123456789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstThreeThenDefaultMasked").ShouldBeTrue();
        props["ShowFirstThreeThenDefaultMasked"].LiteralValue().ShouldBe("123***");
    }

    [Test]
    public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask()
    {
        //  [LogMasked(Text = "_REMOVED_", ShowLast = 3)]
        //  123456789 -> "_REMOVED_789"
        var customized = new CustomizedMaskedLogs
        {
            ShowLastThreeThenCustomMask = "123456789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowLastThreeThenCustomMask").ShouldBeTrue();
        props["ShowLastThreeThenCustomMask"].LiteralValue().ShouldBe("_REMOVED_789");
    }

    [Test]
    public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask_PreserveLength_Ignored()
    {
        //  [LogMasked(Text = "_REMOVED_", ShowLast = 3, PreserveLength = true)]
        //  123456789 -> "_REMOVED_789"
        var customized = new CustomizedMaskedLogs
        {
            ShowLastThreeThenCustomMaskPreservedLengthIgnored = "123456789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowLastThreeThenCustomMaskPreservedLengthIgnored").ShouldBeTrue();
        props["ShowLastThreeThenCustomMaskPreservedLengthIgnored"].LiteralValue().ShouldBe("_REMOVED_789");
    }

    [Test]
    public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask()
    {
        //  [LogMasked(ShowLast = 3)]
        //  123456789 -> "***789"
        var customized = new CustomizedMaskedLogs
        {
            ShowLastThreeThenDefaultMasked = "123456789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowLastThreeThenDefaultMasked").ShouldBeTrue();
        props["ShowLastThreeThenDefaultMasked"].LiteralValue().ShouldBe("***789");
    }

    [Test]
    public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLength()
    {
        //  [LogMasked(ShowFirst = 3,PreserveLength = true))]
        // -> "123******"
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstThreeThenDefaultMaskedPreservedLength = "123456789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstThreeThenDefaultMaskedPreservedLength").ShouldBeTrue();
        props["ShowFirstThreeThenDefaultMaskedPreservedLength"].LiteralValue().ShouldBe("123******");
    }

    [Test]
    public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLength_Even_For_An_Empty_Input()
    {
        //  [LogMasked(ShowFirst = 3,PreserveLength = true))]
        // -> ""
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstThreeThenDefaultMaskedPreservedLength = ""
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];

        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstThreeThenDefaultMaskedPreservedLength").ShouldBeTrue();
        props["ShowFirstThreeThenDefaultMaskedPreservedLength"].LiteralValue().ShouldBe("");
    }

    [Test]
    public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLength_Even_For_An_Input_With_Same_Length_As_ShowFirst()
    {
        //  [LogMasked(ShowFirst = 3,PreserveLength = true))]
        // -> "123"
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstThreeThenDefaultMaskedPreservedLength = "123"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];

        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstThreeThenDefaultMaskedPreservedLength").ShouldBeTrue();
        props["ShowFirstThreeThenDefaultMaskedPreservedLength"].LiteralValue().ShouldBe("123");
    }

    [Test]
    public void LogMaskedAttribute_Shows_First_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLength_Even_For_An_Input_Shorter_Than_ShowFirst()
    {
        //  [LogMasked(ShowFirst = 3,PreserveLength = true))]
        // -> "12"
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstThreeThenDefaultMaskedPreservedLength = "12"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];

        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstThreeThenDefaultMaskedPreservedLength").ShouldBeTrue();
        props["ShowFirstThreeThenDefaultMaskedPreservedLength"].LiteralValue().ShouldBe("12");
    }

    [Test]
    public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLength()
    {
        //  [LogMasked(ShowLast = 3,PreserveLength = true))]
        // -> "******789"
        var customized = new CustomizedMaskedLogs
        {
            ShowLastThreeThenDefaultMaskedPreservedLength = "123456789"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowLastThreeThenDefaultMaskedPreservedLength").ShouldBeTrue();
        props["ShowLastThreeThenDefaultMaskedPreservedLength"].LiteralValue().ShouldBe("******789");
    }

    [Test]
    public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLength_Even_For_An_Input_With_Same_Length_As_ShowLast()
    {
        //  [LogMasked(ShowLast = 3,PreserveLength = true))]
        // -> "123"
        var customized = new CustomizedMaskedLogs
        {
            ShowLastThreeThenDefaultMaskedPreservedLength = "123"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowLastThreeThenDefaultMaskedPreservedLength").ShouldBeTrue();
        props["ShowLastThreeThenDefaultMaskedPreservedLength"].LiteralValue().ShouldBe("123");
    }

    [Test]
    public void LogMaskedAttribute_Shows_Last_NChars_Then_Replaces_All_Other_Chars_With_Default_StarMask_And_PreservedLength_Even_For_An_Input_Shorter_Than_ShowLast()
    {
        //  [LogMasked(ShowLast = 3,PreserveLength = true))]
        // -> "12"
        var customized = new CustomizedMaskedLogs
        {
            ShowLastThreeThenDefaultMaskedPreservedLength = "12"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowLastThreeThenDefaultMaskedPreservedLength").ShouldBeTrue();
        props["ShowLastThreeThenDefaultMaskedPreservedLength"].LiteralValue().ShouldBe("12");
    }

    [Test]
    public void LogMaskedAttribute_Shows_First_NChars_And_Last_NChars_Then_Replaces_All_Other_Chars_With_Custom_Mask_And_PreservedLength_That_Gives_Warning()
    {
        // [LogMasked(Text = "REMOVED", ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        // 12345678987654321 -> 123_REMOVED_321
        var customized = new CustomizedMaskedLogs
        {
            ShowFirstAndLastThreeAndCustomMaskInTheMiddlePreservedLengthIgnored = "12345678987654321"
        };

        var evt = DelegatingSink.Execute(customized);

        var sv = (StructureValue)evt.Properties["Customized"];
        var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

        props.ContainsKey("ShowFirstAndLastThreeAndCustomMaskInTheMiddlePreservedLengthIgnored").ShouldBeTrue();
        props["ShowFirstAndLastThreeAndCustomMaskInTheMiddlePreservedLengthIgnored"].LiteralValue().ShouldBe("123_REMOVED_321");
    }
}
