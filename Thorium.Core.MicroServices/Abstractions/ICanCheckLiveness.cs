using Thorium.Core.MicroServices.LivenessChecks;

namespace Thorium.Core.MicroServices.Abstractions
{
    public interface ICanCheckLiveness
    {
        void ConfigureLivenessChecks(LivenessCheckConfiguration config);
    }
}