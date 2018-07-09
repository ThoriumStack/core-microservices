using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyBucks.Core.DataIntegration;
using MyBucks.Core.DataIntegration.Transports;
using MyBucks.Core.Serializers.CsvSerializer;
using Serilog;

namespace MyBucks.Core.MicroServices
{
    public class DataSeederBase
    {
        private DbContext _context;
        private ILogger _logger;

        protected DataSeederBase(ILogger logger, DbContext context)
        {
            _context = context;
            _logger = logger;
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
                FilePath = Path.Combine(Directory.GetCurrentDirectory(), "DataSeeds", $"{typeof(TEntityType).Name}.csv")
            };
            var integrator = new Integrator();
            integrator.ReceiveData(builder, transport);

            _logger.Information($"{typeof(TEntityType).Name} Table Empty. Seeding...");
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<TSeedType, TEntityType>(); });

            IMapper iMapper = config.CreateMapper();
            dbSet.AddRange(lst.Select(c => iMapper.Map<TSeedType, TEntityType>(c)));
            _context.SaveChanges();
            _logger.Information($"Seeding {typeof(TEntityType).Name} table completed succesfully!");
        }
    }
}