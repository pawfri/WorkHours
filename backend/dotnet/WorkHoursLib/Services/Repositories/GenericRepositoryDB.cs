using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WorkHoursLib.Data;
using WorkHoursLib.Services.Interfaces;

namespace WorkHoursLib.Services.Repositories;

public class GenericRepositoryDB<T> : IGenericRepository<T> where T : class
{
    private readonly AppDbContext _context;

    private readonly DbSet<T> _dbSet;
    public GenericRepositoryDB(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public IEnumerable<T> GetAll()
    {
        return _dbSet.ToList();
    }

    public T? GetById(int id)
    {
        return _dbSet.Find(id);
    }

    public void Add(T entity)
    {
        _dbSet.Add(entity);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(int id)
    {
        T? entity = _dbSet.Find(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }

    public void Save()
    {
        _context.SaveChanges();
    }

}
