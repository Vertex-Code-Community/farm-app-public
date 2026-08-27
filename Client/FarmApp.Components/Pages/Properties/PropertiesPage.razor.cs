using FarmApp.Services.Providers;
using FarmApp.Services.Services;
using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Constants;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Properties;
using FarmApp.ViewModels.PropertyNotes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace FarmApp.Components.Pages.Properties;

public partial class PropertiesPage : IDisposable
{
    [Inject] public required IStateService StateService { get; set; }
    [Inject] public required INavigationService NavigationService { get; set; }
    [Inject] public required IPropertyService PropertyService { get; set; }
    [Inject] public required INotificationService NotificationService { get; set; }
    [Inject] public required IGlobalLoaderService GlobalLoaderService { get; set; }
    [Inject] public required IPropertyNotesAggregatorService PropertyNotesAggregatorService { get; set; }
    [Inject] public required  AuthStateProvider AuthStateProvider { get; set; }
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }

    private readonly NumberFormatInfo _nF = new() { NumberDecimalSeparator = "." };
    private bool _showUpdateModal = false;
    private string _editedName = string.Empty;
    private PropertyViewModel? _editedModel;
    private bool _isLoading = true;
    private List<PropertyNoteModel> _userNotes = new();
    private PropertyViewModel? _propertyToBeRemoved = null;
    private bool _showRemovePropertyDialog = false;

    protected override async Task OnInitializedAsync()
    {
        StateService.OnPropertyAdded += OnPropertyAdded;
        
        if (StateService.PropertiesTask is null) return;
        
        await StateService.PropertiesTask;
        _userNotes = await PropertyNotesAggregatorService.LoadAllAsync();
        _isLoading = false;
        
        StateHasChanged();
    }

    private void OnPropertyAdded()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        StateService.OnPropertyAdded -= OnPropertyAdded;
    }

    private Task OnPropertyClickAsync(PropertyViewModel property)
    {
        NavigationService.NavigateTo(Constants.ClientRoutes.MainPage);

        if (property.Centroid is not null)
            IMapCallbackService.FlyTo((float)property.Centroid.X, (float)property.Centroid.Y, property.Zoom);
        
        return Task.CompletedTask;
    }

    private void OnEditClicked(PropertyViewModel model)
    {
        _editedModel = model;
        _editedName = model.Name;
        _showUpdateModal = true;
    }
    
    private async Task OnUpdateClickedAsync()
    {
        if (_editedModel is null) return;

        var editedName = _editedName;
        var editedModel = _editedModel;
        
        _editedName = string.Empty;
        _showUpdateModal = false;
        _editedModel = null;
        StateHasChanged();
        
        if (string.IsNullOrWhiteSpace(editedName) || editedModel is null)
        {
            NotificationService.Add("Помилка введення даних", "Імʼя не повинно бути пустим");
            return;
        }

        GlobalLoaderService.SwitchOn();
        
        var updatedModelTask = PropertyService.UpdateAsync(editedModel.Id, new UpdatePropertyModel
        {
            Name = editedName
        });
        
        var minimalTimeTask = Task.Delay(TimeSpan.FromMilliseconds(750));
        await Task.WhenAll(updatedModelTask, minimalTimeTask);
        
        GlobalLoaderService.SwitchOff();

        var updatedModel = updatedModelTask.Result;
        if (updatedModel is null) return;

        editedModel.Name = updatedModel.Name;
        StateHasChanged();
        
        NotificationService.Add("Результат", "Назву змінено");
    }

    private void OnRemoveClicked(PropertyViewModel model)
    {
        _propertyToBeRemoved = model;
        _showRemovePropertyDialog = true;
    }
    private void OnCalendarClicked()
    {
        NavigationService.NavigateTo(Constants.ClientRoutes.PropertyCalendarPage);
    }
    private async Task OnRemovePropertyAsync()
    {
        if (_propertyToBeRemoved is null) return;

        var propertyId = _propertyToBeRemoved.Id;

        _propertyToBeRemoved = null;
        _showRemovePropertyDialog = false;
        StateHasChanged();
        
        using var loader = GlobalLoaderService.SwitchOn();
        var status = await PropertyService.DeleteAsync(propertyId);
        if (!status) return;

        StateService.DeleteProperty(propertyId);
        IMapPropertyService.InvokeRemoveProperty(propertyId);
        
        StateHasChanged();
    }
}