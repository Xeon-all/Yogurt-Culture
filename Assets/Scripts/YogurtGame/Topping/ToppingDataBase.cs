using System.Collections.Generic;
using UnityEngine;

public class ToppingDataBase
{
    private readonly Dictionary<string, ToppingItem> _inventory = new();

    public void InitializeAll(int defaultCount = 10)
    {
        _inventory.Clear();
        var allToppings = YogurtGameBoard.Instance.GetAllActiveToppings();
        foreach (var topping in allToppings)
        {
            if (topping == null || string.IsNullOrWhiteSpace(topping.ID))
                continue;
            _inventory[topping.ID] = new ToppingItem(topping, defaultCount);
        }
    }

    public ToppingItem GetItem(string id)
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

    public void Restore(ToppingItem _item)
    {
        if (!_inventory.TryGetValue(_item.Data.ID, out var item))
            return;
        item.Count += _item.Count;
    }

    public bool IsAvailable(string id)
    {
        return _inventory.TryGetValue(id, out var item) && item.Count > 0;
    }

    public List<ToppingItem> GetAllItems()
    {
        var result = new List<ToppingItem>();
        foreach (var item in _inventory.Values)
        {
            result.Add(item);
        }
        return result;
    }
}
