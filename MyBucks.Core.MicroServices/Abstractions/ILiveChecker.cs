namespace MyBucks.Core.MicroServices.Abstractions
{
    public interface ILiveChecker
    {
        void Build();
        bool RunChecks();
    }
}