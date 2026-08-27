using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FarmApp.Components.Components.Input;

public partial class InputComponent
{
    [Parameter] public string Type { get; set; } = "text";
    [Parameter] public string Label { get; set; } = "Label";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public string IconUrl { get; set; }

    private readonly string _id = $"id_{Guid.NewGuid()}";

    protected override bool TryParseValueFromString(string? value, out string result, out string? validationErrorMessage)
    {
        result = value ?? "";
        validationErrorMessage = null;
        return true;
    }

    private void HandleInput(ChangeEventArgs e)
    {
        var newValue = e.Value?.ToString() ?? string.Empty;
        CurrentValueAsString = newValue;
        if (EditContext != null)
        {
            EditContext.NotifyFieldChanged(FieldIdentifier);
        }
    }
}