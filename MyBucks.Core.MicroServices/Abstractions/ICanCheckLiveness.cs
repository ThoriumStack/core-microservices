using MyBucks.Core.MicroServices.LivenessChecks;

namespace MyBucks.Core.MicroServices.Abstractions
{
    public interface ICanCheckLiveness
    {
        void ConfigureLivenessChecks(LivenessCheckConfiguration config);
    }
}