using System.Text.Json;
using FarmApp.DataAccessLayer.DbContext;
using FarmApp.Entities.Entity;
using FarmApp.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PushSharp.Net;

namespace FarmApp.BusinessLogicLayer.Services;

public sealed class FarmDeviceRegistrationRepository(IServiceScopeFactory scopeFactory) : IDeviceRegistrationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task SaveAsync(DeviceRegistration registration, CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FarmAppDbContext>();
        var tagsJson = JsonSerializer.Serialize(registration.Tags.ToArray(), JsonOptions);

        var row = await db.PushDeviceRegistrations
            .FirstOrDefaultAsync(x => x.DeviceId == registration.DeviceId, cancellationToken)
            .ConfigureAwait(false);

        if (row is null)
        {
            db.PushDeviceRegistrations.Add(new PushDeviceRegistrationEntity
            {
                DeviceId = registration.DeviceId,
                DeviceToken = registration.DeviceToken,
                Platform = registration.Platform,
                UserId = registration.UserId,
                TagsJson = tagsJson
            });
        }
        else
        {
            row.DeviceToken = registration.DeviceToken;
            row.Platform = registration.Platform;
            row.UserId = registration.UserId;
            row.TagsJson = tagsJson;
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FarmAppDbContext>();
        await db.PushDeviceRegistrations
            .Where(x => x.DeviceId == deviceId)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task RemoveByTokenAsync(string deviceToken, CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FarmAppDbContext>();
        await db.PushDeviceRegistrations
            .Where(x => x.DeviceToken == deviceToken)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> GetTokensByUserIdAsync(string userId,
        CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FarmAppDbContext>();
        var rows = await db.PushDeviceRegistrations
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Where(r => !HasDisableTag(r.TagsJson))
            .Select(r => r.DeviceToken)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetTokensByTagAsync(string tag,
        CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FarmAppDbContext>();
        var rows = await db.PushDeviceRegistrations
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Where(r => !HasDisableTag(r.TagsJson) && DeserializeTags(r.TagsJson).Contains(tag, StringComparer.Ordinal))
            .Select(r => r.DeviceToken)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetAllTokensAsync(CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FarmAppDbContext>();
        var rows = await db.PushDeviceRegistrations
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Where(r => !HasDisableTag(r.TagsJson))
            .Select(r => r.DeviceToken)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static bool HasDisableTag(string tagsJson) =>
        DeserializeTags(tagsJson).Contains(PushDeviceTags.NotificationDisable, StringComparer.OrdinalIgnoreCase);

    private static List<string> DeserializeTags(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
