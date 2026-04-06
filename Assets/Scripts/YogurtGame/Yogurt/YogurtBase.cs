using System;
using UnityEngine;

/// <summary>
/// 酸奶基础组件：挂载在酸奶 Prefab 上，实现 IReceiveTopping 接口以接收配料拖放。
/// 自身不持有数据，数据统一由 YogurtFactory.ActiveYogurt 上的 YogurtData 组件提供。
/// </summary>
public class YogurtBase : MonoBehaviour, IReceiveTopping
{
    [Header("Prefab 设置")]
    [Tooltip("完成后要实例化的 YogurtInstance Prefab（可选）")]
    [SerializeField] private GameObject prefab;

    /// <summary>
    /// 完成后要实例化的 YogurtInstance Prefab
    /// </summary>
    public GameObject Prefab => prefab;

    /// <summary>
    /// 当前口味值（只读）
    /// </summary>
    public int Flavor => Mathf.RoundToInt(GetComponent<YogurtData>().Exflavor);

    public Action OnReceiveTopping;

    /// <summary>
    /// 调整口味值（可以为正或负）
    /// </summary>
    public void AdjustFlavor(int delta)
    {
        var yogurtData = GetComponent<YogurtData>();
        if (yogurtData == null) return;

        yogurtData.AddExtraFlavor(delta);
    }


    #region IReceiveTopping 实现

    public void ReceiveTopping(ToppingData topping)
    {
        if (topping == null) return;

        var yogurtData = GetComponent<YogurtData>();
        if (yogurtData == null) return;

        yogurtData.AddExtraFlavor(topping.ExFlavor);

        var tags = YogurtData.ParseTags(topping.Tags);
        foreach (var tag in tags)
        {
            yogurtData.AddTag(tag);
        }
        OnReceiveTopping?.Invoke();
    }

    #endregion
}
