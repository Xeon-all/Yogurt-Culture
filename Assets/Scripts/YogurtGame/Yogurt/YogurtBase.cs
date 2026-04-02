using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 酸奶基础组件：挂载在酸奶 Prefab 上，负责持有产品数据（YogurtProduct）。
/// 实现 IReceiveTopping 接口以接收配料拖放。
/// </summary>
public class YogurtBase : MonoBehaviour, IReceiveTopping
{
    [Header("产品数据")]
    [Tooltip("当前酸奶的产品数据（包含标签、口味、配料数量等）")]
    [SerializeField] private YogurtProduct product;

    [Header("Prefab 设置")]
    [Tooltip("完成后要实例化的 YogurtInstance Prefab（可选）")]
    [SerializeField] private GameObject prefab;

    /// <summary>
    /// 完成后要实例化的 YogurtInstance Prefab
    /// </summary>
    public GameObject Prefab => prefab;

    /// <summary>
    /// 当前产品数据
    /// </summary>
    public YogurtProduct Product
    {
        get => product;
        set => product = value;
    }

    /// <summary>
    /// 当前口味值（只读）
    /// </summary>
    public int Flavor => product?.Flavor ?? 0;

    /// <summary>
    /// 当口味值发生变化时触发，参数为新的口味值
    /// </summary>
    public event Action<int> OnFlavorChanged;

    /// <summary>
    /// 制作过程数据（实时记录操作数据）
    /// </summary>
    protected YogurtProcessData processData;

    private void Awake()
    {
        if (processData == null)
        {
            processData = new YogurtProcessData();
        }
    }

    /// <summary>
    /// 重置制作过程数据
    /// </summary>
    public void ResetProcessData()
    {
        if (processData == null)
        {
            processData = new YogurtProcessData();
        }
        else
        {
            processData.Reset();
        }
    }

    /// <summary>
    /// 调整口味值（可以为正或负），并触发 OnFlavorChanged 回调
    /// </summary>
    /// <param name="delta">要增加的口味量（可为负）</param>
    public void AdjustFlavor(int delta)
    {
        if (product == null) return;
        product.Flavor += delta;
        OnFlavorChanged?.Invoke(product.Flavor);
    }

    /// <summary>
    /// 直接设置口味值（触发事件）
    /// </summary>
    public void SetFlavor(int value)
    {
        if (product == null) return;
        product.Flavor = value;
        OnFlavorChanged?.Invoke(product.Flavor);
    }

    /// <summary>
    /// 获取制作过程数据（用于外部记录操作）
    /// </summary>
    public YogurtProcessData GetProcessData()
    {
        return processData;
    }

    #region IReceiveTopping 实现

    /// <summary>
    /// 接收配料数据（由 IReceiveTopping 接口定义）
    /// </summary>
    public void ReceiveTopping(ToppingData topping)
    {
        if (topping == null || product == null) return;

        var tags = YogurtData.ParseTags(topping.Tags);
        YogurtProduct.AddToppings(product, tags);
    }

    #endregion
}
