
namespace FarmApp.ViewModels.Accounts
{
    public class AuthResult
    {
        public bool IsSuccess { get; init; }
        public string? ErrorMessage { get; init; }

        public static AuthResult Success() => new() { IsSuccess = true };
        public static AuthResult Fail(string message) => new() { IsSuccess = false, ErrorMessage = message };
    }

    public class AuthResult<T>
    {
        public bool IsSuccess { get; init; }
        public string? ErrorMessage { get; init; }
        public T? Data { get; init; }

        public static AuthResult<T> Success(T data) =>
            new()
            {
                IsSuccess = true,
                Data = data
            };

        public static AuthResult<T> Fail(string message) =>
            new()
            {
                IsSuccess = false,
                ErrorMessage = message
            };
    }
}
