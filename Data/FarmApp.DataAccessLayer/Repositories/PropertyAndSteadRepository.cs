using FarmApp.DataAccessLayer.DbContext;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.DataAccessLayer.Repositories;

public class PropertyAndSteadRepository : GenericRepository<PropertyAndSteadEntity, FarmAppDbContext, string>, IPropertyAndSteadRepository
{
    public PropertyAndSteadRepository(FarmAppDbContext context) : base(context)
    {
    }

    // public async Task<List<SteadEntity>> GetAllSteadsPropertyIdAsync(string propertyId)
    // {
    //     return await DbSet
    //             .Where(x => x.PropertyId == propertyId)
    //             .Select(x => x.Stead)
    //             .ToListAsync();
    // }
    //
    // public async Task<List<PropertyAndSteadEntity>> GetAllByPropertyIdAsync(string id)
    // {
    //     return await DbSet.Where(x => x.PropertyId == id).ToListAsync();
    // }
}
