using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FarmApp.Components.Components.PasswordInput;

// Inheriting from InputBase gives you 'CurrentValueAsString' and 'ValueExpression' automatically
public partial class PasswordInputComponent 
{
    [Parameter] public string Label { get; set; } = "Password";

    private bool showPassword = false;

    protected override bool TryParseValueFromString(string? value, out string result, out string? validationErrorMessage)
    {
        result = value ?? string.Empty;
        validationErrorMessage = null;
        return true;
    }

    private Task OnInternalValueChanged(string value)
    {
        CurrentValueAsString = value;
        return Task.CompletedTask;
    }
}