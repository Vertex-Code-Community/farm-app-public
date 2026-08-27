using FarmApp.Services.Services.Interfaces;
using FarmApp.Shared.Resources.Localization;
using FarmApp.ViewModels.Users;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace FarmApp.Components.Pages.EditProfile;

public partial class EditProfilePage
{
    [Inject] public required IUserService UserService { get; set; }
    [Inject] public required INavigationService NavigationService { get; set; }
    [Inject] public required INotificationService NotificationService { get; set; }
    [Inject] public required IGlobalLoaderService GlobalLoaderService { get; set; }
    [Inject] public required IStringLocalizer<AppRecources> Localizer { get; set; }

    private bool _isLoading = true;
    private string _imgUrl = "_content/FarmApp.Components/img/profile/andriy.png";
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;
    private string _userId = string.Empty;

    private const int MaxLenght = 50;
    private const int MinLenght = 3;
    
    private bool _showDelteModal;

    protected override async Task OnInitializedAsync()
    {
        var user = await UserService.GetCurrentUserAsync();
        if (user is null)
        {
            NotificationService.Add("Помилка", "Не вдалося завантажити профіль");
            _isLoading = false;
            return;
        }

        _userId = user.Id;
        _firstName = user.FirstName ?? string.Empty;
        _lastName = user.LastName ?? string.Empty;
        _email = user.Email ?? string.Empty;
        _isLoading = false;
    }

    private async Task SaveChanges()
    {
        var firstName = _firstName.Trim();
        var lastName = _lastName.Trim();

        if (firstName.Length < MinLenght && firstName.Length  > MaxLenght 
            || lastName.Length  < MinLenght && lastName.Length  > MaxLenght)
        {
            NotificationService.Add("Помилка введення даних", "Імʼя та прізвище — від 3 до 50 символів");
            return;
        }

        using var loader = GlobalLoaderService.SwitchOn();

        var model = new UpdateUserModel
        {
            Id = _userId,
            Email = _email,
            FirstName = firstName,
            LastName = lastName
        };

        var ok = await UserService.UpdateUserAsync(model);
        if (!ok)
            return;

        NotificationService.Add("Результат", "Дані збережено");
        NavigationService.Back();
    }
}
