using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FarmApp.Components.Components.Checkbox;

public partial class CheckboxComponent
{
    [Parameter] public bool Disabled { get; set; } = false;

    private async Task OnChange(ChangeEventArgs e)
    {
        CurrentValue = (bool)(e.Value ?? false);

        EditContext?.NotifyFieldChanged(FieldIdentifier);

        await Task.CompletedTask;
    }

    protected override bool TryParseValueFromString(string? value, out bool result, out string? validationErrorMessage)
    {
        throw new NotSupportedException("Checkbox does not support string parsing.");
    }
}