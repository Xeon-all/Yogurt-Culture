using System;
using System.Collections.Generic;
using Excel2Unity;
using UnityEngine;

[Serializable]
public class YogurtItem : Itembase<YogurtData>
{
    public List<TagData> Tags;
    public YogurtItem(YogurtData data, int initCount = 10)
        :base(data, initCount)
    {
        IsActive = data.InitActive;
        Tags = YogurtTagSystem.ParseTags(data.Tags);
        CurLv = 0;
        MaxLv = data.MaxLv;
    }
}
