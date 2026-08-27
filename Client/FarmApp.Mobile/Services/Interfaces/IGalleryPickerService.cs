namespace FarmApp.Mobile.Services.Interfaces;

public interface IGalleryPickerService
{
    Task<IReadOnlyCollection<FileResult>> PickAsync();
}