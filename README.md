# Destructurama.Attributed

![License](https://img.shields.io/github/license/destructurama/attributed)

[![codecov](https://codecov.io/gh/destructurama/attributed/graph/badge.svg?token=Ma2sUoqqb1)](https://codecov.io/gh/destructurama/attributed)
[![Nuget](https://img.shields.io/nuget/dt/Destructurama.Attributed)](https://www.nuget.org/packages/Destructurama.Attributed)
[![Nuget](https://img.shields.io/nuget/v/Destructurama.Attributed)](https://www.nuget.org/packages/Destructurama.Attributed)

[![GitHub Release Date](https://img.shields.io/github/release-date/destructurama/attributed?label=released)](https://github.com/destructurama/attributed/releases)
[![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/destructurama/attributed/latest?label=new+commits)](https://github.com/destructurama/attributed/commits/master)
![Size](https://img.shields.io/github/repo-size/destructurama/attributed)

[![GitHub contributors](https://img.shields.io/github/contributors/destructurama/attributed)](https://github.com/destructurama/attributed/graphs/contributors)
![Activity](https://img.shields.io/github/commit-activity/w/destructurama/attributed)
![Activity](https://img.shields.io/github/commit-activity/m/destructurama/attributed)
![Activity](https://img.shields.io/github/commit-activity/y/destructurama/attributed)

[![Run unit tests](https://github.com/destructurama/attributed/actions/workflows/test.yml/badge.svg)](https://github.com/destructurama/attributed/actions/workflows/test.yml)
[![Publish preview to GitHub registry](https://github.com/destructurama/attributed/actions/workflows/publish-preview.yml/badge.svg)](https://github.com/destructurama/attributed/actions/workflows/publish-preview.yml)
[![Publish release to Nuget registry](https://github.com/destructurama/attributed/actions/workflows/publish-release.yml/badge.svg)](https://github.com/destructurama/attributed/actions/workflows/publish-release.yml)
[![CodeQL analysis](https://github.com/destructurama/attributed/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/destructurama/attributed/actions/workflows/codeql-analysis.yml)

This package makes it possible to manipulate how objects are logged to [Serilog](https://serilog.net) using attributes.

# Installation

Install from NuGet:

```powershell
Install-Package Destructurama.Attributed
```

# Usage

Modify logger configuration:

```csharp
using Destructurama;
...
var log = new LoggerConfiguration()
  .Destructure.UsingAttributes()
...
```

## 1. Changing a property name

Apply the `LogWithName` attribute:

<!-- snippet: LogWithName -->
<a id='snippet-LogWithName'></a>
```cs
public class PersonalData
{
    [LogWithName("FullName")]
    public string? Name { get; set; }
}
```
<sup><a href='/src/Destructurama.Attributed.Tests/LogWithNameAttributeTests.cs#L64-L70' title='Snippet source file'>snippet source</a> | <a href='#snippet-LogWithName' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## 2. Ignoring a property

Apply the `NotLogged` attribute:

<!-- snippet: LoginCommand -->
<a id='snippet-LoginCommand'></a>
```cs
public class LoginCommand
{
    public string? Username { get; set; }

    [NotLogged]
    public string? Password { get; set; }
}
```
<sup><a href='/src/Destructurama.Attributed.Tests/Snippets.cs#L29-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-LoginCommand' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

When the object is passed using `{@...}` syntax the attributes will be consulted.

<!-- snippet: LogCommand -->
<a id='snippet-LogCommand'></a>
```cs
var command = new LoginCommand { Username = "logged", Password = "not logged" };
_log.Information("Logging in {@Command}", command);
```
<sup><a href='/src/Destructurama.Attributed.Tests/Snippets.cs#L44-L47' title='Snippet source file'>snippet source</a> | <a href='#snippet-LogCommand' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## 3. Ignoring a property if it has default value

Apply the `NotLoggedIfDefault` attribute:

```csharp
public class LoginCommand
{
  public string Username { get; set; }

  [NotLoggedIfDefault]
  public string Password { get; set; }

  [NotLoggedIfDefault]
  public DateTime TimeStamp { get; set; }
}
```

## 4. Ignoring a property if it has null value

Apply the `NotLoggedIfNull` attribute:

```csharp
public class LoginCommand
{
  /// <summary>
  /// `null` value results in removed property
  /// </summary>
  [NotLoggedIfNull]
  public string Username { get; set; }

  /// <summary>
  /// Can be applied with [LogMasked] or [LogReplaced] attributes
  /// `null` value results in removed property
  /// "123456789" results in "***"
  /// </summary>
  [NotLoggedIfNull]
  [LogMasked]
  public string Password { get; set; }

  /// <summary>
  /// Attribute has no effect on non-reference and non-nullable types
  /// </summary>
  [NotLoggedIfNull]
  public int TimeStamp { get; set; }
}
```

Ignore null properties can be globally applied during logger configuration without need to apply attributes:
```csharp
var log = new LoggerConfiguration()
  .Destructure.UsingAttributes(x => x.IgnoreNullProperties = true)
  ...
```

## 5. Treating types and properties as scalars

To prevent destructuring of a type or property at all, apply the `LogAsScalar` attribute.

## 6. Masking a property

Apply the `LogMasked` attribute with various settings:

- **Text:** If set, the property value will be set to this text.
- **ShowFirst:** Shows the first x characters in the property value.
- **ShowLast:** Shows the last x characters in the property value.
- **PreserveLength:** If set, it will swap out each character with the default value. Note that this property will be ignored if Text has been set to custom value.

Masking works for all properties calling `ToString()` on their values.
Note that masking also works for properties of type `IEnumerable<string>` or derived from it, for example, `string[]` or `List<string>`.

### Examples

<!-- snippet: CustomizedMaskedLogs -->
<a id='snippet-CustomizedMaskedLogs'></a>
```cs
public class CustomizedMaskedLogs
{
    /// <summary>
    /// 123456789 results in "***"
    /// </summary>
    [LogMasked]
    public string? DefaultMasked { get; set; }

    /// <summary>
    /// 9223372036854775807 results in "***"
    /// </summary>
    [LogMasked]
    public long? DefaultMaskedLong { get; set; }

    /// <summary>
    /// 2147483647 results in "***"
    /// </summary>
    [LogMasked]
    public int? DefaultMaskedInt { get; set; }

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
    /// 123456789 results in "#"
    /// </summary>
    [LogMasked(Text = "_REMOVED_")]
    public string? CustomMasked { get; set; }

    /// <summary>
    /// 123456789 results in "#"
    /// </summary>
    [LogMasked(Text = "")]
    public string? CustomMaskedWithEmptyString { get; set; }

    /// <summary>
    /// 123456789 results in "#########"
    /// </summary>
    [LogMasked(Text = "#", PreserveLength = true)]
    public string? CustomMaskedPreservedLength { get; set; }

    /// <summary>
    /// 123456789 results in "123******"
    /// </summary>
    [LogMasked(ShowFirst = 3)]
    public string? ShowFirstThreeThenDefaultMasked { get; set; }

    /// <summary>
    /// 9223372036854775807 results in "922***807"
    /// </summary>
    [LogMasked(ShowFirst = 3, ShowLast = 3)]
    public long? ShowFirstAndLastThreeAndDefaultMaskLongInTheMiddle { get; set; }

    /// <summary>
    /// 2147483647 results in "214****647"
    /// </summary>
    [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
    public int? ShowFirstAndLastThreeAndDefaultMaskIntInTheMiddlePreservedLength { get; set; }

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
    /// 123456789 results in "123_REMOVED_"
    /// </summary>
    [LogMasked(Text = "_REMOVED_", ShowFirst = 3)]
    public string? ShowFirstThreeThenCustomMask { get; set; }

    /// <summary>
    /// d3c4a1f2-3b4e-4f5a-9b6c-7d8e9f0a1b2c results in "d3c4a_REMOVED_"
    /// </summary>
    [LogMasked(Text = "_REMOVED_", ShowFirst = 5)]
    public Guid? ShowFirstFiveThenCustomMaskGuid { get; set; }

    /// <summary>
    /// Descending results in "Desce_REMOVED_"
    /// </summary>
    [LogMasked(Text = "_REMOVED_", ShowFirst = 5)]
    public ListSortDirection ShowFirstFiveThenCustomMaskEnum { get; set; }

    /// <summary>
    /// 123456789 results in "123_REMOVED_"
    /// </summary>
    [LogMasked(Text = "_REMOVED_", ShowFirst = 3, PreserveLength = true)]
    public string? ShowFirstThreeThenCustomMaskPreservedLengthIgnored { get; set; }

    /// <summary>
    /// 123456789 results in "_REMOVED_789"
    /// </summary>
    [LogMasked(Text = "_REMOVED_", ShowLast = 3)]
    public string? ShowLastThreeThenCustomMask { get; set; }

    /// <summary>
    /// 123456789 results in "_REMOVED_789"
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
    /// 123456789 results in "123_REMOVED_789"
    /// </summary>
    [LogMasked(Text = "_REMOVED_", ShowFirst = 3, ShowLast = 3)]
    public string? ShowFirstAndLastThreeAndCustomMaskInTheMiddle { get; set; }

    /// <summary>
    /// 123456789 results in "123_REMOVED_789". PreserveLength is ignored"
    /// </summary>
    [LogMasked(Text = "_REMOVED_", ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
    public string? ShowFirstAndLastThreeAndCustomMaskInTheMiddlePreservedLengthIgnored { get; set; }
}
```
<sup><a href='/src/Destructurama.Attributed.Tests/MaskedAttributeTests.cs#L9-L170' title='Snippet source file'>snippet source</a> | <a href='#snippet-CustomizedMaskedLogs' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## 7. Masking a string property with regular expressions

Apply the `LogReplaced` attribute on a string to apply a RegEx replacement during Logging.

This is applicable in scenarios when a string contains both Sensitive and Non-Sensitive information.
An example of this could be a string such as "__Sensitive|NonSensitive__". Then apply the attribute like the following snippet:

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
 - **Options:** The [RegexOptions](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regexoptions?view=netcore-3.1) that will be applied. Defaults to __RegexOptions.None__.
 - **Timeout:** A time-out interval to evaluate regular expression. Defaults to __Regex.InfiniteMatchTimeout__.

Note that replacement also works for properties of type `IEnumerable<string>` or derived from it, for example, `string[]` or `List<string>`.

### Examples

<!-- snippet: WithRegex -->
<a id='snippet-WithRegex'></a>
```cs
public class WithRegex
{
    private const string REGEX_WITH_VERTICAL_BARS = @"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)";

    /// <summary>
    /// 123|456|789 results in "***|456|789"
    /// </summary>
    [LogReplaced(REGEX_WITH_VERTICAL_BARS, "***|$2|$3")]
    public string? RegexReplaceFirst { get; set; }

    /// <summary>
    /// 123|456|789 results in "123|***|789"
    /// </summary>
    [LogReplaced(REGEX_WITH_VERTICAL_BARS, "$1|***|$3")]
    public string? RegexReplaceSecond { get; set; }
}
```
<sup><a href='/src/Destructurama.Attributed.Tests/Snippets.cs#L6-L25' title='Snippet source file'>snippet source</a> | <a href='#snippet-WithRegex' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

# Benchmarks

The results are available [here](https://destructurama.github.io/attributed/dev/bench/).
