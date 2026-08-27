
using AutoMapper;
using FarmApp.Entities.Entity;
using FarmApp.ViewModels.PropertyNoteStatuses;

namespace FarmApp.BusinessLogicLayer.Profiles
{
    public class PropertyNoteStatusProfile : Profile
    {
        public PropertyNoteStatusProfile()
        {
            CreateMap<PropertyNoteStatusEntity, PropertyNoteStatusModel>();
            CreateMap<PropertyNoteStatusEntity, CreatePropertyNoteStatusModel>();
            CreateMap<CreatePropertyNoteStatusModel, PropertyNoteStatusModel>();
            CreateMap<CreatePropertyNoteStatusModel, PropertyNoteStatusEntity>();
        }
    }
}
