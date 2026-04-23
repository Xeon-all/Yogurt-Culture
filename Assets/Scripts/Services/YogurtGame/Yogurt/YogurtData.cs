using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 酸奶产品的逻辑数据部分。
/// 负责存储和管理配料、口味等数据，不涉及任何画面/拖拽逻辑。
/// </summary>
public class YogurtData : MonoBehaviour
{
    [Header("配料标签")]
    [SerializeField] private List<TagData> ingredientTags = new();

    [SerializeField] private int extraFlavor = 0;
    public int Exflavor => extraFlavor;

    /// <summary>
    /// 添加配料带来的附加风味值
    /// </summary>
    public void AddExtraFlavor(int amount)
    {
        extraFlavor += amount;
    }

    public void SetIngredients(IList<YogurtBase> newIngredients)
    {
        ingredientTags.Clear();
        if (newIngredients == null) return;

        foreach (YogurtBase yogurtBase in newIngredients)
        {
            if (yogurtBase != null)
            {
                ingredientTags.Add(new TagData(YogurtTag.None, 0));
            }
        }
    }

    public List<TagData> GetIngredientTags()
    {
        return ingredientTags;
    }

    public void Clear()
    {
        extraFlavor = 0;
        ingredientTags.Clear();
    }

    /// <summary>
    /// 添加一个配料标签
    /// </summary>
    public void AddTag(YogurtTag tag)
    {
        AddTag(new TagData(tag, 1));
    }

    /// <summary>
    /// 添加一个配料标签（带数值）
    /// </summary>
    public void AddTag(YogurtTag tag, int value)
    {
        AddTag(new TagData(tag, value));
    }

    /// <summary>
    /// 添加一个配料标签（TagData）
    /// </summary>
    public void AddTag(TagData tagData)
    {
        int existingIdx = ingredientTags.FindIndex(t => t.Tag == tagData.Tag);
        if (existingIdx >= 0)
        {
            var existing = ingredientTags[existingIdx];
            ingredientTags[existingIdx] = new TagData(existing.Tag, existing.Value + tagData.Value);
        }
        else
        {
            ingredientTags.Add(tagData);
        }
    }

    /// <summary>
    /// 检查是否包含指定标签
    /// </summary>
    public bool HasTag(YogurtTag tag)
    {
        return ingredientTags.Exists(t => t.Tag == tag);
    }

    /// <summary>
    /// 获取指定标签的数值，若不存在返回 0
    /// </summary>
    public int GetTagValue(YogurtTag tag)
    {
        var found = ingredientTags.Find(t => t.Tag == tag);
        return found.Tag == tag ? found.Value : 0;
    }
}
