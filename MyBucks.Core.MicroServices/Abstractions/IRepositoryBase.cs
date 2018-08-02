using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyBucks.Core.DataProvider;
using MyBucks.Core.MicroServices.Repositories;
using MyBucks.Core.Model.Abstractions;
using MyBucks.Core.Model.DataModel;

namespace MyBucks.Core.MicroServices.Abstractions
{
    public interface IRepositoryBase
    {
        string CurrentUserId { get; set; }
        string CurrentContext { get; set; }
        ContextBase CurrentDbContext { get; set; }

        event EventHandler<PreFilterEventArgs> PreFilter;
        
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken());
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken());

        ISelect<TModel> Select<TModel>() where TModel : BaseModel;
        ISelect<TModel> SelectNoTracking<TModel>() where TModel : BaseModel;

        TDto GetAs<TModel, TDto>(int id) where TModel : BaseModel where TDto : IBaseDtoModel;
        Task<TDto> GetAsAsync<TModel, TDto>(int id) where TModel : BaseModel where TDto : IBaseDtoModel;
        TModel Get<TModel>(int id) where TModel : BaseModel;
        Task<TModel> GetAsync<TModel>(int id) where TModel : BaseModel;
        
        void Insert<TModel>(TModel model) where TModel : BaseModel;
        Task InsertAsync<TModel>(TModel model) where TModel : BaseModel;
        
        void InsertRange<TModel>(IEnumerable<TModel> models) where TModel : BaseModel;
        Task InsertRangeAsync<TModel>(IEnumerable<TModel> models) where TModel : BaseModel;
        
        void Delete<TModel>(int id) where TModel : BaseModel;
        void Delete<TModel>(TModel model) where TModel : BaseModel;
        void DeleteRange<TModel>(IEnumerable<TModel> models) where TModel : BaseModel;

        TModel Map<TDto, TModel>(TDto dto, TModel model) where TDto : IBaseDtoModel where TModel : BaseModel;
        TDestination To<TDestination>(object source);
    }
}