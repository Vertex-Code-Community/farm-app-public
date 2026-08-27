using Newtonsoft.Json;

namespace FarmApp.Models;

public class ApiResponse<T> : ApiResponse where T : class
{
    [JsonProperty("data")]
    public T? Data { get; set; }
}

public class ApiResponse
{
    [JsonProperty("result")] public string? Result { get; set; }
    [JsonProperty("message")] public string? Message { get; set; }
}

public static class ApiResponses
{
    public static ApiResponse<T> Ok<T>(T data, string? message = null) where T : class =>
        new() { Result = "Ok", Message = message, Data = data };

    public static ApiResponse Ok(string? message = null) =>
        new() { Result = "Ok", Message = message };

    public static ApiResponse<T> Error<T>(string message) where T : class =>
        new() { Result = "Error", Message = message, Data = null };

    public static ApiResponse Error(string message) =>
        new() { Result = "Error", Message = message };
}
