using FarmApp.Components.Components.Dialogs.Base;
using FarmApp.Services.Models.DialogModels;

namespace FarmApp.Components.Services.Interfaces
{
    public interface IDialogService
    {
        public event Action? OnUpdate;

        IReadOnlyList<DialogModel> Dialogs { get; }
        Task RequestAsync<TDialogComponent>(DialogParametersModel parameters)
            where TDialogComponent : IBaseDialogComponent;

        Task<TResult?> RequestAsync<TResult, TDialogComponent>(DialogParametersModel parameters)
            where TDialogComponent : IBaseDialogComponent<TResult>;
    }
}
