using AutoMapper;
using FarmApp.Entities.Entity;
using FarmApp.ViewModels.Steads;

namespace FarmApp.BusinessLogicLayer.Profiles;

public class SteadProfile : Profile
{
    public SteadProfile()
    {
        CreateMap<SteadModel, SteadEntity>();
        CreateMap<SteadEntity, SteadModel>();
    }
}
