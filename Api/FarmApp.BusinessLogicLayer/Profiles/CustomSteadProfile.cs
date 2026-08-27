using AutoMapper;
using FarmApp.Entities.Entity;
using FarmApp.ViewModels.CustomSteads;

namespace FarmApp.BusinessLogicLayer.Profiles;

public class CustomSteadProfile : Profile
{
    public CustomSteadProfile()
    {
        CreateMap<CustomSteadModel, CustomSteadEntity>();
        CreateMap<CustomSteadEntity, CustomSteadModel>();
    }
}