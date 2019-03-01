using AutoMapper;

namespace Thorium.Core.MicroServices.Abstractions
{
    public interface IMapperFactory
    {
        IMapper GetMapper(string mapperName = "");
    }
}