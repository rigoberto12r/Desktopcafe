using Microsoft.EntityFrameworkCore;

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

    public virtual async Task<T?> GetByIdAsync(int id) =>
        await DbSet.FindAsync(id);

    public virtual async Task<List<T>> GetAllAsync() =>
        await DbSet.ToListAsync();

    public virtual async Task<T> AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
        await Db.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        Db.Entry(entity).State = EntityState.Modified;
        await Db.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        DbSet.Remove(entity);
        await Db.SaveChangesAsync();
    }
}
