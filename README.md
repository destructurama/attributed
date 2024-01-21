# <img src="/assets/icon.png" height="30px"> Destructurama.Attributed

[![Build status](https://ci.appveyor.com/api/projects/status/1tutmofqjb9wq627/branch/master?svg=true)](https://ci.appveyor.com/project/Destructurama/attributed)
[![NuGet Status](https://img.shields.io/nuget/v/Destructurama.Attributed.svg)](https://www.nuget.org/packages/Destructurama.Attributed/)

This package makes it possible to manipulate how objects are logged to [Serilog](https://serilog.net) using attributes.


## Enabling the module:

Install from NuGet:

```powershell
Install-Package Destructurama.Attributed
```

Modify logger configuration:

```csharp
using Destructurama;
...
var log = new LoggerConfiguration()
  .Destructure.UsingAttributes()
  ...
```


#### Changing a property name

Apply the `LogWithName` attribute:

<!-- snippet: LogWithName -->
<a id='snippet-logwithname'></a>
```cs
using Destructurama.Attributed;
...
public class PersonalData
{
    [LogWithName("FullName")]
    public string? Name { get; set; }
}
```
<sup><a href='/test/Destructurama.Attributed.Tests/LogWithNameAttributedTests.cs#L37-L43' title='Snippet source file'>snippet source</a> | <a href='#snippet-logwithname' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Ignoring a property

Apply the `NotLogged` attribute:

<!-- snippet: LoginCommand -->
<a id='snippet-logincommand'></a>
```cs
using Destructurama.Attributed;
...
public class LoginCommand
{
    public string? Username { get; set; }

    [NotLogged]
    public string? Password { get; set; }
}
```
<sup><a href='/test/Destructurama.Attributed.Tests/Snippets.cs#L29-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-logincommand' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

When the object is passed using `{@...}` syntax the attributes will be consulted.

<!-- snippet: LogCommand -->
<a id='snippet-logcommand'></a>
```cs
var command = new LoginCommand { Username = "logged", Password = "not logged" };
log.Information("Logging in {@Command}", command);
```
<sup><a href='/test/Destructurama.Attributed.Tests/Snippets.cs#L44-L47' title='Snippet source file'>snippet source</a> | <a href='#snippet-logcommand' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Treating types and properties as scalars

To prevent destructuring of a type or property at all, apply the `[LogAsScalar]` attribute.


## Masking a string property

Apply the `LogMasked` attribute with various settings:

 - **Text:** If set, the property value will be set to this text.
 - **ShowFirst:** Shows the first x characters in the property value.
 - **ShowLast:** Shows the last x characters in the property value.
 - **PreserveLength:** If set, it will swap out each character with the default value. Note that this property will be ignored if Text has been set to custom value.

Note that masking also works for properties of type `IEnumerable<string>` or derived from it, for example, `string[]` or `List<string>`.


### Examples

<!-- snippet: CustomizedMaskedLogs -->
<a id='snippet-customizedmaskedlogs'></a>
```cs
using Destructurama.Attributed;
...
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
}
```
<sup><a href='/test/Destructurama.Attributed.Tests/MaskedAttributeTests.cs#L9-L122' title='Snippet source file'>snippet source</a> | <a href='#snippet-customizedmaskedlogs' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Masking a string property with regular expressions

Apply the `LogReplaced` attribute on a string to apply a RegEx replacement during Logging.

This is applicable in scenarios which a string contains both Sensitive and Non-Sensitive information. An example of this could be a string such as "__Sensitive|NonSensitive__". Then apply the attribute like the following snippet:

```csharp
[LogReplaced(@"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)", "***|$2")]
public property Information { get; set; }

// Will log: "***|NonSensitive"
``` 

`LogReplaced` attribute is available with the following constructor:

```csharp
LogReplaced(string pattern, string replacement)
```

__Constructor arguments__:
 - **pattern:** The pattern that should be applied on value.
 - **replacement:** The string that will be applied by RegEx. 

__Available properties__:
 - **Options:** The [RegexOptions](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regexoptions?view=netcore-3.1) that will be applied. Defaults to __RegexOptions.None__
 - **Timeout:** A time-out interval to evaluate regular expression. Defaults to __Regex.InfiniteMatchTimeout__


### Examples

<!-- snippet: WithRegex -->
<a id='snippet-withregex'></a>
```cs
public class WithRegex
{
    const string RegexWithVerticalBars = @"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)";

    /// <summary>
    /// 123|456|789 results in "***|456|789"
    /// </summary>
    [LogReplaced(RegexWithVerticalBars, "***|$2|$3")]
    public string? RegexReplaceFirst { get; set; }

    /// <summary>
    /// 123|456|789 results in "123|***|789"
    /// </summary>
    [LogReplaced(RegexWithVerticalBars, "$1|***|$3")]
    public string? RegexReplaceSecond { get; set; }
}
```
<sup><a href='/test/Destructurama.Attributed.Tests/Snippets.cs#L6-L25' title='Snippet source file'>snippet source</a> | <a href='#snippet-withregex' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
