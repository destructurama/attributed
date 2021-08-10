using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using System.Linq;
using Serilog.Core;

namespace Destructurama.Attributed.Tests
{
    public class Snippets
    {
        #region LoginCommand
        public class LoginCommand
        {
            public string Username { get; set; }

            [NotLogged]
            public string Password { get; set; }
        }
        #endregion
        
        static ILogger log = Log.ForContext<Snippets>();
        
        [Test]
        public void LogCommand()
        {
            #region LogCommand
            var command = new LoginCommand { Username = "logged", Password = "not logged" };
            log.Information("Logging in {@Command}", command);
            #endregion
        }
    }
}