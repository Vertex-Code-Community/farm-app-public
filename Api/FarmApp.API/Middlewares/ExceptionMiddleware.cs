using System.Net;
using System.Text;
using FarmApp.Shared.Exceptions;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;

namespace FarmApp.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public ExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<ExceptionMiddleware>();
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);

            var body = await new StreamReader(context.Request.Body, Encoding.UTF8).ReadToEndAsync();
            var url = context.Request.GetDisplayUrl();
        }
        catch (ServerException ex)
        {
            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine("__________________ EXCEPTION __________________");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("__________________ EXCEPTION __________________");
            
            var result = JsonConvert.SerializeObject(new ServerException("Server error occurred, try later please"));
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync(result);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, ServerException exception)
    {
        var result = JsonConvert.SerializeObject(exception);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)exception.Code;
        await context.Response.WriteAsync(result);
    }
}