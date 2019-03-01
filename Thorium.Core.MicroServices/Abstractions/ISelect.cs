using System.Linq;
using Thorium.Core.Model.DataModel;
using Thorium.Core.Model.DtoModel;

namespace Thorium.Core.MicroServices.Abstractions
{
    public interface ISelect<TModel> :
        IQueryable<TModel> where TModel : BaseModel
    {
        //ISelect<TModel> Where(Expression<Func<TModel, bool>> predicate);

        //ISelect<TModel> Include<TProperty>(Expression<Func<TModel, TProperty>> path);

        IQueryable<TDto> To<TDto>() where TDto : BaseDtoModel;

        //int Count();

        //ISelect<TModel> Take(int count);
    }
}