using System;
using UnityEngine;

[Serializable]
public class ToppingItem
{
    public ToppingData Data;
    public int Count;

    public ToppingItem(ToppingData data, int initCount = 10)
    {
        Data = data;
        Count = Mathf.Max(0, initCount);
    }
}
