using System;
using System.Threading;
using MyBucks.Core.MicroServices;
using MyBucks.Core.MicroServices.Abstractions;
using MyBucks.Core.MicroServices.LivenessChecks;

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
          //  configuration.Inject<ILivenessCheck, DatabaseLivenessCheck<PoefDbContext>>();
            
        }

        public void ConfigureLivenessChecks(LivenessCheckConfiguration config)
        {
           // config.AddCheck< DatabaseLivenessCheck<PoefDbContext>>();
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