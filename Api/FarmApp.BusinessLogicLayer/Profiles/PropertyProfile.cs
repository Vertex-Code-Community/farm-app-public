using AutoMapper;
using FarmApp.Entities.Entity;
using FarmApp.ViewModels.Properties;

namespace FarmApp.BusinessLogicLayer.Profiles;

public class PropertyProfile : Profile
{
    public PropertyProfile()
    {
        CreateMap<PropertyEntity, PropertyModel>().ForMember(
            dest => dest.HasNotes,
            opt => opt.MapFrom(src => src.PropertyNotes.Count > 0));
        
        CreateMap<PropertyEntity, PropertyPreviewModel>().ForMember(
            dest => dest.Notes,
            opt => opt.MapFrom(src => src.PropertyNotes));
        CreateMap<PropertyModel, PropertyEntity>(); 
        
        CreateMap<PropertyAndSteadEntity, PropertySteadModel>();
        CreateMap<PropertySteadModel, PropertyAndSteadEntity>();
    }
}
