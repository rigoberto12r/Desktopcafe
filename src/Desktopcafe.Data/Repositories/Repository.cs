using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Desktopcafe.Data.Repositories;

public class Repository<T> where T : class
{
    protected readonly AppDbContext Db;
    protected readonly DbSet<T> DbSet;

    public Repository(AppDbContext db)
    {
        Db = db;
        DbSet = db.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            return await DbSet.FindAsync(id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get {EntityType} by Id {Id}", typeof(T).Name, id);
            throw;
        }
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        try
        {
            return await DbSet.ToListAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get all {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        try
        {
            await DbSet.AddAsync(entity);
            await Db.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task UpdateAsync(T entity)
    {
        try
        {
            Db.Entry(entity).State = EntityState.Modified;
            await Db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to update {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task DeleteAsync(T entity)
    {
        try
        {
            DbSet.Remove(entity);
            await Db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete {EntityType}", typeof(T).Name);
            throw;
        }
    }
}
