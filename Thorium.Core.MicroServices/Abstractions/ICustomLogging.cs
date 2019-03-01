using Serilog;

namespace Thorium.Core.MicroServices.Abstractions
{
    /// <summary>
    /// Customize serilog in your micro service.
    /// </summary>
    public interface ICustomLogging
    {
        void ConfigureLogging(LoggerConfiguration logger);
    }
}