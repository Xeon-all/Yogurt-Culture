using System;
using UnityEngine;
using Excel2Unity;

[Serializable]
public class Itembase<T> where T : TableDataBase
{
    public T Data;
    public int Count;
    public bool IsActive;

    public Itembase(T data, int initCount = 10, bool isActive = true)
    {
        Data = data;
        Count = Mathf.Max(0, initCount);
        IsActive = isActive;
    }
}
