using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.PropertyNoteStatuses;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Components.PropertyNoteStatusCreate
{
    public partial class PropertyNoteStatusCreateModal
    {
        [Inject] public required IGlobalLoaderService GlobalLoaderService { get; set; }
        [Inject] public required IStateService StateService { get; set; }
        [Inject] public required IPropertyNoteStatusService PropertyNoteStatusService { get; set; }
        [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
        [Parameter] public int StatusId { get; set; } = -1;

        [Parameter] public EventCallback CloseModalCallback { get; set; }

        private bool _sheetOpen = true;
        private bool _showDeleteModal = false;

        private PropertyNoteStatusPreviewModel _status = new();
        private bool _isLoading = true;
        private EditContext? _editContext;

        private PropertyNoteStatusModel _editingStatus = new();

        private StatusColor _selectedColor = new();
        private readonly StatusColor[] AllowedColors =
        {
            new StatusColor { TextHexColor = "#0364C5", BGHexColor = "#D5EAFF" }, // blue
            new StatusColor { TextHexColor = "#E35002", BGHexColor = "#FFEEDB" }, // orange
            new StatusColor { TextHexColor = "#5D21CE", BGHexColor = "#EEE7FF" }, // purple
            new StatusColor { TextHexColor = "#585C70", BGHexColor = "#E5E8F6" }, // gray
            new StatusColor { TextHexColor = "#925C00", BGHexColor = "#FDF2CA" }, // brown
            new StatusColor { TextHexColor = "#1D8B41", BGHexColor = "#DCFAE9" }, // green
            new StatusColor { TextHexColor = "#C42921", BGHexColor = "#FFDED8" }, // red
        };

        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;
            _editContext = new EditContext(_status);

            if (StatusId != -1)
            {
                var existing = StateService.PropertyNoteStatuses.FirstOrDefault(x => x.Id == StatusId);
                if (existing is not null) _editingStatus = existing;

                if (_editingStatus != null)
                {
                    _status.Name = _editingStatus.Name;

                    var matchingColor = AllowedColors.FirstOrDefault(c =>
                        c.TextHexColor.Equals(_editingStatus.TextColorHex, StringComparison.OrdinalIgnoreCase));

                    if (matchingColor != null)
                    {
                        _selectedColor = matchingColor;
                    }
                    else
                    {
                        _selectedColor = new StatusColor
                        {
                            TextHexColor = _editingStatus.TextColorHex,
                            BGHexColor = _editingStatus.BGColorHex
                        };
                    }
                    _status.TextColorHex = _selectedColor.TextHexColor;
                    _status.BGColorHex = _selectedColor.BGHexColor;
                }
            }
            else
            {
                _selectedColor = AllowedColors[0];
                _status.TextColorHex = _selectedColor.TextHexColor;
                _status.BGColorHex = _selectedColor.BGHexColor;
            }

            await Task.Delay(600);
            _isLoading = false;
        }

        private async Task OnSheetOpenChanged(bool open)
        {
            _sheetOpen = open;
            if (!open)
                await CloseModalCallback.InvokeAsync();
        }

        private async Task OnSubmit()
        {
            if (_editContext is null)
                return;

            if (!_editContext.Validate())
            {
                _editContext.NotifyValidationStateChanged();
                return;
            }
            using var loader = GlobalLoaderService.SwitchOn();

            var success = StatusId == -1
                ? await TryCreatePropertyNoteStatusAsync()
                : await TryUpdatePropertyNoteStatusAsync();

            if (success)
                await CloseModalCallback.InvokeAsync();
        }

        private async Task<bool> TryCreatePropertyNoteStatusAsync()
        {
            var statusModel = new CreatePropertyNoteStatusModel()
            {
                Name = _status.Name,
                TextColorHex = _selectedColor.TextHexColor,
                BGColorHex = _selectedColor.BGHexColor
            };
            var propertyNoteStatus = await PropertyNoteStatusService.CreateStatus(statusModel);
            if (propertyNoteStatus is null)
                return false;

            StateService.PropertyNoteStatuses.Add(propertyNoteStatus);
            return true;
        }

        private async Task<bool> TryUpdatePropertyNoteStatusAsync()
        {
            var statusModel = new UpdatePropertyNoteStatusModel()
            {
                Name = _status.Name,
                TextColorHex = _selectedColor.TextHexColor,
                BGColorHex = _selectedColor.BGHexColor
            };
            var propertyNoteStatus = await PropertyNoteStatusService.UpdateStatusAsync(StatusId, statusModel);

            if (propertyNoteStatus is null)
                return false;

            var index = StateService.PropertyNoteStatuses.FindIndex(x => x.Id == StatusId);
            if (index >= 0)
                StateService.PropertyNoteStatuses[index] = propertyNoteStatus;

            return true;
        }
        private async Task DeleteStatusAsync()
        {
            _showDeleteModal = false;
            if (StatusId == -1)
                return;

            GlobalLoaderService.SwitchOn();
            var result = await PropertyNoteStatusService.DeleteAsync(StatusId);
            GlobalLoaderService.SwitchOff();

            if (result != true)
                return;

            StateService.DeletePropertyNoteStatus(StatusId);
            await CloseModalCallback.InvokeAsync();
            
        }

        private void OnColorSelected(StatusColor color)
        {
            _selectedColor = color;
            
            _status.TextColorHex = color.TextHexColor;
            _status.BGColorHex = color.BGHexColor;
        }

        private sealed class StatusColor
        {
            public string TextHexColor { get; set; } = string.Empty;
            public string BGHexColor { get; set; } = string.Empty;
        } 
    }
}
