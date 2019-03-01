using McMaster.Extensions.CommandLineUtils;

namespace Thorium.Core.MicroServices.Abstractions
{
    public interface ICliExtender
    {
        void ExtendCli(CommandLineApplication app);
    }
}