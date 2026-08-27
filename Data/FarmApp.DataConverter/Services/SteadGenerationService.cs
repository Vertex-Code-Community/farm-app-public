using System.Globalization;
using FarmApp.DataAccessLayer.DbContext;
using FarmApp.DataConverter.DbContext;
using FarmApp.DataConverter.Models;
using FarmApp.DataConverter.Services.Interfaces;
using FarmApp.Entities.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FarmApp.DataConverter.Services;

public class SteadGenerationService : ISteadGenerationService
{
    private readonly FarmAppDbContext _farmAppDbContext;
    private readonly DataConverterDbContext _dataConverterDbContext;
    private readonly IPolygonGenerationService _polygonGenerationService;

    private static ulong _steadIdCounter = 1; // make atomic if multithreading is used
    
    public SteadGenerationService(FarmAppDbContext farmAppDbContext,
        DataConverterDbContext dataConverterDbContext,
        IPolygonGenerationService polygonGenerationService)
    {
        _farmAppDbContext = farmAppDbContext;
        _dataConverterDbContext = dataConverterDbContext;
        _polygonGenerationService = polygonGenerationService;
    }

    public async Task GenerateSteadsAsync(List<int> zoomList, List<CsvRowModel> rows, IServiceScope scope, int districtIndex)
    {
        foreach (var row in rows)
        {
            try
            {
                await ProcessLineAsync(row, zoomList, districtIndex);
            }
            catch (Exception e)
            {
                Console.WriteLine("LINE ERROR");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        
        // Console.WriteLine($"End thousand {_farmAppDbContext.Counter} {Thread.CurrentThread.ManagedThreadId}");
        await _farmAppDbContext.SaveChangesAsync();
        await _dataConverterDbContext.SaveChangesAsync();
        
        scope.Dispose();
    }

    private async Task ProcessLineAsync(CsvRowModel row, List<int> zoomList, int districtIndex)
    {
        var category = await GerOrCreateCategoryAsync(row);
        var purpose = await GerOrCreatePurposeAsync(row);
        var ownership = await GerOrCreateOwnershipAsync(row);
        
        var stead = new SteadEntity
        {
            Id = $"{(ulong) districtIndex * 10000000 + Interlocked.Increment(ref _steadIdCounter)}",
            CadNum = row.Cadnum,
            Area = float.TryParse(row.Area, NumberStyles.Float, CultureInfo.InvariantCulture, out var val) ? val : 0,
            AreaUnit = row.UnitArea,
            Address = row.Address,
            
            CategoryId = category.Id,
            PurposeId = purpose.Id,
            OwnershipId = ownership.Id
        };

        await _polygonGenerationService.GeneratePolygonAsync(stead.Id, row.Geometry, zoomList);
        await _farmAppDbContext.Steads.AddAsync(stead);
    }

    private async Task<OwnershipEntity> GerOrCreateOwnershipAsync(CsvRowModel row)
    {
        var ownership = await _farmAppDbContext.Ownerships
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == row.OwnershipCode);

        if (ownership is not null) return ownership;

        try
        {
            ownership = new OwnershipEntity
            {
                Id = row.OwnershipCode,
                Name = row.Ownership
            };
            
            // Console.WriteLine($"Add ownership {row.Ownership} {_farmAppDbContext.Counter} {Thread.CurrentThread.ManagedThreadId}");

            _farmAppDbContext.Ownerships.Add(ownership);
            _farmAppDbContext.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            // Console.WriteLine("Ownership Exception");
            // Console.WriteLine(_farmAppDbContext.Counter + " " + e.Message);
            // Console.WriteLine(_farmAppDbContext.Counter + " " + e.StackTrace);
            //
            // if (e.InnerException is not null)
            // {
            //     Console.WriteLine(_farmAppDbContext.Counter + " " + e.InnerException.Message);
            //     Console.WriteLine(_farmAppDbContext.Counter + " " + e.InnerException.StackTrace);
            // }
            
            var entry = _farmAppDbContext.Entry(ownership);
            if (entry.State == EntityState.Added) entry.State = EntityState.Detached;
            
            // _farmAppDbContext.SaveChanges();
            
            ownership = null;
            // Console.WriteLine($"Ownership DbUpdateException {row.Category} {_farmAppDbContext.Counter} {Thread.CurrentThread.ManagedThreadId}");

            while (ownership is null) // add counter 
            {
                // Console.WriteLine($"Ownership Try Get {row.Ownership} {Thread.CurrentThread.ManagedThreadId}");
                
                await Task.Delay(200);
                ownership = await _farmAppDbContext.Ownerships
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == row.OwnershipCode);
            }
        }
        
        return ownership;
    }
    
    private async Task<CategoryEntity> GerOrCreateCategoryAsync(CsvRowModel row)
    {
        var category = await _farmAppDbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == row.Category);

        if (category is not null) return category;

        try
        {
            category = new CategoryEntity
            {
                Id = Guid.NewGuid().ToString(),
                Name = row.Category
            };
            
            // Console.WriteLine($"Add category {row.Category} {_farmAppDbContext.Counter} {Thread.CurrentThread.ManagedThreadId}");
            
            _farmAppDbContext.Categories.Add(category);
            _farmAppDbContext.SaveChanges();
        }
        catch (DbUpdateException e)
        {
            // Console.WriteLine("Category Exception");
            // Console.WriteLine(_farmAppDbContext.Counter + " " + e.Message);
            // Console.WriteLine(_farmAppDbContext.Counter + " " + e.StackTrace);
            //
            // if (e.InnerException is not null)
            // {
            //     Console.WriteLine(_farmAppDbContext.Counter + " " + e.InnerException.Message);
            //     Console.WriteLine(_farmAppDbContext.Counter + " " + e.InnerException.StackTrace);
            // }
            
            var entry = _farmAppDbContext.Entry(category);
            if (entry.State == EntityState.Added) entry.State = EntityState.Detached;
            
            // _farmAppDbContext.SaveChanges();
            
            category = null;
            // Console.WriteLine($"Category DbUpdateException {row.Category} {_farmAppDbContext.Counter} {Thread.CurrentThread.ManagedThreadId}");
            while (category is null) // add counter
            {
                // Console.WriteLine($"Category Try Get {row.Category} {Thread.CurrentThread.ManagedThreadId}");
                await Task.Delay(200);
                category = await _farmAppDbContext.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Name == row.Category);
            }
            
            // Console.WriteLine($"FOUND Category Try Get {row.Category} {_farmAppDbContext.Counter} {Thread.CurrentThread.ManagedThreadId}");
        }
        
        return category;
    }
    
    private async Task<PurposeEntity> GerOrCreatePurposeAsync(CsvRowModel row)
    {
        var purpose = await _farmAppDbContext.Purposes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == row.Purpose);

        if (purpose is not null) return purpose;

        try
        {
            purpose = new PurposeEntity
            {
                Id = Guid.NewGuid().ToString(),
                Name = row.Purpose
            };
            
            // Console.WriteLine($"Add purpose {row.Purpose} {_farmAppDbContext.Counter} {Thread.CurrentThread.ManagedThreadId}");
            
            _farmAppDbContext.Purposes.Add(purpose);
            _farmAppDbContext.SaveChanges();
        }
        catch(DbUpdateException e)
        {
            var entry = _farmAppDbContext.Entry(purpose);
            if (entry.State == EntityState.Added) entry.State = EntityState.Detached;
            
            purpose = null;

            while (purpose is null) // add counter
            {
                await Task.Delay(200);
                purpose = await _farmAppDbContext.Purposes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Name == row.Purpose);
            }
        }
        
        return purpose;
    }
}