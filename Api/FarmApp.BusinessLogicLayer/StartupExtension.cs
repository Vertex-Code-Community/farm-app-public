using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using FarmApp.BusinessLogicLayer.Profiles;
using FarmApp.BusinessLogicLayer.Providers;
using FarmApp.BusinessLogicLayer.Services;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.BusinessLogicLayer.Services.NoLoyaltyUser;
using FarmApp.Entities.Entity;
using FarmApp.ViewModels.Options;
using Constants = FarmApp.Shared.Constants.Constants;
using FarmApp.BusinessLogicLayer.Workers;
using FarmApp.Models.Options;
using PushSharp.Net;

namespace FarmApp.BusinessLogicLayer;

public static class StartupExtension
{
    public static void BusinessLogicInitializer(this IServiceCollection services, IConfiguration configuration)
    { 
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISteadService, SteadService>();
        services.AddScoped<ICustomSteadService, CustomSteadService>();
        services.AddScoped<IPropertyNoteService, PropertyNoteService>();
        services.AddScoped<IPropertyNoteStatusService, PropertyNoteStatusService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationHistoryService, NotificationHistoryService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INoLoyaltyUserService, NoLoyaltyUserService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<ISignedUrlService, SignedUrlService>();
        services.AddScoped<IImageProcessor, ImageSharpProcessor>();
        services.AddScoped<IVideoThumbnailGenerator, FFmpegVideoThumbnailGenerator>();
        services.AddScoped<IVerificationService, VerificationService>();
        services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();


        services.AddSingleton<IWeatherService,WeatherService>();
        services.AddHostedService<TempFileCleanupWorker>();
        services.AddHostedService<WeatherBroadcastWorker>();
        services.AddHostedService<PushNotificationWorker>();

        #region Provider

        services.AddScoped<EmailProvider>();
        services.AddScoped<JwtProvider>();

        #endregion
        
        services.Configure<JwtConnectionOptions>(configuration.GetSection(Constants.AppSettings.JWT_CONFIGURATION));
        services.Configure<EmailOptions>(configuration.GetSection(Constants.AppSettings.EMAIL_CONFIGURATION));
        services.Configure<VerificationOptions>(configuration.GetSection(Constants.AppSettings.VERIFICATION_CONFIGURATION));
        services.Configure<UsersOptions>(configuration.GetSection(Constants.AppSettings.USERS_CONFIGURATION));
        services.Configure<WeatherOptions>(configuration.GetSection(Constants.AppSettings.WEATHER_API_CONFIGURATION));

        services.Configure<FileStorageOptions>(options =>
        {
            options.RootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FarmAppFiles");

            options.PublicBaseUrl = "https://localhost:7227/files";
        });

        services.Configure<FFmpegOptions>(configuration.GetSection(nameof(FFmpegOptions)));

        var mapperConfig = new MapperConfiguration(config =>
        {
            config.AddProfile(new UserProfile());
            config.AddProfile(new NotificationProfile());
            config.AddProfile(new SteadProfile());
            config.AddProfile(new CustomSteadProfile());
            config.AddProfile(new PropertyProfile());
            config.AddProfile(new PropertyNoteProfile());
            config.AddProfile(new PropertyNoteStatusProfile());
        });

        var mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);
        
        services.Configure<DataProtectionTokenProviderOptions>(options => { options.Name = TokenOptions.DefaultEmailProvider; });
        services.AddScoped<IUserTwoFactorTokenProvider<UserEntity>, CustomEmailTokenProvider>();
        
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.FromMinutes(0),
                    ValidateIssuer = true,
                    ValidIssuer = configuration[$"{nameof(JwtConnectionOptions)}:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration[$"{nameof(JwtConnectionOptions)}:Audience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration[$"{nameof(JwtConnectionOptions)}:SecretKey"]!)),
                    ValidateIssuerSigningKey = true
                };
            });

        
        services.AddPushNotifications(push =>
        {
            push.AddFcm(fcm =>
            {
                fcm.ProjectId = configuration["PushSharp:Fcm:ProjectId"]!;
                fcm.CredentialFilePath = configuration["PushSharp:Fcm:CredentialFilePath"];
                fcm.CredentialJson = configuration["PushSharp:Fcm:CredentialJson"];
            });
            push.AddDeviceStore<FarmDeviceRegistrationRepository>();
        });

        DataAccessLayer.StartupExtension.DataAccessInitializer(services, configuration);
    }
}
