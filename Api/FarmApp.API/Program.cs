using FarmApp.Api.Middlewares;
using FarmApp.BusinessLogicLayer.Services;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.ViewModels.Options;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Constants = FarmApp.Shared.Constants.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services =  builder.Services;
var configuration =  builder.Configuration;

FarmApp.BusinessLogicLayer.StartupExtension.BusinessLogicInitializer(services, configuration);

services.Configure<JwtConnectionOptions>(configuration.GetSection(Constants.AppSettings.JWT_CONFIGURATION));

services.AddSingleton<IConfiguration>(configuration);

services.AddControllers();
services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "FarmAppApi", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            []
        }
    });
});

services.AddMemoryCache();

services.AddHttpClient<IWeatherHttpClient, WeatherHttpClient
    >(client =>
{
    client.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");
    client.Timeout = TimeSpan.FromSeconds(10);
}).SetHandlerLifetime(TimeSpan.FromMinutes(5));


services.AddCors();

// Trust X-Forwarded-Proto/Host from reverse proxies so the API emits
// absolute URLs with https + correct host.
services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FarmApp API V1");
});

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(corsBuilder => corsBuilder.AllowAnyOrigin()
                             .AllowAnyHeader()
                             .AllowAnyMethod());

app.UseHttpsRedirection();



app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
