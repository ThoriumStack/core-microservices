using System;
using AutoMapper;
using MyBucks.Core.DataProvider;
using MyBucks.Core.MicroServices.Abstractions;
using MyBucks.Core.MicroServices.ConfigurationModels;
using Serilog;

namespace MyBucks.Core.MicroServices.Repositories
{
    public abstract class GenericRepositoryBase<TDbContext> : RepositoryBase, IGenericRepositoryBase<TDbContext>
        where TDbContext : ContextBase
    {
        private readonly DbSettings _dbSettings;
        
        protected GenericRepositoryBase(DbSettings dbSetting, IMapper mapper, ILogger logger) : base (mapper, logger)
        {
            _dbSettings = dbSetting;
        }
        
        public virtual TDbContext CreateDbContext()
        {
            TDbContext context;
            if (!string.IsNullOrWhiteSpace(_dbSettings.ConnectionString))
                context = (TDbContext) Activator.CreateInstance(typeof(TDbContext), _dbSettings.ConnectionString);
            else
                context = Activator.CreateInstance<TDbContext>();
            
            CurrentDbContext = context;
            return context;
        }

        private TDbContext _context;

        public new virtual TDbContext CurrentDbContext
        {
            get => _context;
            set
            {
                _context = value;
                base.CurrentDbContext = value;
            }
        }
    }
}