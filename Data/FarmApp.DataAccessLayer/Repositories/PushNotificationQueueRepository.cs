using FarmApp.DataAccessLayer.DbContext;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;

namespace FarmApp.DataAccessLayer.Repositories
{
    public class PushNotificationQueueRepository : GenericRepository<PushNotificationQueueEntity, FarmAppDbContext, string>,
        IPushNotificationQueueRepository
    {
        public PushNotificationQueueRepository(FarmAppDbContext context) : base(context)
        {
        }
    }
}
