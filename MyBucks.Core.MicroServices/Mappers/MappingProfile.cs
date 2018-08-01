using AutoMapper;
using MyBucks.Core.Model.DataModel;
using MyBucks.Core.Model.DtoModel;

namespace MyBucks.Core.MicroServices.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<BaseModel, BaseDtoModel>().ReverseMap();
            CreateMap<BaseContextModel, BaseContextDtoModel>().ReverseMap();
        }
    }
}