namespace FarmApp.Shared.Helpers;

public class VariableChangeTracker<T> : IDisposable where T : struct
{
    public event Action? OnChange;
    public event Func<T>? OnGetValue;
    
    private bool _disposed = false;
    private bool _running = false;
    private int _counter = 0;

    private readonly int _counterLimit;
    private readonly int _delayMs;
    
    public VariableChangeTracker(int counterLimit, int delayMs)
    {
        _counterLimit = counterLimit;
        _delayMs = delayMs;
    }
    
    public async Task TrackAsync()
    {
        if (_disposed) return;

        _counter = 0;
        
        if (_running)
        {
            OnChange?.Invoke();
            return;
        }

        _running = true;
        
        while (_running)
        {
            if (_disposed)
            {
                _running = false;
                _counter = 0;
            }
            
            var prevValue = OnGetValue?.Invoke();
            if (_counter == 0) OnChange?.Invoke();
            
            await Task.Delay(_delayMs);
            if (_disposed)
            {
                _running = false;
                _counter = 0;
            }
            
            var value = OnGetValue?.Invoke();

            if (value.Equals(prevValue)) _counter++;
            else _counter = 0;

            if (_counter < _counterLimit) continue;
            
            _running = false;
            _counter = 0;
        }
    }
    
    public void Dispose()
    {
        _disposed = true;
    }
}