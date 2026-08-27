using FarmApp.DataAccessLayer.DbContext;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FarmApp.DataAccessLayer.Repositories;

public class NotificationRepository
    : GenericRepository<NotificationEntity, FarmAppDbContext, long>, INotificationRepository
{
    [ActivatorUtilitiesConstructor]
    public NotificationRepository(FarmAppDbContext context) : base(context)
    {
    }
    
}
