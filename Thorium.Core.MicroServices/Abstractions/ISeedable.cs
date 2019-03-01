using System.Collections.Generic;
using Serilog;
using Thorium.Core.MicroServices.ConfigurationModels;

namespace Thorium.Core.MicroServices.Abstractions
{
    public interface ISeedable
    {
        void SeedData(List<DbSettings> databaseSettings, ILogger logger);
    }
}