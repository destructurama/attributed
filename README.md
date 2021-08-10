# <img src="/assets/icon.png" height="30px"> Destructurama.Attributed

[![Build status](https://ci.appveyor.com/api/projects/status/1tutmofqjb9wq627/branch/master?svg=true)](https://ci.appveyor.com/project/Destructurama/attributed)
[![NuGet Status](https://img.shields.io/nuget/v/Destructurama.Attributed.svg)](https://www.nuget.org/packages/Destructurama.Attributed/)

This package makes it possible to manipulate how objects are logged to [Serilog](http://serilog.net) using attributes.


## Enabling the module:

Install from NuGet:

```powershell
Install-Package Destructurama.Attributed
```

Modify logger configuration:

```csharp
var log = new LoggerConfiguration()
  .Destructure.UsingAttributes()
  ...
```


## Ignoring a property

Apply the `NotLogged` attribute:

```csharp
public class LoginCommand
{
  public string Username { get; set; }

  [NotLogged]
  public string Password { get; set; }
}
```

When the object is passed using `{@...}` syntax the attributes will be consulted.

```csharp
var command = new LoginCommand { Username = "logged", Password = "not logged" };
log.Information("Logging in {@Command}", command);
```


## Treating types and properties as scalars

To prevent destructuring of a type or property at all, apply the `[LoggedAsScalar]` attribute.


## Masking a string property

Apply the `LogMasked` attribute with various settings:

 - **Text:** If set the properties value will be set to this text.
 - **ShowFirst:** Shows the first x characters in the property value 
 - **ShowLast:** Shows the last x characters in the property value 
 - **PreserveLength:** If set it will swap out each character with the default value. Note that this property will be ignored if Text has been set to custom value.


### Examples

```csharp
public class CreditCard
{
  /// <summary>
  /// 123456789 results in "*********"
  /// </summary>
  [LogMasked]
  public string DefaultMasked { get; set; }
  
  /// <summary>
  ///  123456789 results in "REMOVED"
  /// </summary>
  [LogMasked(Text="REMOVED")]
  public string CustomMasked { get; set; }
  
  /// <summary>
  ///  123456789 results in "123***"
  /// </summary>
  [LogMasked(ShowFirst=3)]
  public string ShowFirstThreeThenDefaultMasked { get; set; }

  /// <summary>
  ///  123456789 results in "123******"
  /// </summary>
  [LogMasked(ShowFirst=3, PreserveLength=true)]
  public string ShowFirstThreeThenDefaultMaskedPreserveLength { get; set; }

  /// <summary>
  /// 123456789 results in "***789"
  /// </summary>
  [LogMasked(ShowLast=3)]
  public string ShowLastThreeThenDefaultMasked { get; set; }
  
  /// <summary>
  /// 123456789 results in "******789"
  /// </summary>
  [LogMasked(ShowLast=3, PreserveLength=true)]
  public string ShowLastThreeThenDefaultMaskedPreserveLength  { get; set; }

  /// <summary>
  ///  123456789 results in "123REMOVED"
  /// </summary>
  [LogMasked(Text="REMOVED", ShowFirst=3)]
  public string ShowFirstThreeThenCustomMask { get; set; }

  /// <summary>
  ///  123456789 results in "REMOVED789"
  /// </summary>
  [LogMasked(Text="REMOVED", ShowLast=3)]
  public string ShowLastThreeThenCustomMask { get; set; }
  
  /// <summary>
  ///  123456789 results in "******789"
  /// </summary>
  [LogMasked(ShowLast=3, PreserveLength=true)]
  public string ShowLastThreeThenCustomMaskPreserveLength { get; set; }

  /// <summary>
  ///  123456789 results in "123******"
  /// </summary>
  [LogMasked(ShowFirst=3, PreserveLength=true)]
  public string ShowFirstThreeThenCustomMaskPreserveLength { get; set; }

  /// <summary>
  /// 123456789 results in "123***789"
  /// </summary>
  [LogMasked(ShowFirst=3, ShowLast=3)]
  public string ShowFirstAndLastThreeAndDefaultMaskeInTheMiddle { get; set; }

  /// <summary>
  ///  123456789 results in "123REMOVED789"
  /// </summary>
  [LogMasked(Text="REMOVED", ShowFirst=3, ShowLast=3)]
  public string ShowFirstAndLastThreeAndCustomMaskInTheMiddle { get; set; }

  /// <summary>
  ///  NOTE PreserveLength=true is ignored in this case
  ///  123456789 results in "123REMOVED789"
  /// </summary>
  [LogMasked(Text="REMOVED", ShowFirst=3, ShowLast=3, PreserveLength=true)]
  public string ShowFirstAndLastThreeAndCustomMaskInTheMiddle { get; set; }
}
```


## Masking a string property with regular expressions

Apply the `LogReplaced` attribute on a string, in which you want to apply a RegEx replacement during Logging.

This is applicable in scenarios which a string contains both Sensitive and Non-Sensitive information. An example of this could be a string such as "__Sensitive|NonSensitive__". Then you can apply the attibute like the following snippet:
```csharp
[LogReplaced(@"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)", "***|$2")]
public property Information { get; set; }

// Will log: "***|NonSensitive"
``` 

`LogReplaced` attribute is available with two constructors:

```csharp
LogReplaced(string pattern, string replacement)

LogReplaced(string pattern, string replacement, RegexOptions regexOptions)
```

 - **Pattern:** The pattern that should be applied on value..
 - **Replacement:** The string that will be applied by RegEx. 
 - **RegexOptions:** The [RegexOptions](https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regexoptions?view=netcore-3.1) that will be applied. Defaults to __RegexOptions.None__


### Examples

```csharp
public class CreditCard
{
  const string RegexWithVerticalBars = @"([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)\|([a-zA-Z0-9]+)";
  
  /// <summary>
  /// 123|456|789 results in "***|456|789"
  /// </summary>
  [LogReplaced(RegexWithVerticalBars, "***|$2|$3")]
  public string RegexReplaceFirst { get; set; }

  /// <summary>
  /// 123|456|789 results in "123|***|789"
  /// </summary>
  [LogReplaced(RegexWithVerticalBars, "$1|***|$3")]
  public string RegexReplaceSecond { get; set; }
}
```
