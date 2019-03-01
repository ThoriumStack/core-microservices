using System.IO;
using System.IO.Pipes;
using McMaster.Extensions.CommandLineUtils;
using Thorium.Core.MicroServices.Abstractions;

namespace Thorium.Core.MicroServices
{
    
    public class ReadyCheckCli: ICliExtender
    {
        public void ExtendCli(CommandLineApplication app)
        {
              app.Command("check-ready", (command) =>
            {
                command.Description = "Check if the service is ready.";
                command.HelpOption("-?|-h|--help");
                
                command.OnExecute(() =>
                {
                    var client = new NamedPipeClientStream($"service-readiness-check:{app.Name}");
                    client.Connect();
                    var reader = new StreamReader(client);
                    var val = reader.ReadLine();
                    var returnCode = 1;
                    int.TryParse(val, out returnCode);
           
                    return returnCode;
                });
            });
            
            app.Command("check-live", (command) =>
            {
                command.Description = "Check if the service is ready.";
                command.HelpOption("-?|-h|--help");
                
                command.OnExecute(() =>
                {
                    var client = new NamedPipeClientStream($"service-live-check:{app.Name}");
                    client.Connect();
                    var reader = new StreamReader(client);
                    var val = reader.ReadLine();
                    var returnCode = 1;
                    int.TryParse(val, out returnCode);
           
                    return returnCode;
                });
            });
        }
    }
}