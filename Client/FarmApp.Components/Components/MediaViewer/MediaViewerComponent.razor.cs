using FarmApp.Components.Components.ConfirmationDialog;
using FarmApp.Components.Components.MediaChoiceDialog;
using FarmApp.Components.Services.Interfaces;
using FarmApp.Services.Services.Interfaces;
using FarmApp.ViewModels.Media;
using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.MediaViewer
{
    public partial class MediaViewerComponent
    {
        [Inject] public required IMediaService MediaService { get; set; }
        [Inject] public required IBackButtonService BackButtonService { get; set; }
        [Inject] public required IDialogService DialogService { get; set; }
        [Parameter] public string Title { get; set; } = String.Empty;
        [Parameter] public string Subtitle { get; set; } = String.Empty;

        [Parameter] public required bool IsOpen { get; set; }
        [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
        [Parameter] public EventCallback<PickedMediaFile> RemoveFile { get; set; }
        [Parameter] public required List<PickedMediaFile> MediaFiles { get; set; }

        [Parameter] public int StartingIndex { get; set; } = 0;

        private int _scrollIndex = 0;
        private bool _displayCounter = false;
        private List<int> videoIndexes = [];
        private CancellationTokenSource? _cts;

        private List<PickedMediaFile> _mediaToDisplay = new ();

        private int ScrollIndex
        {
            get => _scrollIndex;
            set
            {
                if (value < 0 || value >= _mediaToDisplay.Count)
                    return;

                if (_scrollIndex != value)
                {
                    _scrollIndex = value;
                    _ = OnScrollIndexChanged(value);
                }
            }
        }

        private Task OnScrollIndexChanged(int newIndex)
        {
            return ShowDisplay(1000);
        }

        protected override void OnInitialized()
        {
            BackButtonService.OnBackButtonPressed += HandleBackButtonClick;
        }

        protected override void OnParametersSet()
        {
            if (MediaFiles != null && MediaFiles.Count > 0)
            {
                _mediaToDisplay = MediaFiles.Where(f => f.UploadState != UploadState.Failed).ToList();
            }
            else
            {
                _mediaToDisplay = new();
            }
            NormalizeIndex();
        }
        private void NormalizeIndex()
        {
            if (_mediaToDisplay.Count == 0)
            {
                _scrollIndex = 0;
                return;
            }

            if (_scrollIndex >= _mediaToDisplay.Count)
            {
                _scrollIndex = _mediaToDisplay.Count - 1;
            }

            if (_scrollIndex < 0)
            {
                _scrollIndex = 0;
            }
        }
        private async Task CloseMediaViewer()
        {
            IsOpen = false;
            await IsOpenChanged.InvokeAsync(false);
        }

        private async Task<bool> HandleBackButtonClick()
        {
            if (!IsOpen) return false;
            await CloseMediaViewer();
            return true;
        }

        private async Task ShowDisplay(int timeInMs)
        {
            _cts?.Cancel();
            _cts?.Dispose();

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                _displayCounter = true;
                StateHasChanged();

                await Task.Delay(timeInMs, token);

                _displayCounter = false;
                StateHasChanged();
            }
            catch (OperationCanceledException)
            { }
        }
        public async Task DeleteMedia()
        {
            var result = await DialogService.RequestAsync<bool?, ConfirmationComponent>(new FarmApp.Services.Models.DialogModels.DialogParametersModel
            {
                Payload = new Dictionary<string, string>
                {
                    ["body"] = "Media_Remove"
                }
            });

            if (_mediaToDisplay.Count == 0 || ScrollIndex >= _mediaToDisplay.Count)
                return;

            var fileToDelete = _mediaToDisplay[ScrollIndex];

            var newIndex = ScrollIndex;

            if (newIndex >= _mediaToDisplay.Count - 1)
                newIndex--;

            await RemoveFile.InvokeAsync(fileToDelete);
        }

        private void SelectMedia(int index)
        {
            if (index < 0 || index >= _mediaToDisplay.Count)
                return;
            ScrollIndex = index;
            StateHasChanged();
        }

        public void Dispose()
        {
            BackButtonService.OnBackButtonPressed -= HandleBackButtonClick;

            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
