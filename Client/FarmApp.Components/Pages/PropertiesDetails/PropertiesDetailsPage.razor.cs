using FarmApp.Components.Components.NotesListSection;
using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Properties;
using FarmApp.ViewModels.PropertyNotes;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Pages.PropertiesDetails
{
    public partial class PropertiesDetailsPage : IDisposable
    {
        [Inject] public required IStateService StateService { get; set; }
        [Inject] public required IPropertyService PropertyService { get; set; }
        [Inject] public required IPropertyNotesAggregatorService AggregatorService { get; set; }
        [Parameter] public string PropertyId { get; set; } = string.Empty;
        private List<PropertyNoteModel> _userNotes = [];
        private PropertyViewModel? _property = new();

        private NotesListSectionComponent? _notesSection;

        private bool _isLoading = false;

        private string EffectivePropertyId => _property?.Id ?? PropertyId;

        protected override async Task OnInitializedAsync()
        {
            StateService.OnPropertyNoteAdded += OnPropertyNoteAddedHandler;

            _isLoading = true;

            await Task.WhenAll(Task.Delay(600), LoadDataAsync());
            
            _isLoading = false;
        }

        private void OnPropertyNoteAddedHandler()
        {
            var id = EffectivePropertyId;
            if (string.IsNullOrEmpty(id))
                return;

            var preview = StateService.GetPropertyPreview(id);
            if (preview is null)
                return;

            _userNotes = AggregatorService.GetNoteModelByPreviewModel(preview.Notes, preview.Id).ToList();
            _ = InvokeAsync(StateHasChanged);
        }
        private async Task LoadDataAsync()
        {
            _property = StateService.Properties.FirstOrDefault(x => x.Id == PropertyId);

            if (_property == null || !_property.HasNotes)
            {
                return;

            }
            var propertyPreview = StateService.GetPropertyPreview(_property.Id);
            if (propertyPreview == null)
            {
                propertyPreview = await PropertyService.GetPreviewByIdAsync(_property.Id);
                if (propertyPreview == null)
                {
                    _isLoading = false;
                    return;
                }
                StateService.AddPropertyNotes(propertyPreview);
            }

            _userNotes = AggregatorService.GetNoteModelByPreviewModel(propertyPreview.Notes, propertyPreview.Id).ToList();
        }
        private void OnNoteCreateClick()
        {
            _notesSection?.OpenCreateNoteModal();
        }

        public void Dispose()
        {
            StateService.OnPropertyNoteAdded -= OnPropertyNoteAddedHandler;
        }
    }
}
