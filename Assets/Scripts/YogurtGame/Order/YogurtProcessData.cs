using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 酸奶制作过程数据类。
/// 负责在制作过程中实时记录玩家的操作数据（搅拌时长、力度、添加的Topping等）。
/// 该类为纯数据类，不继承 MonoBehaviour，由 Ingredient 持有。
/// 在制作流程结束时，数据会被传递给 YogurtData 生成最终成品。
/// </summary>
public class YogurtProcessData
{
    /// <summary>
    /// 搅拌总时长（秒）
    /// </summary>
    public float TotalStirDuration { get; set; }

    /// <summary>
    /// 搅拌次数
    /// </summary>
    public int StirCount { get; set; }

    /// <summary>
    /// 累计搅拌力度（每帧力度 * deltaTime 的累加值）
    /// </summary>
    public float StirForceAccumulated { get; set; }

    /// <summary>
    /// 已添加的 Topping 标签列表
    /// </summary>
    public List<YogurtTag> AddedToppingTags { get; private set; } = new List<YogurtTag>();

    /// <summary>
    /// 累计口味值（基于各种操作累加）
    /// </summary>
    public float FlavorAccumulated { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    public YogurtProcessData()
    {
        Reset();
    }

    /// <summary>
    /// 重置所有数据
    /// </summary>
    public void Reset()
    {
        TotalStirDuration = 0f;
        StirCount = 0;
        StirForceAccumulated = 0f;
        AddedToppingTags.Clear();
        FlavorAccumulated = 0f;
    }

    /// <summary>
    /// 记录一次搅拌动作
    /// </summary>
    /// <param name="duration">本次搅拌时长（秒）</param>
    /// <param name="force">本次搅拌力度</param>
    public void RecordStir(float duration, float force)
    {
        if (duration > 0)
        {
            TotalStirDuration += duration;
            StirCount++;
            StirForceAccumulated += force * duration;
        }
    }

    /// <summary>
    /// 添加一个 Topping
    /// </summary>
    /// <param name="tag">Topping 标签</param>
    public void AddTopping(YogurtTag tag)
    {
        if (tag != YogurtTag.None && !AddedToppingTags.Contains(tag))
        {
            AddedToppingTags.Add(tag);
        }
    }

    /// <summary>
    /// 累加口味值
    /// </summary>
    /// <param name="delta">口味增量（可以为负）</param>
    public void AddFlavor(float delta)
    {
        FlavorAccumulated += delta;
    }

    /// <summary>
    /// 获取最终配料标签列表（用于传递给 YogurtData）
    /// </summary>
    public List<YogurtTag> GetIngredientTags()
    {
        // TODO: 根据 AddedToppingTags 返回完整的标签列表
        // 目前返回副本
        return new List<YogurtTag>(AddedToppingTags);
    }
}
