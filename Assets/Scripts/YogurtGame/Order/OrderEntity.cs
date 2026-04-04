using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using YogurtCulture.GameLoop;

/// <summary>
/// 挂载在 Order Prefab 上的脚本。
/// 持有订单数据，响应玩家的 yogurt 提交，自包含匹配逻辑和结果表现。
/// </summary>
public class OrderEntity : MonoBehaviour
{
    [Header("运行时注入")]
    [SerializeField] private OrderManager.Order orderData;

    [Header("UI 预制体")]
    [Tooltip("订单文本行的预制体（必须带 TextMeshPro 组件）")]
    [SerializeField] private GameObject textLinePrefab;

    [Tooltip("文本行预设间距（Y 轴偏移）")]
    [SerializeField] private float lineSpacing = 0.05f;

    [Tooltip("内容区域根节点（若为空则使用自身）")]
    [SerializeField] private Transform contentRoot;

    /// <summary>
    /// Manager 在实例化后注入数据。
    /// </summary>
    public void Setup(OrderManager.Order data)
    {
        orderData = data;
        BuildContent();
    }

    /// <summary>
    /// 根据 orderData 动态构建子物体：
    /// 第一行：口味需求值；后续每行对应一个 TagData。
    /// </summary>
    private void BuildContent()
    {
        var root = contentRoot != null ? contentRoot : transform;
        ClearChildren(root);

        if (textLinePrefab == null)
        {
            Debug.LogWarning("[OrderEntity] textLinePrefab is not assigned, skipping content build.");
            return;
        }

        // 第一行：口味需求
        AppendLine(root, $"口味值: {orderData?.FlavorExpec ?? 0}", 0);

        // 后续每行：各 TagData
        var demandTags = orderData?.DemandTags;
        if (demandTags != null)
        {
            for (int i = 0; i < demandTags.Count; i++)
            {
                var tag = demandTags[i];
                AppendLine(root, $"{tag.Tag}(需求:{tag.Value})", i + 1);
            }
        }
    }

    private void AppendLine(Transform root, string content, int index)
    {
        var line = Instantiate(textLinePrefab, root);
        var tmp = line.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = content;
        }

        // 简单垂直排列：基准锚点在顶部，后续往下推
        var rect = line.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 1);
            rect.anchorMax = new Vector2(0.5f, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, -index * lineSpacing);
        }
    }

    private void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 玩家 yogurt 与该订单碰撞时由 YogurtInstance 调用。
    /// </summary>
    public void TrySubmit(YogurtData yogurt)
    {
        if (yogurt == null) return;
        Debug.Log($"[OrderEntity] ========== 订单提交判定 ==========\n" +
                  $"订单需求: {FormatDemandTags(orderData?.DemandTags)}");

        if (Match(yogurt))
        {
            Debug.Log("[OrderEntity] 判定结果: 满足需求");
            OnSubmitSuccess(yogurt);
        }
        else
        {
            Debug.Log("[OrderEntity] 判定结果: 不满足需求");
            OnSubmitFail();
        }
    }

    private bool Match(YogurtData yogurt)
    {
        int matchFlavor = CalculateProvidedFlavor(yogurt);
        // 打印计算详情
        Debug.Log($"酸奶实际参数: {FormatYogurtTags(yogurt)}\n" +
                  $"累计需求值（dotProduct）: {matchFlavor}，需求阈值（totalDemandValue）: {orderData.FlavorExpec}，结果: {matchFlavor >= orderData.FlavorExpec}");

        return matchFlavor >= orderData.FlavorExpec;
    }

    private string FormatDemandTags(List<TagData> tags)
    {
        if (tags == null || tags.Count == 0) return "(无)";
        return string.Join(", ", tags.ConvertAll(t => $"{t.Tag}(需求:{t.Value})"));
    }

    private string FormatYogurtTags(YogurtData yogurt)
    {
        if (yogurt == null) return "(null)";
        var tags = yogurt.GetIngredientTags();
        if (tags == null || tags.Count == 0) return "无标签";
        return string.Join(", ", tags.ConvertAll(t => $"{t.Tag}(实际:{t.Value})"));
    }

    private void OnSubmitSuccess(YogurtData yogurt)
    {
        int providedFlavor = CalculateProvidedFlavor(yogurt);
        OrderManager.Instance.OrderSuccess(transform.parent.position);
        StartCoroutine(DissolveAndDestroy(true, yogurt.gameObject, providedFlavor));
    }

    private void OnSubmitFail()
    {
        StartCoroutine(DissolveAndDestroy(false, null, 0));
    }

    private IEnumerator DissolveAndDestroy(bool success, GameObject yogurtObj, int providedFlavor)
    {
        // OrderManager.Instance.OrderHandOver(transform.parent.position);
        float duration = 0.3f;
        float elapsed = 0f;
        gameObject.GetComponent<Collider2D>().enabled = false;
        foreach(Transform child in transform) Destroy(child.gameObject);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (sr != null)
            {
                sr.material.SetFloat("_DissolveAmount", Mathf.LerpUnclamped(-1f, 2f, t));
            }
            yield return null;
        }

        if (sr != null)
        {
            sr.material.SetFloat("_DissolveAmount", 2f);
        }

        if (success)
        {
            Destroy(yogurtObj);
        }

        int demandFlavor = orderData.FlavorExpec;
        OrderManager.Instance.PublishOrderResult(orderData, success, demandFlavor, providedFlavor);
        OrderManager.Instance.ClearSlot(orderData.SlotIndex);
        Destroy(gameObject);
    }

    /// <summary>
    /// 从 yogurt 计算含附加风味值的总风味量。
    /// </summary>
    private int CalculateProvidedFlavor(YogurtData yogurt)
    {
        if (yogurt == null) return 0;
        if (orderData == null || yogurt == null) return 0;

        List<TagData> demandTags = orderData.DemandTags ?? new();
        if (demandTags.Count == 0) return 0;

        int dotProduct = 0;

        foreach (TagData demand in demandTags)
        {
            dotProduct += demand.Value * yogurt.GetTagValue(demand.Tag);
        }
        return dotProduct + yogurt.Exflavor;
    }
}
