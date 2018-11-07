using System;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace MyBucks.Core.MicroServices
{
    public class ServiceCommandLine
    {
        private readonly CommandLineApplication _app;

        public  ServiceCommandLine(string appName)
        {
            
            _app = new CommandLineApplication();
            _app.Name = appName;
            _app.HelpOption("-?|-h|--help");
        }

        public void Command(string name, Action<CommandLineApplication> configuration,
            bool throwOnUnexpectedArg = true)
        {
            _app.Command(name, configuration, throwOnUnexpectedArg);
        }

        public bool Run(string[] args)
        {
            if (args.Length == 0)
            {
                return false;
            };

            if (!_app.Commands.Any())
            {
                Console.WriteLine("This service has no command line options.");
                return false;
            }
            
            _app.OnExecute(() => {
                _app.HelpTextGenerator.Generate(_app, Console.Out);
                return 0;
            });

            var returnCode = 1;

            try
            {
                returnCode = _app.Execute(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                
            }

            return true;
        }
    }
}