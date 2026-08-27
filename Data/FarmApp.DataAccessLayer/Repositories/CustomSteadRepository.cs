using FarmApp.Entities.Entity;
using FarmApp.DataAccessLayer.DbContext;
using FarmApp.DataAccessLayer.Repositories.Interfaces;

namespace FarmApp.DataAccessLayer.Repositories;

public class CustomSteadRepository : GenericRepository<CustomSteadEntity, FarmAppDbContext, string>, ICustomSteadRepository
{
    public CustomSteadRepository(FarmAppDbContext context) : base(context)
    {
    }
}