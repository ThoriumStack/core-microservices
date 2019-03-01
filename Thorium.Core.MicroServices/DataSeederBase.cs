using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Thorium.Core.DataIntegration;
using Thorium.Core.DataIntegration.Serializers.CsvSerializer;
using Thorium.Core.DataIntegration.Transports;

namespace Thorium.Core.MicroServices
{
    public class DataSeederBase
    {
        private readonly DbContext _context;
        private readonly ILogger _logger;
        private readonly string _seedDataPath;

        protected DataSeederBase(ILogger logger, DbContext context, string seedDataPath)
        {
            _context = context;
            _logger = logger;
            _seedDataPath = seedDataPath;
        }

        protected void SeedCollectionFromCsv<TSeedType, TEntityType>() where TEntityType : class where TSeedType : new()
        {

            var dbSet = _context.Set<TEntityType>();

            if (dbSet.Any())
            {
                return;
            }

            var lst = new List<TSeedType>();

            var builder = new InputBuilder()
                .SetSerializer(new CsvSerializer())
                .ReadAll(lst);


            var transport = new LocalFileTransport
            {
                FilePath = Path.Combine(_seedDataPath, "DataSeeds", $"{typeof(TEntityType).Name}.csv")
            };
            var integrator = new Integrator();
            integrator.ReceiveData(builder, transport);

            _logger.Information($"{typeof(TEntityType).Name} Table Empty. Seeding...");
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<TSeedType, TEntityType>(); });

            var iMapper = config.CreateMapper();
            dbSet.AddRange(lst.Select(c => iMapper.Map<TSeedType, TEntityType>(c)));
            _context.SaveChanges();
            _logger.Information($"Seeding {typeof(TEntityType).Name} table completed successfully!");
        }
    }
}