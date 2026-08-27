using Bch.Components.Table;
using FarmApp.AdminComponents.Services.Interfaces;
using FarmApp.ViewModels.Users;
using Microsoft.AspNetCore.Components;

namespace FarmApp.AdminComponents.Pages.Users;

public partial class UsersPage : ComponentBase
{
    [Inject] public required IGlobalLoaderService GlobalLoaderService { get; set; }
    [Inject] public required IUsersApiService UsersApiService { get; set; }

    private UserViewModel _editUserViewModel = new();
    private BchTable<UserViewModel>? _usersTable;
    private List<UserViewModel>? _models = new();
    
    private int _currentPage = 1;
    private readonly int _countTake = 10;

    private bool _confirmationModalOpen;
    private bool _editModalOpen;    
    private string _userIdForDelete = string.Empty;
    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _models = await UsersApiService.GetUsersModelAsync((_currentPage - 1) * _countTake, _countTake);
        
        StateHasChanged();
        _usersTable?.ReloadTable();
    }
    
    private async Task OpenConfirmModalForDelete(string id)
    {
        _confirmationModalOpen = true;
        _userIdForDelete = id;
        StateHasChanged();
    }

    private void OpenEditModal(UserViewModel userViewModel)
    {
        _editUserViewModel = userViewModel;
        _editModalOpen = true;
        StateHasChanged();
    }

    private async Task OnUpdateUser(UpdateUserModel model)
    {
        using var _ = GlobalLoaderService.SwitchOn();
        
        await UsersApiService.UpdateUserAsync(model);
        _editUserViewModel = null;
        _editModalOpen = false;

        await LoadAsync();
        StateHasChanged();
    }

    private void CloseEditModal()
    {
        _editModalOpen = false;
        StateHasChanged();
    } 
    
    private async Task RemoveUserAsync(bool confirmed)
    {
        if (!confirmed) return;
        
        using var _ = GlobalLoaderService.SwitchOnContent(); 
        await UsersApiService.DeleteUserModelAsync(_userIdForDelete);
        await LoadAsync();
        
        StateHasChanged();
    }
}