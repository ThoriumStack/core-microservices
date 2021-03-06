﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Thorium.Core.MicroServices.Abstractions;
using Thorium.Core.Model.DataModel;
using Thorium.Core.Model.DtoModel;

namespace Thorium.Core.MicroServices.Repositories
{
    public class Select<TModel> :
        ISelect<TModel> where TModel : BaseModel
    {
        private readonly IMapper _mapper;
        private IQueryable<TModel> Set { get; set; }

        public Select(IMapper mapper, IQueryable<TModel> set)
        {
            _mapper = mapper;
            Set = set;
        }

        public Type ElementType => Set.ElementType;

        public Expression Expression => Set.Expression;

        public IQueryProvider Provider => Set.Provider;

//        public ISelect<TModel> Where(Expression<Func<TModel, bool>> predicate)
//        {
//            Set = Set.Where(predicate);
//
//            return this;
//        }
//
//        public ISelect<TModel> Include<TProperty>(Expression<Func<TModel, TProperty>> path)
//        {
//            Set = Set.Include(path);
//
//            return this;
//        }

        public IQueryable<TDto> To<TDto>() where TDto : BaseDtoModel => Set.ProjectTo<TDto>(_mapper.ConfigurationProvider);

        public IEnumerator<TModel> GetEnumerator() => Set.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        //public int Count() => Set.Count();

//        public ISelect<TModel> Take(int count)
//        {
//            Set = Set.Take(count);
//
//            return this;
//        }
    }
}