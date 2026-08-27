using Microsoft.AspNetCore.Components;

namespace FarmApp.Services.Models.DialogModels
{
    public class DialogModel
    {
        public required DialogParametersModel Parameters { get; set; }
        public required RenderFragment Fragment { get; set; }
    }
}
