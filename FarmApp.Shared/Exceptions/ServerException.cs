using System.Net;

namespace FarmApp.Shared.Exceptions;

public class ServerException : Exception
{
    public HttpStatusCode Code { get; set; }
    public List<string> ErrorMessages { get; set; } = new();

    public override string Message => ErrorMessages.Count == 0 ? "Error" : ErrorMessages.Aggregate((x, y) => $"{x}\n\n{y}");
    
    public ServerException() {}
    
    public ServerException(string error, HttpStatusCode errorCode = HttpStatusCode.InternalServerError)
    {
        Code = errorCode;
        ErrorMessages = new List<string>();
        ErrorMessages.Add(error);
    }

    public ServerException(List<string> errorMessages, HttpStatusCode errorCode = HttpStatusCode.InternalServerError)
    {
        ErrorMessages = new List<string>(errorMessages);
        Code = errorCode;
    }
}
