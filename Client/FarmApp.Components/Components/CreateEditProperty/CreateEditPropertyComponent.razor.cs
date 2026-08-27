using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.CreateEditProperty;

public partial class CreateEditPropertyComponent
{
    [Parameter] public EventCallback<bool> ShowChanged { get; set; }
    [Parameter] public bool Show
    {
        get => _show;
        set
        {
            if (_show == value) return;
            _show = value;

            ShowChanged.InvokeAsync(value);
        }
    }

    [Parameter] public EventCallback<string> NameChanged { get; set; }
    [Parameter] public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;

            NameChanged.InvokeAsync(value);
        }
    }

    [Parameter] public EventCallback<string> AreaTextChanged { get; set; }
    [Parameter] public string AreaText
    {
        get => _areaText;
        set
        {
            if (_areaText == value) return;
            _areaText = value;

            AreaTextChanged.InvokeAsync(value);
        }
    }

    [Parameter] public EventCallback OnClicked { get; set; }
    [Parameter] public bool EditMode { get; set; }

    private bool _show = false;
    private string _name = string.Empty;
    private string _areaText = string.Empty;

    private void HandleSheetOpenChanged(bool open)
    {
        Show = open;
    }

    private void CloseSheetAsync()
    {
        Show = false;
    }

    private Task ConfirmAsync() => OnClicked.InvokeAsync();
}