using MyBucks.Core.MicroServices.ConfigurationModels;
using Serilog;

namespace MyBucks.Core.MicroServices
{
    public interface ISeedable
    {
        void SeedData(DbSettings databaseSettings, ILogger logger);
    }
}