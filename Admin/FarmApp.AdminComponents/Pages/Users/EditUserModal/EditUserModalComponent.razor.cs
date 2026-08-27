using FarmApp.ViewModels.Users;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.Pages.Users.EditUserModal;

public partial class EditUserModalComponent : ComponentBase
{
    [Parameter]
    public required bool Opened
    {
        get => _opened;
        set
        {
            if (_opened == value) return;
            _opened = value;
            OpenedChanged.InvokeAsync(value);
        }
    }

    [Parameter] public required string Title { get; set; }
    [Parameter] public required bool Result { get; set; }
    [Parameter] public required string Width { get; set; } = "75%";
    [Parameter] public EventCallback<bool> ResultChanged { get; set; }
    [Parameter] public EventCallback<bool> OpenedChanged { get; set; }
    [Parameter] public EventCallback OnCloseClick { get; set; }
    
    [Parameter, EditorRequired] public required UserViewModel UserViewModel { get; set; }
    [Parameter, EditorRequired] public required EventCallback<UpdateUserModel> OnUpdateUser { get; set; }

    private UpdateUserModel _modelForUpdate = new();
    private bool _opened;

    protected override void OnInitialized()
    {
        _modelForUpdate.Email = UserViewModel.Email;
        _modelForUpdate.Password = string.Empty;
        _modelForUpdate.Id = UserViewModel.Id;
        _modelForUpdate.FirstName = UserViewModel.FirstName;
        _modelForUpdate.LastName = UserViewModel.LastName;
    }
    
    private void OnSave()
    { 
        OnUpdateUser.InvokeAsync(_modelForUpdate);
    }
}