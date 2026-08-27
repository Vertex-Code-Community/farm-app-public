namespace FarmApp.AdminComponents.Services.Interfaces;

public interface IHeaderControlsService
{
    bool ShowDate { get; set; }

    event Action? OnChanged;
}


