using System;
using Excel2Unity;
using UnityEngine;

[Serializable]
public class YogurtItem : Itembase<YogurtDatabase>
{
    // public override YogurtDatabase Data { get ; set ; }
    public YogurtItem(YogurtDatabase data, int initCount = 1, bool isActive = true)
        :base(data, initCount, isActive){}
}
