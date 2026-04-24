using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 酸奶基础组件：挂载在酸奶 Prefab 上，实现 IReceiveTopping 接口以接收配料拖放。
/// 自身不持有数据，数据统一由 YogurtFactory.ActiveYogurt 上的 YogurtData 组件提供。
/// </summary>
public class YogurtBase : MonoBehaviour, IReceiveTopping
{
    public ProductData data = new();
    [SerializeField] private GameObject prefab;

    /// <summary>
    /// 完成后要实例化的 YogurtInstance Prefab
    /// </summary>
    public GameObject Prefab => prefab;
    public void InitWithYogurtItem(YogurtItem item)
    {
        if (item == null) return;
        data = new();
        data.AdjustFlavor(item.Data.ExFlavor);
        var tags = item.Tags;
        foreach (var tag in tags)
            data.AddTag(tag);
    }


    #region IReceiveTopping 实现

    public void ReceiveTopping(ToppingItem item)
    {
        if (item?.Data == null) return;

        int count = item.Count;
        data.AdjustFlavor(item.Data.ExFlavor * count);
        var tags = item.Tags;
        foreach (var tag in tags)
            data.AddTag(tag);
    }

    #endregion
}
