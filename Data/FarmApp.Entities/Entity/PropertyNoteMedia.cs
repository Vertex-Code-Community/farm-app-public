
using FarmApp.Entities.Interfaces;
using FarmApp.Shared.Enums;

namespace FarmApp.Entities.Entity
{
    public class PropertyNoteMedia : IBaseEntity<string>
    {
        public string Id { get; set; }
        public string PropertyNoteId { get; set; }
        public string ContentType { get; set; }
        public PropertyNoteEntity PropertyNote { get; set; }
        public MediaType MediaType { get; set; }
        public string RelativePath { get; set; }
    }
}
