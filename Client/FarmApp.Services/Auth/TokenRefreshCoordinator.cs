using FarmApp.ViewModels.Accounts;

namespace FarmApp.Services.Auth
{
    public class TokenRefreshCoordinator
    {
        private readonly SemaphoreSlim _lock = new(1, 1);
        private Task<TokenModel?>? _refreshTask;


        public async Task<TokenModel?> RefreshAsync(
            Func<CancellationToken, Task<TokenModel?>> refreshFunc, CancellationToken cancellationToken = default)
        {
            if (_refreshTask != null)
            {
                return await _refreshTask;
            }

            await _lock.WaitAsync(cancellationToken);

            try
            {
                _refreshTask ??= refreshFunc(cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
            try
            {
                return await _refreshTask;
            }
            finally
            {
                _refreshTask = null;
            }
        }
    }
}
