using System;
using UnityEngine;
using Excel2Unity;

[Serializable]
public class Itembase<T> : IItembase where T : TableDataBase
{
    public T Data{get; set;}
    TableDataBase IItembase.Data
    {
        get => Data;
        set => Data = (T)value;   // 可加类型检查
    }
    public int Count{get; set;}
    public bool IsActive{get; set;}
    public int CurLv{get; set;}
    public int MaxLv{get; set;}

    public Itembase(T data, int initCount = 10)
    {
        Data = data;
        Count = Mathf.Max(0, initCount);
    }
    public virtual void Upgrade()
    {
        CurLv = Mathf.Min(CurLv + 1, MaxLv);
    }
}
public interface IItembase
{
    void Upgrade();
    TableDataBase Data { get; set; }
    int MaxLv { get; }
    int CurLv { get; }
    int Count { get; set; }
    bool IsActive { get; set; }
}   
