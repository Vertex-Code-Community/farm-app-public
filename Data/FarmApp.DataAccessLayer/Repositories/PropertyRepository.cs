using FarmApp.DataAccessLayer.DbContext;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;

namespace FarmApp.DataAccessLayer.Repositories;

public class PropertyRepository : GenericRepository<PropertyEntity, FarmAppDbContext, string>, IPropertyRepository
{
    public PropertyRepository(FarmAppDbContext context) : base(context)
    {
    }
}
