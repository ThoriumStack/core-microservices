using System.Collections.Generic;
using MyBucks.Core.MicroServices.ConfigurationModels;
using Serilog;

namespace MyBucks.Core.MicroServices.Abstractions
{
    public interface ISeedable
    {
        void SeedData(List<DbSettings> databaseSettings, ILogger logger);
    }
}