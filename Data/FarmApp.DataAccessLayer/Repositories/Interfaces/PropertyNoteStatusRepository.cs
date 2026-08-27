using FarmApp.DataAccessLayer.DbContext;
using FarmApp.Entities.Entity;

namespace FarmApp.DataAccessLayer.Repositories.Interfaces
{
    public class PropertyNoteStatusRepository : GenericRepository<PropertyNoteStatusEntity, FarmAppDbContext, int>, IPropertyNoteStatusRepository
    {
        public PropertyNoteStatusRepository(FarmAppDbContext context) : base(context)
        {
        }
    }
}
