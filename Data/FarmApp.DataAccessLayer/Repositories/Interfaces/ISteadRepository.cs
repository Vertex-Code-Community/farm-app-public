using FarmApp.Entities.Entity;

namespace FarmApp.DataAccessLayer.Repositories.Interfaces;

public interface ISteadRepository : IGenericRepository<SteadEntity, string>
{
    IQueryable<SteadEntity> GetFiltered(string cadNum, List<string> steadsId);
}
