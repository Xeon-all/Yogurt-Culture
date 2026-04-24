using System;
using System.Collections.Generic;
using Excel2Unity;
using UnityEngine;

[Serializable]
public class ToppingItem : Itembase<ToppingData>
{
    public List<TagData> Tags;

    public ToppingItem(ToppingData data, int initCount = 10)
        : base(data, initCount)
    {
        IsActive = data.InitActive;
        Tags = YogurtTagSystem.ParseTags(data.Tags);
    }
}
