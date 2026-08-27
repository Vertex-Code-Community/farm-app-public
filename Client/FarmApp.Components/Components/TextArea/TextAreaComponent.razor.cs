using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.TextArea;

public partial class TextAreaComponent
{
    [Parameter] public string Label { get; set; } = "Label";
    [Parameter] public bool ReadOnly { get; set; }

    private readonly string _id = $"_id_{Guid.NewGuid()}";

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
    }
}