using System.Linq;
using AutoMapper;
using MyBucks.Core.Model.Abstractions;
using Serilog;

namespace MyBucks.Core.MicroServices.Repositories
{
    public abstract class ContextRepositoryBase : RepositoryBase
    {
        protected ContextRepositoryBase(IMapper mapper, ILogger logger) :
            base(mapper, logger)
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