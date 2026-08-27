using FarmApp.Entities.Entity;

namespace FarmApp.DataAccessLayer.Repositories.Interfaces;

public interface IUserRepository : IGenericRepository<UserEntity, string>
{
    Task<UserEntity?> GetByEmailAsync(string email);
    Task<UserEntity?> GetByEmailConfirmTokenAsync(string emailConfirmToken);
    Task<UserEntity?> GetByRefreshTokenAsync(string refreshToken);
    Task<UserEntity?> GetByPasswordResetTokenAsync(string resetToken);
    Task<List<UserEntity>> SearchUsersAsync(string searchTerm, int maxResults = 20);
    Task<List<UserEntity>> FilterWithPagination(int skip, int take);

    Task<List<UserEntity>> GetUsersWithSavedLocationBatchAsync(int skip, int take, CancellationToken cancellationToken = default);

    Task<UserEntity?> GetByIdWithNotificationPreferencesAsync(string id, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetByIdForUpdateWithNotificationPreferencesAsync(string id, CancellationToken cancellationToken = default);
    Task AddDefaultNotificationPreferencesAsync(string userId, CancellationToken cancellationToken = default);
    void AddNotificationPreferencesForUserIfMissing(UserEntity user);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
