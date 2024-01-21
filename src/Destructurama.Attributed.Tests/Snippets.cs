using NUnit.Framework;
using Serilog;

namespace Destructurama.Attributed.Tests;

#region WithRegex

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

#endregion

public class Snippets
{
    #region LoginCommand
    public class LoginCommand
    {
        public string? Username { get; set; }

        [NotLogged]
        public string? Password { get; set; }
    }
    #endregion

    private static readonly ILogger _log = Log.ForContext<Snippets>();

    [Test]
    public void LogCommand()
    {
        #region LogCommand
        var command = new LoginCommand { Username = "logged", Password = "not logged" };
        _log.Information("Logging in {@Command}", command);
        #endregion
    }
}
