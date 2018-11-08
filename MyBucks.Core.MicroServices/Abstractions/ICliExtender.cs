using McMaster.Extensions.CommandLineUtils;

namespace MyBucks.Core.MicroServices.Abstractions
{
    public interface ICliExtender
    {
        void ExtendCli(CommandLineApplication app);
    }
}