using System;
using AutoMapper;
using Serilog;
using Thorium.Core.DataProvider;
using Thorium.Core.MicroServices.Abstractions;
using Thorium.Core.MicroServices.ConfigurationModels;

namespace Thorium.Core.MicroServices.Repositories
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
            
            context.CurrentContext = CurrentContext;
            context.CurrentUserId = CurrentUserId;
            
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