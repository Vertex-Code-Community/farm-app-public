using AutoMapper;
using FarmApp.Entities.Entity;
using FarmApp.ViewModels.Media;

namespace FarmApp.BusinessLogicLayer.Profiles
{
    public class MediaProfile : Profile
    {
        public MediaProfile()
        {
            CreateMap<PropertyNoteMedia, UploadedMediaFile>();
            CreateMap<List<PropertyNoteMedia>, List<UploadedMediaFile>>();
        }
    }
}
