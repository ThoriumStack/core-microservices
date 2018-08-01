using AutoMapper;

namespace MyBucks.Core.MicroServices.Abstractions
{
    public interface IMapperFactory
    {
        IMapper GetMapper(string mapperName = "");
    }
}