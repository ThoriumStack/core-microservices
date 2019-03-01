using System.Linq;
using AutoMapper;
using Serilog;
using Thorium.Core.DataProvider;
using Thorium.Core.MicroServices.ConfigurationModels;
using Thorium.Core.Model.Abstractions;

namespace Thorium.Core.MicroServices.Repositories
{
    public abstract class GenericContextRepositoryBase<TDbContext> : GenericRepositoryBase<TDbContext>
        where TDbContext : ContextBase
    {
        protected GenericContextRepositoryBase(DbSettings dbSettings, IMapper mapper, ILogger logger) :
            base(dbSettings, mapper, logger)
        {
            PreFilter += OnPreFilter;
        }

        protected virtual void ApplyContextFilter(object sender, PreFilterEventArgs e)
        {
            if (typeof(IBaseContextModel).IsAssignableFrom(e.Type))
            {
                var applicationModels = (IQueryable<IBaseContextModel>)e.Filtered;

                var filtered = applicationModels.Where(model => model.Context == null ||
                                                                model.Context == CurrentContext);

                e.Filtered = filtered;
            }
        }

        protected virtual void OnPreFilter(object sender, PreFilterEventArgs e)
        {
            // TODO: Maybe add default filter on RepositoryBase, e.g. security filters
            //base.OnPreFilter(sender, e);
            ApplyContextFilter(sender, e);
        }
    }
}