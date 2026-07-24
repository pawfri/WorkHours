using System;
using System.Collections.Generic;
using System.Text;
using WorkHoursLib.Services.Interfaces;

namespace WorkHoursLib.Services.Repositories;

public class GenericRepositoryIM<T> : IGenericRepository<T> where T : class, IEntity
{
    private readonly List<T> _items = new();
    private int _nextId = 1;

    public IEnumerable<T> GetAll()
    {
        return _items.ToList();
    }

    public T? GetById(int id)
    {
        return _items.FirstOrDefault(e => e.Id == id);
    }

    public void Add(T entity)
    {
        entity.Id = _nextId++;
        _items.Add(entity);
    }

    public void Update(T entity)
    {
        var index = _items.FindIndex(e => e.Id == entity.Id);

        if (index >= 0)
        {
            _items[index] = entity;
        }
    }
    public void Delete(int id)
    {
        var entity = GetById(id);

        if (entity != null)
        {
            _items.Remove(entity);
        }
    }

    public void Save()
    {
        // In-memory repository
    }

}
