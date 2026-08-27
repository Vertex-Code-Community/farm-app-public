using FarmApp.ViewModels.PropertyNotes;

namespace FarmApp.Services.Services.Interfaces;

public interface IPropertyNotesAggregatorService
{
    Task<List<PropertyNoteModel>> LoadAllAsync();
    List<PropertyNoteModel> GetNoteModelByPreviewModel(List<PropertyNotePreviewModel> propertyNotes, string propertyId);
}
