using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 酸奶产品数据结构：纯数据类，不继承 MonoBehaviour。
/// 用于存储酸奶成品的所有属性（标签、口味、配料数量等）。
/// </summary>
[Serializable]
public class YogurtProduct
{
    /// <summary>
    /// 标签列表（包含 Tag 类型和对应数值）
    /// </summary>
    public List<TagData> Tags;

    /// <summary>
    /// 口味值
    /// </summary>
    public int Flavor;

    /// <summary>
    /// 当前已放入的配料数量
    /// </summary>
    public int ToppingCount;

    /// <summary>
    /// 配料数量上限（硬编码为 3）
    /// </summary>
    public const int ToppingMax = 3;

    /// <summary>
    /// 构造函数
    /// </summary>
    public YogurtProduct()
    {
        Tags = new List<TagData>();
        Flavor = 0;
        ToppingCount = 0;
    }


    #region 数据访问方法

    /// <summary>
    /// 获取指定标签的数值，若不存在返回 0
    /// </summary>
    public int GetTagValue(YogurtTag tag)
    {
        var found = Tags.Find(t => t.Tag == tag);
        return found.Tag == tag ? found.Value : 0;
    }

    /// <summary>
    /// 检查是否包含指定标签
    /// </summary>
    public bool HasTag(YogurtTag tag)
    {
        return Tags.Exists(t => t.Tag == tag);
    }

    /// <summary>
    /// 获取剩余可添加的配料数量
    /// </summary>
    public int RemainingToppingSlots => ToppingMax - ToppingCount;

    /// <summary>
    /// 检查是否还能添加配料
    /// </summary>
    public bool CanAddTopping => ToppingCount < ToppingMax;

    #endregion
}
