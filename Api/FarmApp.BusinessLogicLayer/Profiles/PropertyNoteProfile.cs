using AutoMapper;
using FarmApp.Entities.Entity;
using FarmApp.ViewModels.PropertyNotes;

namespace FarmApp.BusinessLogicLayer.Profiles;

public class PropertyNoteProfile : Profile
{
    public PropertyNoteProfile()
    {
        CreateMap<PropertyNoteEntity, PropertyNoteModel>();
        CreateMap<PropertyNoteEntity, PropertyNotePreviewModel>();

        CreateMap<PropertyNoteModel, PropertyNoteEntity>(); 
        
        CreateMap<CreatePropertyNoteModel, PropertyNoteEntity>();
    }
}