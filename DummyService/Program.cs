using System;
using System.Threading;
using MyBucks.Core.MicroServices;
using MyBucks.Core.MicroServices.Abstractions;

namespace DummyService
{
    class Program
    {
        static void Main(string[] args)
        {
            var svcRunner = new ServiceRunner();
            
            svcRunner.Run(new DummyStartup());
        }
    }

    public class DummyStartup : IServiceStartup
    {
        public void ConfigureService(ServiceConfiguration configuration)
        {
            configuration.AddServiceEndpoint<DummyEndpoint>();
        }
    }

    public class DummyEndpoint: IServiceEndpoint
    {
        public void StartServer()
        {
            Thread.Sleep(1000);
        }

        public void StopServer()
        {
            Thread.Sleep(1000);
        }

        public string EndpointDescription => "Dummy Endpoint";
        
    }
}