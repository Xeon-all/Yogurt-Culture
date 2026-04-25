using System.Collections.Generic;
using UnityEngine;
using Excel2Unity;
using System;
public interface IDatabase
{
    object this[string id] { get; }
    object GetItem(string id);
    int GetCount(string id);
    void SetCount(string id, int count);
    int Consume(string id, int amount = 1);
    void Restore(string id, int amount);
    void Upgrade(string id);
    bool IsAvailable(string id);
    bool GetActive(string id);
    void SetActive(string id, bool isActive);
    List<object> GetAllActiveItems();
    List<object> GetAllItems();
}
public abstract class Database<T> : IDatabase where T : TableDataBase
{
    protected readonly Dictionary<string, Itembase<T>> _inventory = new();
    
    public abstract string TableShortName { get; }
    public Database()
    {
        _inventory = new();
    }
    public object this[string id] => GetItem(id);
    
    public object GetItem(string id)
    {
        _inventory.TryGetValue(id, out var item);
        return item;
    }

    public int GetCount(string id)
    {
        return _inventory.TryGetValue(id, out var item) ? item.Count : 0;
    }

    public void SetCount(string id, int count)
    {
        if (_inventory.TryGetValue(id, out var item))
        {
            item.Count = Mathf.Max(0, count);
        }
    }

    public int Consume(string id, int amount = 1)
    {
        if (!_inventory.TryGetValue(id, out var item))
            return 0;

        int actual = Mathf.Min(amount, item.Count);
        item.Count -= actual;
        return actual;
    }

    public void Restore(string id, int amount)
    {
        if (!_inventory.TryGetValue(id, out var item))
            return;
        item.Count += amount;
    }

    public void Upgrade(string id)
    {
        if (!_inventory.TryGetValue(id, out var item))
            return;
        item.Upgrade();
    }

    public bool IsAvailable(string id)
    {
        return _inventory.TryGetValue(id, out var item) && item.Count > 0 && item.IsActive;
    }

    public bool GetActive(string id)
    {
        return _inventory.TryGetValue(id, out var item) && item.IsActive;
    }

    public void SetActive(string id, bool isActive)
    {
        if (_inventory.TryGetValue(id, out var item))
        {
            item.IsActive = isActive;
        }
    }

    public List<object> GetAllActiveItems()
    {
        var result = new List<object> ();
        foreach (var item in _inventory.Values)
        {
            if (item.IsActive)
                result.Add(item);
        }
        return result;
    }

    public List<object> GetAllItems()
    {
        return new List<object>(_inventory.Values);
    }
}
