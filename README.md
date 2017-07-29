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

