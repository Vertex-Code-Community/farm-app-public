namespace FarmApp.Services.Services.Interfaces;

public interface IFirstTimeSetupService
{
    bool IsCompleted { get; }
    void MarkCompleted();
}
