using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmApp.Components.Services.Interfaces
{
    public interface IBackButtonService
    {
        event Func<Task<bool>>? OnBackButtonPressed;
        Task<bool> RaiseBackButtonPressed();
        bool HasSubscribers { get; }
    }

}
