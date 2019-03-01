using System.Collections.Generic;
using AutoMapper;
using Thorium.Core.MicroServices.Abstractions;

namespace Thorium.Core.MicroServices.Mappers
{
    public class MapperFactory : IMapperFactory
    {
        public Dictionary<string, IMapper> Mappers { get; set; } = new Dictionary<string, IMapper>();
        public IMapper GetMapper(string mapperName)
        {
            return Mappers[mapperName];
        }
    }
}