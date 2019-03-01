namespace Thorium.Core.MicroServices.Abstractions
{
    public interface ILiveChecker
    {
        void Build();
        bool RunChecks();
    }
}