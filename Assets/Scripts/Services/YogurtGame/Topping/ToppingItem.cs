using System;
using Excel2Unity;
using UnityEngine;

[Serializable]
public class ToppingItem : Itembase<ToppingData>
{
    // public override ToppingData Data { get ; set ; }
    public ToppingItem(ToppingData data, int initCount = 10, bool isActive = true)
        :base(data, initCount, isActive){}
}
