using Thorium.Core.DataProvider;

namespace Thorium.Core.MicroServices.Abstractions
{
    public interface IGenericRepositoryBase<TDbContext> : IRepositoryBase
        where TDbContext : ContextBase
    {
        TDbContext CreateDbContext();

        new TDbContext CurrentDbContext { get; set; }
    }
}