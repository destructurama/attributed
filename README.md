### Destructurama.Attributed [![Build status](https://ci.appveyor.com/api/projects/status/1tutmofqjb9wq627?svg=true)](https://ci.appveyor.com/project/Destructurama/attributed)

This package makes it possible to manipulate how objects are logged to [Serilog](http://serilog.net) using attributes.

#### Enabling the module:

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

#### Ignoring a property

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

#### Treating types and properties as scalars

To prevent destructuring of a type or property at all, apply the `[LogAsScalar]` attribute.

#### Masking a string property

Apply the `LogMasked` attribute with various settings:

 - **Text**: If set the properties value will be set to this text.
 - **ShowFirst**: Shows the first x characters in the property value 
 - **ShowLast**: Shows the last x characters in the property value 
 - **PreserveLength**: If set it will swap out each character with the default value. Note that this property will be ignored if Text has been set to custom value.

 **Examples**

```csharp
public class Creditcard
{
  /// <summary>
  /// 123456789 results in "*********"
  /// </summary>
  [LogMasked]
  public string DefaultMasked { get; set; }
  
  /// <summary>
  ///  123456789 results in "REMOVED"
  /// </summary>
  [LogMasked(Text: "REMOVED")]
  public string CustomMasked { get; set; }
  
  /// <summary>
  ///  123456789 results in "123***"
  /// </summary>
  [LogMasked(ShowFirst: 3)]
  public string ShowFirstThreeThenDefaultMasked { get; set; }

  /// <summary>
  ///  123456789 results in "123******"
  /// </summary>
  [LogMasked(ShowFirst: 3, PreserveLength = true)]
  public string ShowFirstThreeThenDefaultMaskedPreserveLength { get; set; }

  /// <summary>
  /// 123456789 results in "***789"
  /// </summary>
  [LogMasked(ShowLast: 3)]
  public string ShowLastThreeThenDefaultMasked { get; set; }
  
  /// <summary>
  /// 123456789 results in "******789"
  /// </summary>
  [LogMasked(ShowLast: 3, PreserveLength = true)]
  public string ShowLastThreeThenDefaultMaskedPreserveLength  { get; set; }

  /// <summary>
  ///  123456789 results in "123REMOVED"
  /// </summary>
  [LogMasked(Text: "REMOVED", ShowFirst: 3)]
  public string ShowFirstThreeThenCustomMask { get; set; }

  /// <summary>
  ///  123456789 results in "REMOVED789"
  /// </summary>
  [LogMasked(Text: "REMOVED", ShowLast: 3)]
  public string ShowLastThreeThenCustomMask { get; set; }
  
  /// <summary>
  ///  123456789 results in "******789"
  /// </summary>
  [LogMasked(ShowLast: 3, PreserveLength = true)]
  public string ShowLastThreeThenCustomMaskPreserveLength { get; set; }

  /// <summary>
  ///  123456789 results in "123******"
  /// </summary>
  [LogMasked(ShowFirst: 3, PreserveLength = true)]
  public string ShowFirstThreeThenCustomMaskPreserveLength { get; set; }

  /// <summary>
  /// 123456789 results in "123***789"
  /// </summary>
  [LogMasked(ShowFirst: 3, ShowLast: 3)]
  public string ShowFirstAndLastThreeAndDefaultMaskeInTheMiddle { get; set; }

  /// <summary>
  ///  123456789 results in "123REMOVED789"
  /// </summary>
  [LogMasked(Text: "REMOVED", ShowFirst: 3, ShowLast: 3)]
  public string ShowFirstAndLastThreeAndCustomMaskInTheMiddle { get; set; }

  /// <summary>
  ///  NOTE PreserveLength = true is ignored in this case
  ///  123456789 results in "123REMOVED789"
  /// </summary>
  [LogMasked(Text: "REMOVED", ShowFirst: 3, ShowLast: 3, PreserveLength = true)]
  public string ShowFirstAndLastThreeAndCustomMaskInTheMiddle { get; set; }
}
```



