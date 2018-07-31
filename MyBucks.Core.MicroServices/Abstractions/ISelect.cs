using System;
using System.Linq;
using System.Linq.Expressions;
using MyBucks.Core.Model.DataModel;
using MyBucks.Core.Model.DtoModel;

namespace MyBucks.Core.MicroServices.Abstractions
{
    public interface ISelect<TModel> :
        IQueryable<TModel> where TModel : BaseModel
    {
        ISelect<TModel> Where(Expression<Func<TModel, bool>> predicate);

        ISelect<TModel> Include<TProperty>(Expression<Func<TModel, TProperty>> path);

        IQueryable<TDto> To<TDto>() where TDto : BaseDtoModel;

        int Count();

        ISelect<TModel> Take(int count);
    }
}