using FarmApp.Services.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.Header;

public partial class HeaderComponent
{
    [Parameter] public string Title { get; set; }

    [Parameter] public string ImgUrl { get; set; }

    [Parameter] public string TitleImgUrl { get; set; }

    [Parameter] public EventCallback IconCallback { get; set; }
}
