using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MyBucks.Core.MicroServices.Abstractions;
using MyBucks.Core.Model.DataModel;
using MyBucks.Core.Model.DtoModel;

namespace MyBucks.Core.MicroServices.Repositories
{
    public class Select<TModel> :
        ISelect<TModel> where TModel : BaseModel
    {
        private IQueryable<TModel> Set { get; set; }

        public Select(IQueryable<TModel> set)
        {
            Set = set;
        }

        public Type ElementType => Set.ElementType;

        public Expression Expression => Set.Expression;

        public IQueryProvider Provider => Set.Provider;

        public ISelect<TModel> Where(Expression<Func<TModel, bool>> predicate)
        {
            Set = Set.Where(predicate);

            return this;
        }

        public ISelect<TModel> Include<TProperty>(Expression<Func<TModel, TProperty>> path)
        {
            Set = Set.Include(path);

            return this;
        }

        public IQueryable<TDto> To<TDto>() where TDto : BaseDtoModel => Set.ProjectTo<TDto>();

        public IEnumerator<TModel> GetEnumerator() => Set.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count() => Set.Count();

        public ISelect<TModel> Take(int count)
        {
            Set = Set.Take(count);

            return this;
        }
    }
}