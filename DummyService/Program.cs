using System.Threading;
using Thorium.Core.MicroServices;
using Thorium.Core.MicroServices.Abstractions;
using Thorium.Core.MicroServices.LivenessChecks;

namespace DummyService
{
    class Program
    {
        static int Main(string[] args)
        {
            var svcRunner = new ServiceRunner();
            
            
            var returnCode =  svcRunner.Run(new DummyStartup(), args);
            return returnCode;
        }
    }

    public class DummyStartup : IServiceStartup, ICanCheckLiveness
    {
        public void ConfigureService(ServiceConfiguration configuration)
        {
            configuration.AddServiceEndpoint<DummyEndpoint>();
            
        }

        public void ConfigureLivenessChecks(LivenessCheckConfiguration config)
        {
            config.AddCheck<ExampleLiveCheck>();
        }
    }

    public class ExampleLiveCheck: ILivenessCheck
    {
        public bool IsLive()
        {
            return false;
        }
    }

    public class DummyEndpoint: IServiceEndpoint
    {
        public void StartServer()
        {
            Thread.Sleep(60000);
        }

        public void StopServer()
        {
            Thread.Sleep(1000);
        }

        public string EndpointDescription => "Dummy Endpoint";
        
    }
}