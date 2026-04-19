using UnityEngine;

/// <summary>
/// 酸奶工厂：负责管理制作中的酸奶实例（activeYogurt），
/// 以及将 YogurtData 数据转化为 YogurtInstance 画面表现。
/// 遵循单一职责原则，数据逻辑与表现逻辑分离。
/// </summary>
public class YogurtFactory : Singleton<YogurtFactory>
{
    /// <summary>
    /// 默认酸奶 Prefab 路径（相对于 Resources）
    /// </summary>
    private const string DEFAULT_YOGURT_PREFAB_PATH = "Prefabs/GameFunc/BaseYogurt";

    /// <summary>
    /// 当前制作中的酸奶实例（全局唯一），持有 YogurtData 组件作为数据源
    /// </summary>
    private YogurtBase activeYogurt;

    /// <summary>
    /// 当前制作中的酸奶实例（全局唯一）
    /// </summary>
    public YogurtBase ActiveYogurt => activeYogurt;

    /// <summary>
    /// 是否存在制作中的酸奶
    /// </summary>
    public bool HasActiveYogurt => activeYogurt != null;
    public Transform BaseParent;
    public Transform ProductParent;

    /// <summary>
    /// 创建 BaseYogurt 预制体。
    /// 点击按钮后调用，在绑定位置生成 Prefab，销毁旧实例，全局唯一 activeYogurt。
    /// </summary>
    public void CreateBaseYogurt()
    {
        if (activeYogurt != null)
        {
            Destroy(activeYogurt.gameObject);
            activeYogurt = null;
        }

        var prefab = Resources.Load<GameObject>(DEFAULT_YOGURT_PREFAB_PATH);
        if (prefab == null)
        {
            Debug.LogError($"[YogurtFactory] Failed to load prefab at path: {DEFAULT_YOGURT_PREFAB_PATH}");
            return;
        }

        var instance = Instantiate(prefab, BaseParent.position, Quaternion.identity, BaseParent);
        activeYogurt = instance.GetComponent<YogurtBase>();

        if (activeYogurt == null)
        {
            Debug.LogError("[YogurtFactory] Created prefab does not have YogurtBase component.");
            Destroy(instance);
            return;
        }
    }

    /// <summary>
    /// 完成制作：复用 activeYogurt 的 YogurtData，销毁制作中实例，生成酸奶成品实例。
    /// </summary>
    public void CreateYogurtProduct()
    {
        if (activeYogurt == null)
        {
            Debug.LogWarning("[YogurtFactory] No active yogurt to create product from.");
            return;
        }

        var yogurtData = activeYogurt.GetComponent<YogurtData>();
        var prefab = activeYogurt.Prefab;

        Destroy(activeYogurt.gameObject);
        activeYogurt = null;

        Instantiate(prefab, yogurtData, ProductParent.position, ProductParent);
    }

    /// <summary>
    /// 从 YogurtData 创建一个 YogurtInstance。
    /// </summary>
    public YogurtInstance Instantiate(
        GameObject prefab,
        YogurtData yogurtData,
        Vector3 position,
        Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogError("[YogurtFactory] YogurtBase has no prefab bound. Cannot instantiate.");
            return null;
        }
        AudioManager.Instance.PlaySFX("yogurtSpawn");
        var instance = Instantiate(prefab, position, Quaternion.identity, parent);
        var ps = VFXManager.Instance.AppendVFX("sparkle", instance.transform);
        var s = ps.shape;
        s.scale = new Vector3(s.scale.x/2, s.scale.y * 0.7f, s.scale.z);
        var yogurtInstance = instance.GetComponent<YogurtInstance>();

        var targetData = instance.GetComponent<YogurtData>();
        if (targetData != null && yogurtData != null)
        {
            targetData.AddExtraFlavor(yogurtData.Exflavor);
            foreach (var tag in yogurtData.GetIngredientTags())
            {
                targetData.AddTag(tag);
            }
        }

        if (yogurtInstance == null)
        {
            Debug.LogWarning("[YogurtFactory] Created instance does not have YogurtInstance component.");
        }

        return yogurtInstance;
    }

    /// <summary>
    /// 从 YogurtBase 创建一个 YogurtInstance。
    /// </summary>
    public YogurtInstance InstantiateFromBase(
        YogurtBase yogurtBase,
        Vector3 position,
        Transform parent = null)
    {
        if (yogurtBase == null)
        {
            Debug.LogError("[YogurtFactory] YogurtBase is null.");
            return null;
        }

        var yogurtData = yogurtBase.GetComponent<YogurtData>();
        return Instantiate(yogurtBase.Prefab, yogurtData, position, parent);
    }

    /// <summary>
    /// 创建一个空的酸奶实例（使用默认数据）。
    /// </summary>
    public YogurtInstance CreateDefault(
        GameObject prefab,
        Vector3 position,
        Transform parent = null)
    {
        return Instantiate(prefab, null, position, parent);
    }
}
