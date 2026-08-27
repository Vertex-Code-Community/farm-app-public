using FarmApp.ViewModels.Notifications;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Pages.ViewNotification
{
    public partial class ViewNotificationPage
    {
        [Parameter] public required UserNotificationViewModel Notification { get; set; }
    }
}
