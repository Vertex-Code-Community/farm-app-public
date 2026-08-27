using FarmApp.DataAccessLayer.DbContext;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.DataAccessLayer.Repositories;

public class SteadRepository : GenericRepository<SteadEntity, FarmAppDbContext, string>, ISteadRepository
{
    public SteadRepository(FarmAppDbContext context) : base(context)
    {
    }

    public IQueryable<SteadEntity> GetFiltered(string cadNum, List<string> steadsId)
    {
        return DbSet
                .Where(x => string.IsNullOrWhiteSpace(cadNum) || x.CadNum.Contains(cadNum))
                .Where(y => steadsId.Contains(y.Id));
    }
}
