namespace MyBucks.Core.MicroServices.Abstractions
{
    public interface IServiceEndpoint
    {
        void StartServer();
        void StopServer();
        string EndpointDescription { get; }
        //bool Ready { get; }
        
    }
}