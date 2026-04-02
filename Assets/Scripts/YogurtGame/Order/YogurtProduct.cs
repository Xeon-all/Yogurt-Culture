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

    #region 工厂方法

    /// <summary>
    /// 根据 YogurtBase 创建一个 YogurtProduct。
    /// 将 YogurtBase 中的相关数据复制到新创建的 YogurtProduct 中。
    /// </summary>
    /// <param name="yogurtBase">源 YogurtBase 实例</param>
    /// <returns>新创建的 YogurtProduct 实例</returns>
    public static YogurtProduct Create(YogurtBase yogurtBase)
    {
        if (yogurtBase == null)
        {
            return new YogurtProduct();
        }

        var product = new YogurtProduct
        {
            Flavor = yogurtBase.Flavor,
            Tags = new List<TagData>(),
            ToppingCount = 0
        };

        // 从 YogurtBase 的过程数据中获取配料标签
        var processData = yogurtBase.GetProcessData();
        if (processData != null)
        {
            var ingredientTags = processData.GetIngredientTags();
            if (ingredientTags != null)
            {
                foreach (var tag in ingredientTags)
                {
                    product.Tags.Add(new TagData(tag.Tag, tag.Value));
                    product.ToppingCount += tag.Value;
                }
            }
        }

        return product;
    }

    /// <summary>
    /// 将配料列表中的所有 Tag 数值依次叠加进入 YogurtProduct 中（向量加法）。
    /// 超出配料上限的部分会被忽略。
    /// </summary>
    /// <param name="product">目标 YogurtProduct 实例</param>
    /// <param name="toppings">要添加的配料 TagData 列表</param>
    public static void AddToppings(YogurtProduct product, List<TagData> toppings)
    {
        if (product == null || toppings == null || toppings.Count == 0)
        {
            return;
        }

        foreach (var topping in toppings)
        {
            // 检查是否已达到配料上限
            if (product.ToppingCount >= ToppingMax)
            {
                Debug.Log($"[YogurtProduct] ToppingCount 已达到上限 {ToppingMax}，忽略配料 {topping.Tag}");
                continue;
            }

            // 计算可以添加的数量（不超过上限）
            int remainingSlots = ToppingMax - product.ToppingCount;
            int actualAddValue = Math.Min(topping.Value, remainingSlots);

            if (actualAddValue <= 0)
            {
                continue;
            }

            // 查找是否已存在相同的 Tag
            int existingIdx = product.Tags.FindIndex(t => t.Tag == topping.Tag);
            if (existingIdx >= 0)
            {
                var existing = product.Tags[existingIdx];
                product.Tags[existingIdx] = new TagData(existing.Tag, existing.Value + actualAddValue);
            }
            else
            {
                product.Tags.Add(new TagData(topping.Tag, actualAddValue));
            }

            product.ToppingCount += actualAddValue;
        }
    }

    #endregion

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
