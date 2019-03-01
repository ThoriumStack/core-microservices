using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using Thorium.Core.MicroServices.Abstractions;

namespace Thorium.Core.MicroServices.Endpoints
{
    public class LiveCheckEndpoint : IServiceEndpoint
    {
        private readonly ILogger _logger;
        private NamedPipeServerStream _server;
        private bool _running = true;
        private ILiveChecker _checker;


        public LiveCheckEndpoint(ILiveChecker checker)
        {
            _checker = checker;
        }

        public void StartServer()
        {
            
            _server = new NamedPipeServerStream(
                $"service-live-check:{Assembly.GetEntryAssembly().GetName().Name}");
            Task.Run(() =>
            {
                while (_running)
                {
                    Ready = true;
                    _server.WaitForConnection();
                    StreamWriter writer = new StreamWriter(_server);
                    writer.WriteLine(_checker.RunChecks() ? "0" : "1");
                    writer.Flush();
                    _server.Disconnect();
                }
            });
        }


        public void StopServer()
        {
            _running = false;
            _server.Close();
        }

        public string EndpointDescription => "Live Check Endpoint";
        public bool Ready { get; private set; }
        public bool ServiceReadyStatus { get; set; }
    }
}