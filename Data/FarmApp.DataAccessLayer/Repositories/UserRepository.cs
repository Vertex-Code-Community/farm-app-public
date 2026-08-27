using Microsoft.EntityFrameworkCore;
using FarmApp.DataAccessLayer.DbContext;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;

namespace FarmApp.DataAccessLayer.Repositories;

public class UserRepository : GenericRepository<UserEntity, FarmAppDbContext, string>, IUserRepository
{
    private readonly FarmAppDbContext _context;

    public UserRepository(FarmAppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<UserEntity?> GetByIdWithNotificationPreferencesAsync(string id,
        CancellationToken cancellationToken = default) =>
        DbSet.AsNoTracking()
            .Include(u => u.NotificationPreferences)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<UserEntity?> GetByIdForUpdateWithNotificationPreferencesAsync(string id,
        CancellationToken cancellationToken = default) =>
        DbSet
            .Include(u => u.NotificationPreferences)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task AddDefaultNotificationPreferencesAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _context.Set<UserNotificationPreferencesEntity>().AddAsync(
            new UserNotificationPreferencesEntity { UserId = userId }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void AddNotificationPreferencesForUserIfMissing(UserEntity user)
    {
        if (user.NotificationPreferences is not null) return;
        var prefs = new UserNotificationPreferencesEntity { UserId = user.Id };
        user.NotificationPreferences = prefs;
        _context.Set<UserNotificationPreferencesEntity>().Add(prefs);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public Task<UserEntity?> GetByEmailAsync(string email)
    {
        var users = DbSet.ToList();
        return DbSet.FirstOrDefaultAsync(x => x.Email == email);
    }

    public Task<UserEntity?> GetByEmailConfirmTokenAsync(string emailConfirmToken)
    {
        return DbSet.FirstOrDefaultAsync(x => x.EmailConfirmToken == emailConfirmToken);
    }

    public Task<UserEntity?> GetByRefreshTokenAsync(string refreshToken)
    {
        var now = DateTime.UtcNow;
        return DbSet.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken && x.RefreshTokenLifeTime != null && x.RefreshTokenLifeTime > now);
    }

    public Task<UserEntity?> GetByPasswordResetTokenAsync(string resetToken)
    {
        var now = DateTime.UtcNow;
        return DbSet.FirstOrDefaultAsync(x => x.PasswordResetToken == resetToken && x.PasswordResetTokenExpiration != null && x.PasswordResetTokenExpiration > now);
    }

    public Task<List<UserEntity>> SearchUsersAsync(string searchTerm, int maxResults = 20)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Task.FromResult(new List<UserEntity>());

        var lowerSearchTerm = searchTerm.ToLower();

        return DbSet
            .AsNoTracking()
            .Where(x => x.Email.ToLower().Contains(lowerSearchTerm) ||
                        x.FirstName.ToLower().Contains(lowerSearchTerm) ||
                        x.LastName.ToLower().Contains(lowerSearchTerm))
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .Take(maxResults)
            .ToListAsync();
    }
    
    public Task<List<UserEntity>> FilterWithPagination(int skip, int take)
    {
        return DbSet
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public Task<List<UserEntity>> GetUsersWithSavedLocationBatchAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        // Align with device tags: PushDeviceTags.NotificationDisable + PushDeviceTags.WeatherAlerts(…)
        return DbSet
            .AsNoTracking()
            .Include(u => u.NotificationPreferences)
            .Where(u => u.SelectedLocationLatitude != null && u.SelectedLocationLongitude != null)
            .Where(u => u.NotificationPreferences == null
                || (!u.NotificationPreferences.NotificationsDisabled && u.NotificationPreferences.WeatherAlertsEnabled))
            .OrderBy(u => u.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
