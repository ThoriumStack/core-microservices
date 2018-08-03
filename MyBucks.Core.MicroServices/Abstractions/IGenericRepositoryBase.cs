using MyBucks.Core.DataProvider;

namespace MyBucks.Core.MicroServices.Abstractions
{
    public interface IGenericRepositoryBase<TDbContext> : IRepositoryBase
        where TDbContext : ContextBase
    {
        TDbContext CreateDbContext();

        new TDbContext CurrentDbContext { get; set; }
    }
}