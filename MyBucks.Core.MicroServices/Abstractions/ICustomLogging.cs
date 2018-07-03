using Serilog;

namespace MyBucks.Core.MicroServices.Abstractions
{
    public interface ICustomLogging
    {
        void ConfigureLogging(ILogger logger);
    }
}