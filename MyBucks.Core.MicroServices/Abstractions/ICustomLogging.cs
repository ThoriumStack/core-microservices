using Serilog;

namespace MyBucks.Core.MicroServices.Abstractions
{
    /// <summary>
    /// Customize serilog in your micro service.
    /// </summary>
    public interface ICustomLogging
    {
        void ConfigureLogging(ILogger logger);
    }
}