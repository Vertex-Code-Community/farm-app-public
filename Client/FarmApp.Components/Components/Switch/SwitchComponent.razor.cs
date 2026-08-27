using Microsoft.AspNetCore.Components;

namespace FarmApp.Components.Components.Switch
{
    public partial class SwitchComponent
    {
        [Parameter] public bool IsEnabled { get; set; }
        [Parameter] public EventCallback<bool> IsEnabledChanged { get; set; }

        private Task OnInputChanged(ChangeEventArgs e)
        {
            var value = e.Value switch
            {
                bool b => b,
                string s when bool.TryParse(s, out var parsed) => parsed,
                string s when s.Equals("on", StringComparison.OrdinalIgnoreCase) => true,
                _ => false
            };
            return IsEnabledChanged.InvokeAsync(value);
        }
    }
}
