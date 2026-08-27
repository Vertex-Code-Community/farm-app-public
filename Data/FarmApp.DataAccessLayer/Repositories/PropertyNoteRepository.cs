using FarmApp.DataAccessLayer.DbContext;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;
using FarmApp.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.DataAccessLayer.Repositories;

public class PropertyNoteRepository : GenericRepository<PropertyNoteEntity, FarmAppDbContext, string>, IPropertyNoteRepository
{
    public PropertyNoteRepository(FarmAppDbContext context) : base(context)
    {
    }
}
