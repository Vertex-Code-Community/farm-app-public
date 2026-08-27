using FarmApp.DataAccessLayer.DbContext;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;

namespace FarmApp.DataAccessLayer.Repositories
{
    public class PropertyNoteMediaRepository : GenericRepository<PropertyNoteMedia, FarmAppDbContext, string>, IPropertyNoteMediaRepository
    {
        public PropertyNoteMediaRepository(FarmAppDbContext context) : base(context)
        {
        }
    }
}
