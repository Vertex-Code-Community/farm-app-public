using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.Dialogs.Base
{
    public interface IBaseDialogComponent<TResult> : IComponent
    {
        public EventCallback OnClose { get; set; }
        public EventCallback<TResult?> OnSubmit { get; set; }
        public object? Payload { get; set; }
    }

    public interface IBaseDialogComponent : IComponent
    {
        public EventCallback OnClose { get; set; }
        public EventCallback OnSubmit { get; set; }
        public object? Payload { get; set; }
    }
}
