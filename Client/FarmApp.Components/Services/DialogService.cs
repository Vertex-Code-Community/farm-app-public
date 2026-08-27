using FarmApp.Components.Components.Dialogs.Base;
using FarmApp.Components.Components.Dialogs.Global.Modal;
using FarmApp.Components.Services.Interfaces;
using FarmApp.Services.Models.DialogModels;
using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace FarmApp.Components.Services
{
    public class DialogService : IDialogService
    {
        public event Action? OnUpdate;
        public IReadOnlyList<DialogModel> Dialogs => _dialogs;

        private readonly List<DialogModel> _dialogs = new();

        public async Task<TResult?> RequestAsync<TResult, TDialogComponent>(DialogParametersModel parameters)
            where TDialogComponent : IBaseDialogComponent<TResult>
        {
            var result = await RequestCoreAsync(
                typeof(TDialogComponent),
                parameters,
                typeof(TResult));

            return (TResult?)result;
        }
        public async Task RequestAsync<TDialogComponent>(DialogParametersModel parameters)
             where TDialogComponent : IBaseDialogComponent
        {
            await RequestCoreAsync(
                typeof(TDialogComponent),
                parameters,
                null);
        }
        private async Task<object?> RequestCoreAsync(
            Type dialogType,
            DialogParametersModel parameters,
            Type? resultType)
        {
            var key = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<object?>();

            parameters.OnOverlayClicked =
                EventCallback.Factory.Create(this, () => tcs.TrySetResult(null));

            var dialogModel = new DialogModel
            {
                Parameters = parameters,
                Fragment = builder =>
                {
                    builder.OpenComponent<DialogModalComponent>(0);
                    builder.SetKey(key);

                    builder.AddAttribute(1, "Parameters", parameters);

                    builder.AddAttribute(2, "ChildContent", (RenderFragment)(childBuilder =>
                    {
                        childBuilder.OpenComponent(3, dialogType);

                        if (resultType != null)
                        {
                            var method = typeof(DialogService)
                                .GetMethod(nameof(CreateTypedCallback),
                                           BindingFlags.NonPublic | BindingFlags.Instance)!
                                .MakeGenericMethod(resultType);

                            var callback = method.Invoke(this, new object[] { tcs });

                            childBuilder.AddAttribute(4, "OnSubmit", callback);
                        }
                        else
                        {
                            childBuilder.AddAttribute(4, "OnSubmit",
                                EventCallback.Factory.Create(this, () =>
                                {
                                    tcs.TrySetResult(null);
                                }));
                        }

                        childBuilder.AddAttribute(5, "OnClose",
                            EventCallback.Factory.Create(this, () =>
                            {
                                tcs.TrySetResult(null);
                            }));

                        childBuilder.AddAttribute(6, "Payload", parameters.Payload);

                        childBuilder.CloseComponent();
                    }));

                    builder.CloseComponent();
                }
            };

            _dialogs.Add(dialogModel);
            OnUpdate?.Invoke();

            var result = await tcs.Task;

            _dialogs.Remove(dialogModel);
            OnUpdate?.Invoke();

            return result;
        }
        private EventCallback<T> CreateTypedCallback<T>(
            TaskCompletionSource<object?> tcs)
        {
            return EventCallback.Factory.Create<T>(this, (T payload) =>
            {
                tcs.TrySetResult(payload);
            });
        }
    }
}
