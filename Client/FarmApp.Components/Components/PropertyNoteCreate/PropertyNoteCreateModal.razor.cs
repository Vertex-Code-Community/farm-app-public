using FarmApp.Components.Components.ConfirmationDialog;
using FarmApp.Components.Helpers;
using FarmApp.Components.Services.Interfaces;
using FarmApp.Components.Validators.PropertyNote;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Media;
using FarmApp.ViewModels.PropertyNotes;
using FarmApp.ViewModels.PropertyNoteStatuses;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
namespace FarmApp.Components.Components.PropertyNoteCreate;

public partial class PropertyNoteCreateModal 
{
    [Inject] public required IPropertyNoteService PropertyNoteService { get; set; }
    [Inject] public required IMediaService MediaService { get; set; }
    [Inject] public required IDialogService DialogService { get; set; }
    [Inject] public required IStateService StateService { get; set; }
    [Inject] public required IGlobalLoaderService GlobalLoaderService { get; set; }
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }
    [Parameter] public string PropertyNoteId { get; set; } = string.Empty;
    [Parameter] public bool isNested { get; set; } = false;
    [Parameter] public string PropertyId { get; set; } = string.Empty;
    [Parameter] public DateTime Date { get; set; } = DateTime.Now;
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public List<PickedMediaFile> Files { get; set; } = new();
    [Parameter] public EventCallback<List<PickedMediaFile>> FilesChanged { get; set; }
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    [Parameter] public EventCallback<PropertyNoteModel> OnPropertyNoteUpdated { get; set; }
    [Parameter] public TimeOptionModel? RemindAtStart { get; set; }
    [Parameter] public TimeOptionModel? RemindAtEnd { get; set; }
    [Parameter] public EventCallback<TimeOptionModel?> RemindAtStartChanged { get; set; }
    [Parameter] public EventCallback<TimeOptionModel?> RemindAtEndChanged { get; set; }

    [Inject] public IGuideTourService GuideTourService { get; set; } = default!;

    private PropertyNoteStatusModel _noneStatus = new PropertyNoteStatusModel
    {
        Id = -1,
        Name = "None",
        Code = "NONE",
        BGColorHex = "#E5E8F6",
        TextColorHex = "#585C70",
        IsDefault = true
    };

    private bool CreateMode => string.IsNullOrWhiteSpace(PropertyNoteId);
    private string _modalTitle => !_editMode ? Localizer["Property-Note_New-Note"] : Localizer["Property-Note_Edit-Note"]; 
    private bool _editMode => !CreateMode;
    private bool _isLoading = true;

    private PropertyNotePreviewModel _propertyNote = new();

    private List<PropertyNoteStatusModel> _statuses = new();
    private PropertyNoteStatusModel? _selectedStatus;

    private EditContext? _editContext;
    private ValidationMessageStore? _messageStore;

    protected override async Task OnInitializedAsync()
    {
        _selectedStatus = _noneStatus;

        _propertyNote.StartTime = Date;
        _propertyNote.EndTime = Date.AddDays(1);

        _isLoading = true;
        try
        {
            await Task.WhenAll(InitializePage(), Task.Delay(600));
        }
        finally
        {
            _isLoading = false;
        }
    }
    protected override void OnParametersSet()
    {
        _propertyNote.StartTime = Date;
        _propertyNote.EndTime = Date.AddDays(1);
    }
    private async Task InitializePage()
    {
        _statuses = StateService.PropertyNoteStatuses.ToList();
        _statuses.Insert(0, _noneStatus);

        if (!CreateMode)
        {
            var propertyPreviewModel = StateService.GetPropertyPreview(PropertyId);
            var propertyNote = propertyPreviewModel?.Notes
                .FirstOrDefault(x => x.Id == PropertyNoteId);

        if (propertyNote is null)
        {
            await IsOpenChanged.InvokeAsync(false);
            return;
        }

            _propertyNote = new PropertyNotePreviewModel
            {
                CreatedAt = propertyNote.CreatedAt,
                PreviewMediaId = propertyNote.PreviewMediaId,
                EndTime = propertyNote.EndTime,
                Header = propertyNote.Header,
                Id = propertyNote.Id,
                StartTime = propertyNote.StartTime,
                StatusId = propertyNote.StatusId,
                Text = propertyNote.Text,
                NotificationsEnabled = propertyNote.NotificationsEnabled
            };
            _selectedStatus = _statuses.FirstOrDefault(x => x.Id == propertyNote.StatusId);

            var uploadedMedias = await MediaService.GetMediaByNoteIdAsync(PropertyNoteId);

        }

        _editContext = new EditContext(_propertyNote);
        _messageStore = new ValidationMessageStore(_editContext);
    }

    private async Task SubmitAsync()
    {
        if (!PropertyNoteValidator.Validate(_editContext!,
            _messageStore!, _propertyNote, this, PropertyId))
            return;

        using var lodaer = GlobalLoaderService.SwitchOn();

        if (CreateMode)
            await CreateAsync();
        else
            await UpdateAsync();

        await IsOpenChanged.InvokeAsync(false);
    }
    private async Task CreateAsync()
    {
        
        var createPropertyNote = new CreatePropertyNoteModel
        {
            PropertyId = PropertyId,
            StartTime = _propertyNote.StartTime,
            EndTime = _propertyNote.EndTime,
            CreatedAt = DateTime.Now,
            Header = _propertyNote.Header,
            Text = _propertyNote.Text,
            StatusId = _selectedStatus?.Id is > 0 ? _selectedStatus.Id : null,
            NotificationsEnabled = _propertyNote.NotificationsEnabled,
            NotifyBeforeStart = _propertyNote.NotifyBeforeStart,
            NotifyBeforeEnd = _propertyNote.NotifyBeforeEnd
        };
        createPropertyNote.TempUploadResults = Files
            .Where(x => x.UploadState == UploadState.Uploaded)
            .Select(x => x.TempUploadResult!).ToList();

        var createdPropertyNote = await PropertyNoteService.CreateAsync(createPropertyNote);
        if (createdPropertyNote is null) return;

        StateService.AddPropertyNote(createdPropertyNote);

        if (GuideTourService.ActiveGroup == TourGroup.Onboarding && GuideTourService.ActiveStep?.Sequence == 5)
        {
            GuideTourService.CompleteCurrentStep();
        }

        IMapPropertyService.InvokeDrawProperties(StateService.Properties);


    }

    private async Task<bool?> CanCloseModal()
    {
        if (Files.Count == 0 &&
            string.IsNullOrEmpty(_propertyNote.Header) &&
            string.IsNullOrEmpty(_propertyNote.Text) &&
            _selectedStatus?.Id == -1 || _editMode)
        {
            return true;
        }
        var result = await DialogService.RequestAsync<bool?, ConfirmationComponent>(new FarmApp.Services.Models.DialogModels.DialogParametersModel
        {
            Payload = new Dictionary<string, string>
            {
                ["header"] = "Unsaved_Data",
                ["body"] = "Property-Note_Unsaved_Data"
            }
            
        });

        return result == true;

    }

    private void OnPreviewNoteSet(string? mediaId)
    {
        var propertyPreviewModel = StateService.GetPropertyPreview(PropertyId);
        var propertyNote = propertyPreviewModel?.Notes
            .FirstOrDefault(x => x.Id == PropertyNoteId);

        propertyNote!.PreviewMediaId = mediaId;

    }
    private async Task UpdateAsync()
    {

        var updatedPropertyNote = await PropertyNoteService.UpdateAsync(_propertyNote.Id, new UpdatePropertyNoteModel
        {
            Header = _propertyNote.Header,
            Text = _propertyNote.Text,
            StatusId = _selectedStatus?.Id is > 0 ? _selectedStatus.Id : null,
            TempUploadResults = Files.Where(f => f.UploadState == UploadState.Uploaded && !f.IsRemote)
                                      .Select(f => f.TempUploadResult!).ToList(),
            StartTime = _propertyNote.StartTime,
            EndTime = _propertyNote.EndTime,
            NotificationsEnabled = _propertyNote.NotificationsEnabled,
            NotifyBeforeStart = _propertyNote.NotifyBeforeStart == Shared.Enums.NotificationOffset.None || !_propertyNote.NotificationsEnabled ?
            null : _propertyNote.NotifyBeforeStart,
            NotifyBeforeEnd = _propertyNote.NotifyBeforeEnd == Shared.Enums.NotificationOffset.None || !_propertyNote.NotificationsEnabled ?
            null : _propertyNote.NotifyBeforeEnd,
        });

        if (updatedPropertyNote is null) return;

        await OnPropertyNoteUpdated.InvokeAsync(updatedPropertyNote);
        StateService.UpdatePropertyNote(updatedPropertyNote);

    }
   
    private async Task OnFilesPicked(List<PickedMediaFile> files)
    {
        Files = Files.Concat(files).ToList();
        await FilesChanged.InvokeAsync(Files);
    }

    private async Task OnRemindAtStartSelected(TimeOptionModel beforeStart)
    {
        if (_propertyNote.NotificationsEnabled)
            _propertyNote.NotifyBeforeStart = beforeStart.Duration;
        RemindAtStart = beforeStart;
        await RemindAtStartChanged.InvokeAsync(RemindAtStart);
        StateHasChanged();
    }

    private async Task OnRemindAtEndSelected(TimeOptionModel beforeEnd)
    {
        if (_propertyNote.NotificationsEnabled)
            _propertyNote.NotifyBeforeEnd = beforeEnd.Duration;
        RemindAtEnd = beforeEnd;
        await RemindAtEndChanged.InvokeAsync(RemindAtEnd);
        StateHasChanged();
    }

    private void OnStatusSelected(PropertyNoteStatusModel status)
    {
        _selectedStatus = status;
        StateHasChanged();
    }
    private bool _isFilePickerOpen = false;

    private void OpenFilePicker()
    {
        _isFilePickerOpen = true;
    }

}
