using System;
using UnityEngine;

[Serializable]
public class ToppingItem
{
    public ToppingData Data;
    public int Count;
    public bool IsActive;

    public ToppingItem(ToppingData data, int initCount = 10, bool isActive = true)
    {
        Data = data;
        Count = Mathf.Max(0, initCount);
        IsActive = isActive;
    }
}
