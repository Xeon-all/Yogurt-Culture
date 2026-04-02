using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂载在 Order Prefab 上的脚本。
/// 持有订单数据，响应玩家的 yogurt 提交，自包含匹配逻辑和结果表现。
/// </summary>
public class OrderEntity : MonoBehaviour
{
    [Header("运行时注入")]
    [SerializeField] private OrderManager.Order orderData;

    /// <summary>
    /// Manager 在实例化后注入数据。
    /// </summary>
    public void Setup(OrderManager.Order data)
    {
        orderData = data;
    }

    /// <summary>
    /// 玩家 yogurt 与该订单碰撞时由 YogurtInstance 调用。
    /// </summary>
    public void TrySubmit(YogurtData yogurt)
    {
        if (yogurt == null) return;

        if (Match(yogurt))
        {
            OnSubmitSuccess(yogurt);
        }
        else
        {
            OnSubmitFail();
        }
    }

    private bool Match(YogurtData yogurt)
    {
        if (orderData == null || yogurt == null) return false;

        List<TagData> demandTags = orderData.DemandTags ?? new();
        if (demandTags.Count == 0) return false;

        float totalDemandValue = 0f;
        float dotProduct = 0f;

        foreach (TagData demand in demandTags)
        {
            totalDemandValue += demand.Value;
            dotProduct += demand.Value * yogurt.GetTagValue(demand.Tag);
        }

        return dotProduct >= totalDemandValue;
    }

    private void OnSubmitSuccess(YogurtData yogurt)
    {
        EconomyManager.Instance?.AddMoney(orderData.Price);
        Destroy(yogurt.gameObject);
        OrderManager.Instance?.ClearSlot(orderData.SlotIndex);
        Destroy(gameObject);
    }

    private void OnSubmitFail()
    {
        Destroy(gameObject);
        OrderManager.Instance?.ClearSlot(orderData.SlotIndex);
    }
}
