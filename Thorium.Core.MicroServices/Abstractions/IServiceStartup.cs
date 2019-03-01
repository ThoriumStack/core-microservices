namespace Thorium.Core.MicroServices.Abstractions
{
    public interface IServiceStartup
    {
        void ConfigureService(ServiceConfiguration configuration);

    }
}