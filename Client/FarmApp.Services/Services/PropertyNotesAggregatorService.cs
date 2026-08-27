using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.PropertyNotes;

namespace FarmApp.Services.Services;

public class PropertyNotesAggregatorService : IPropertyNotesAggregatorService
{
    private readonly IStateService _stateService;
    private readonly IPropertyService _propertyService;

    public PropertyNotesAggregatorService(
        IStateService stateService,
        IPropertyService propertyService)
    {
        _stateService = stateService;
        _propertyService = propertyService;
    }

    public List<PropertyNoteModel> GetNoteModelByPreviewModel(List<PropertyNotePreviewModel> propertyNotes, string propertyId)
    {
        var result = new List<PropertyNoteModel>();
        foreach(var note in propertyNotes)
        {
            result.Add(new PropertyNoteModel
            {
                CreatedAt = note.CreatedAt,
                EndTime = note.EndTime,
                Header = note.Header,
                StartTime = note.StartTime,
                StatusId = note.StatusId,
                Id = note.Id,
                PropertyId = propertyId,
                Text = note.Text,
                PreviewMediaId = note.PreviewMediaId,
                NotificationsEnabled = note.NotificationsEnabled,
                NotifyBeforeStart = note.NotifyBeforeStart,
                NotifyBeforeEnd = note.NotifyBeforeEnd
            });
        }
        return result;
    }

    public async Task<List<PropertyNoteModel>> LoadAllAsync()
    {
        if (!_stateService.ArePropertiesReady)
            return new List<PropertyNoteModel>();

        var result = new List<PropertyNoteModel>();

        foreach (var property in _stateService.Properties)
        {
            if (!property.HasNotes)
                continue;

            var preview = _stateService.GetPropertyPreview(property.Id)
                          ?? await _propertyService.GetPreviewByIdAsync(property.Id);

            if (preview == null)
                continue;

            _stateService.AddPropertyNotes(preview);

            foreach(var note in preview.Notes)
            {
                result.Add(new PropertyNoteModel
                {
                    CreatedAt = note.CreatedAt,
                    EndTime = note.EndTime,
                    Header = note.Header,
                    Id = note.Id,
                    PreviewMediaId = note.PreviewMediaId,
                    PropertyId = property.Id,
                    StartTime = note.StartTime,
                    StatusId = note.StatusId,
                    Text = note.Text
                });
            }
            
        }

        return result
            .GroupBy(n => n.Id)
            .Select(g => g.First())
            .ToList();
    }
}
