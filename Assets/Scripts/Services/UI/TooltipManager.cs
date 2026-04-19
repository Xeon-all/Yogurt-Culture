using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipManager : Singleton<TooltipManager>
{

    [Header("UI 引用")]
    public RectTransform tooltipRect;    // 提示框本体的 RectTransform
    public TextMeshProUGUI tooltipText;  // 提示框内的文本组件
    
    [Header("系统设置")]
    public Camera mainCamera;            // 用于 3D 坐标转换的主相机

    override protected void Awake()
    {
        base.Awake();
        if (mainCamera == null) mainCamera = Camera.main;
        Hide(); // 初始隐藏
    }

    /// <summary>
    /// 显示提示框的通用方法
    /// </summary>
    /// <param name="content">显示内容</param>
    /// <param name="target">调用者的 Transform (支持 RectTransform 和普通 Transform)</param>
    /// <param name="offset">像素偏移量</param>
    public void Show(string content, Transform target, Vector2 offset = default)
    {
        tooltipText.text = content;

        // 1. 强制立即刷新 UI 布局（极其重要！）
        // 因为内容刚改变，如果不强制刷新，下一帧才能获取到正确的宽高，会导致定位漂移
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);

        // 2. 统一坐标转换：判断调用者是 UI 还是 3D/2D 物体
        Vector2 screenPosition;
        if (target is RectTransform uiRect)
        {
            // 如果是 UI，通常本身就是屏幕空间坐标
            screenPosition = RectTransformUtility.WorldToScreenPoint(null, uiRect.position);
        }
        else
        {
            // 如果是 3D/2D 物体，将世界坐标转为屏幕坐标
            screenPosition = mainCamera.WorldToScreenPoint(target.position);
        }

        // 3. 智能 Pivot 计算：防止提示框飞出屏幕边缘
        // 核心逻辑：如果目标在屏幕右侧，Tooltip 的 Pivot 设为右边(1)，让它往左长；反之亦然。
        float pivotX = screenPosition.x / Screen.width > 0.5f ? 1.0f : 0.0f;
        float pivotY = screenPosition.y / Screen.height > 0.5f ? 1.0f : 0.0f;
        tooltipRect.pivot = new Vector2(pivotX, pivotY);

        // 4. 应用位置与偏移
        // 根据 Pivot 的不同，Offset 的方向也需要智能翻转
        Vector2 finalOffset = new Vector2(
            pivotX == 1 ? -offset.x : offset.x,
            pivotY == 1 ? -offset.y : offset.y
        );
        
        tooltipRect.position = screenPosition + finalOffset;
        tooltipRect.gameObject.SetActive(true);
    }

    public void Hide()
    {
        tooltipRect.gameObject.SetActive(false);
    }
}