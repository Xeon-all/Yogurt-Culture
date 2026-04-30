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
    [SerializeField] private float YOffset;

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
        foreach (Transform child in root)
            Destroy(child.gameObject);

        CreateDisplay();
    }
    private void CreateDisplay()
    {
        if (textLinePrefab == null)
        {
            Debug.LogWarning("[OrderEntity] textLinePrefab is not assigned, skipping content build.");
            return;
        }

        DebugContentBuild();
    }
    private void DebugContentBuild()
    {
        // 第一行：口味需求
        AppendLine(contentRoot, $"口味值: {orderData?.FlavorExpec ?? 0}", 0);

        // 后续每行：各 TagData
        var demands = orderData?.Demands;
        if (demands != null)
        {
            for (int i = 0; i < demands.Count; i++)
            {
                if(demands[i] is TagDemand tag)
                    AppendLine(contentRoot, $"{tag.demandTag}{tag.minVal} - {tag.maxVal}", i + 1);
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
            rect.anchoredPosition = new Vector2(0, -index * lineSpacing + YOffset);
        }
    }

    /// <summary>
    /// 玩家 yogurt 与该订单碰撞时由 YogurtInstance 调用。
    /// </summary>
    public void TrySubmit(ProductData yogurt)
    {
        if (yogurt == null) return;
        var result = OrderManager.Instance.GetOrderResult(orderData, Match(yogurt), CalculateProvidedFlavor(yogurt));
        StartCoroutine(DissolveAndDestroy());
        OrderManager.Instance.OrderComplete(result);
    }

    private bool Match(ProductData yogurt) 
    {
        int matchFlavor = CalculateProvidedFlavor(yogurt);
        return matchFlavor >= orderData.FlavorExpec;
    }

    private IEnumerator DissolveAndDestroy()
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
        Destroy(gameObject);
    }

    /// <summary>
    /// 从 yogurt 计算含附加风味值的总风味量。
    /// </summary>
    private int CalculateProvidedFlavor(ProductData yogurt)
    {
        List<IOrderDemand> demands = orderData.Demands;
        if (demands.Count == 0) return 0;

        int output = 0;

        foreach (var demand in demands)
            output += demand.GetScore(yogurt);
        return output;
    }
}
