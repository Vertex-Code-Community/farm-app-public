using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;


namespace FarmApp.Components.Components.ErrorMessage
{
    public partial class ErrorMessage<T>
    {
        [CascadingParameter] private EditContext? CascadedEditContext { get; set; }

        [Parameter]
        public string? Message { get; set; } = "Something went wrong";

        [Parameter] public Expression<Func<T>>? For { get; set; }

        private bool ShouldShow => (For != null
        ? CascadedEditContext?.GetValidationMessages(FieldIdentifier.Create(For)).Any(msg => !string.IsNullOrWhiteSpace(msg)) ?? false
        : !string.IsNullOrWhiteSpace(Message));
    }
}