using UnityEngine;

/// <summary>
/// 垃圾桶控制器。
/// 当 Yogurt 实例拖拽结束并与垃圾桶 collider 重合时，销毁对应的 Yogurt。
/// </summary>
public class TrashCan : MonoBehaviour
{
    [Header("检测设置")]
    [Tooltip("垃圾桶所在的 Layer 名称")]
    [SerializeField] private string trashLayerName = "trash";

    private int trashLayerMask;
    private Collider2D trashCollider;

    private void Awake()
    {
        trashLayerMask = LayerMask.GetMask(trashLayerName);
        trashCollider = GetComponent<Collider2D>();

        if (trashLayerMask == 0)
        {
            Debug.LogWarning($"[TrashCan] Layer '{trashLayerName}' 未找到，请确保已配置。");
        }
    }

    /// <summary>
    /// 检测指定物体是否与垃圾桶 collider 重合。
    /// </summary>
    /// <param name="target">要检测的物体（通常为 Yogurt 实例）</param>
    /// <returns>如果与垃圾桶重合返回 true，否则返回 false</returns>
    public bool CheckOverlapWithTrashCan(GameObject target)
    {
        if (target == null || trashCollider == null || trashLayerMask == 0)
        {
            return false;
        }

        Collider2D targetCollider = target.GetComponent<Collider2D>();
        if (targetCollider == null)
        {
            return false;
        }

        ContactFilter2D filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = trashLayerMask,
            useTriggers = true
        };

        Collider2D[] results = new Collider2D[4];
        int hitCount = targetCollider.OverlapCollider(filter, results);

        for (int i = 0; i < hitCount; i++)
        {
            if (results[i] == trashCollider)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 尝试销毁指定的 Yogurt 实例。
    /// </summary>
    /// <param name="target">要销毁的物体</param>
    public void TryDestroyYogurt(GameObject target)
    {
        if (target == null) return;

        YogurtData yogurtData = target.GetComponent<YogurtData>();
        if (yogurtData != null)
        {
            Destroy(target);
            Debug.Log($"[TrashCan] 已销毁 Yogurt: {target.name}");
            return;
        }

        YogurtInstance yogurtInstance = target.GetComponent<YogurtInstance>();
        if (yogurtInstance != null)
        {
            Destroy(target);
            Debug.Log($"[TrashCan] 已销毁 Yogurt: {target.name}");
        }
    }
}
