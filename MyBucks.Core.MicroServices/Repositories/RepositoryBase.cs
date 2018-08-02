using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyBucks.Core.DataProvider;
using Serilog;

using MyBucks.Core.MicroServices.Abstractions;
using MyBucks.Core.Model.Abstractions;
using MyBucks.Core.Model.DataModel;
using MyBucks.Core.MicroServices.Mappers;

namespace MyBucks.Core.MicroServices.Repositories
{
    public abstract class RepositoryBase : IRepositoryBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        protected RepositoryBase(IMapper mapper, ILogger logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        private string _currentUserId;
        public string CurrentUserId
        {
            get => _currentUserId;
            set
            {
                _currentUserId = value;
                if (CurrentDbContext != null)
                    CurrentDbContext.CurrentUserId = _currentUserId;
            }
        }

        private string _currentContext;

        public string CurrentContext
        {
            get => _currentContext;
            set
            {
                _currentContext = value;
                if (CurrentDbContext != null)
                    CurrentDbContext.CurrentContext = _currentContext;
            }
        }
        
        public virtual ContextBase CurrentDbContext { get; set; }

        public event EventHandler<PreFilterEventArgs> PreFilter = delegate { };
        
        private PreFilterEventArgs RaisePreFilterEvent<TModel>(IQueryable<TModel> set) where TModel : BaseModel
        {
            var e = new PreFilterEventArgs(set, typeof(TModel));
            PreFilter(this, e);

            return e;
        }
        
        public ISelect<TModel> Select<TModel>() where TModel : BaseModel
        {
            var e = RaisePreFilterEvent(CurrentDbContext.Set<TModel>());

            return new Select<TModel>(e.Filtered.Cast<TModel>());
        }
        
        public ISelect<TModel> SelectNoTracking<TModel>() where TModel : BaseModel
        {
            var e = RaisePreFilterEvent(CurrentDbContext.Set<TModel>().AsNoTracking());

            return new Select<TModel>(e.Filtered.Cast<TModel>());
        }

        public TDto GetAs<TModel, TDto>(int id) where TModel : BaseModel where TDto : IBaseDtoModel
        {
            var model = Select<TModel>().SingleOrDefault(m => m.Id == id);
            return To<TDto>(model);
        }

        public Task<TDto> GetAsAsync<TModel, TDto>(int id) where TModel : BaseModel where TDto : IBaseDtoModel
        {
            return _mapper.MapAsync<TModel, TDto>(GetAsync<TModel>(id));
        }

        public TModel Get<TModel>(int id) where TModel : BaseModel
        {
            return Select<TModel>().SingleOrDefault(x => x.Id == id);
        }

        public Task<TModel> GetAsync<TModel>(int id) where TModel : BaseModel
        {
            return Select<TModel>().SingleOrDefaultAsync(x => x.Id == id);
        }

        public void Insert<TModel>(TModel model) where TModel : BaseModel
        {
            CurrentDbContext.Set<TModel>().Add(model);
        }
        
        public Task InsertAsync<TModel>(TModel model) where TModel : BaseModel
        {
            return CurrentDbContext.Set<TModel>().AddAsync(model);
        }
        
        public void InsertRange<TModel>(IEnumerable<TModel> models) where TModel : BaseModel
        {
            CurrentDbContext.Set<TModel>().AddRange(models);
        }
        
        public Task InsertRangeAsync<TModel>(IEnumerable<TModel> models) where TModel : BaseModel
        {
            return CurrentDbContext.Set<TModel>().AddRangeAsync(models);
        }

        public void Delete<TModel>(int id) where TModel : BaseModel
        {
            Delete(CurrentDbContext.Set<TModel>().Find(id));
        }

        public void Delete<TModel>(TModel model) where TModel : BaseModel
        {
            CurrentDbContext.Set<TModel>().Remove(model);
        }

        public void DeleteRange<TModel>(IEnumerable<TModel> models) where TModel : BaseModel
        {
            CurrentDbContext.Set<TModel>().RemoveRange(models);
        }
        
        /// <summary>
        /// Maps a dto model object to a model object, and returns the result
        /// </summary>
        /// <param name="dto">Dto model object to map from</param>
        /// <param name="model">Model object to map to and return</param>
        /// <returns></returns>
        public TModel Map<TDto, TModel>(TDto dto, TModel model) where TDto : IBaseDtoModel where TModel : BaseModel => _mapper.Map(dto, model);

        public TDestination To<TDestination>(object source) => _mapper.Map<TDestination>(source);
        
        public int SaveChanges() => CurrentDbContext.SaveChanges();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken()) =>
            CurrentDbContext.SaveChangesAsync(cancellationToken);

        public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken()) =>
            CurrentDbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}