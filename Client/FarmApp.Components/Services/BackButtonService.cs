using FarmApp.Components.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmApp.Components.Services
{
    public class BackButtonService : IBackButtonService
    {
        public event Func<Task<bool>>? OnBackButtonPressed;
        public bool HasSubscribers => OnBackButtonPressed != null && OnBackButtonPressed.GetInvocationList().Length > 0;

        public async Task<bool> RaiseBackButtonPressed()
        {
            var handler = OnBackButtonPressed;
            if (handler == null) return false;
            var handlers = handler.GetInvocationList().Cast<Func<Task<bool>>>().Reverse();
            foreach (var h in handlers)
            {
                if (await h()) return true;
            }
            return false;
        }
    }
}
