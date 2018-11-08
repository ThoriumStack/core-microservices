using System;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Threading.Tasks;
using MyBucks.Core.MicroServices.Abstractions;

namespace MyBucks.Core.MicroServices.Endpoints
{
    public class ReadyCheckEndpoint : IServiceEndpoint
    {
        private NamedPipeServerStream _server;
        private bool _running = true;

        public void StartServer()
        {
            _server = new NamedPipeServerStream(
                $"service-readiness-check:{Assembly.GetEntryAssembly().GetName().Name}");
            Task.Run(() =>
            {
                while (_running)
                {
                    Ready = true;
                    _server.WaitForConnection();
                    StreamWriter writer = new StreamWriter(_server);
//                    Console.WriteLine($"Ready check: {ServiceReadyStatus}");
                    // var line = reader.ReadLine();
                    writer.WriteLine(ServiceReadyStatus ? "0" : "1");
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

        public string EndpointDescription => "Ready Check Endpoint";
        public bool Ready { get; private set; }
        public bool ServiceReadyStatus { get; set; }
    }
}