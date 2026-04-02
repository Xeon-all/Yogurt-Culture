using UnityEngine;

/// <summary>
/// 酸奶工厂：负责管理制作中的酸奶实例（activeYogurt），
/// 以及将 YogurtProduct 数据转化为 YogurtInstance 画面表现。
/// 遵循单一职责原则，数据逻辑与表现逻辑分离。
/// </summary>
public class YogurtFactory : Singleton<YogurtFactory>
{
    /// <summary>
    /// 默认酸奶 Prefab 路径（相对于 Resources）
    /// </summary>
    private const string DEFAULT_YOGURT_PREFAB_PATH = "Prefabs/GameFunc/BaseYogurt";

    /// <summary>
    /// 当前制作中的酸奶实例（全局唯一）
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
    /// <param name="spawnPoint">生成位置（世界坐标，可为空则使用原点）</param>
    /// <param name="parent">父 Transform（可选）</param>
    public void CreateBaseYogurt()
    {
        // 销毁旧的制作中实例
        if (activeYogurt != null)
        {
            Destroy(activeYogurt.gameObject);
            activeYogurt = null;
        }

        // 加载并实例化 Prefab
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

        // 初始化产品数据
        activeYogurt.Product = new YogurtProduct();
        if (activeYogurt.TryGetComponent(out YogurtData yogurtData))
        {
            yogurtData.Clear();
        }
    }

    /// <summary>
    /// 创建 YogurtProduct。
    /// 收集 activeYogurt 数据，销毁制作中实例，生成酸奶成品实例并传递数据，
    /// 使用绑定的 Prefab 生成最终产品。
    /// </summary>
    /// <param name="spawnPoint">成品生成位置（世界坐标，可为空则使用 Prefab 绑定位置）</param>
    /// <param name="parent">父 Transform（可选）</param>
    /// <returns>创建的 YogurtInstance，失败返回 null</returns>
    public void CreateYogurtProduct()
    {
        if (activeYogurt == null)
        {
            Debug.LogWarning("[YogurtFactory] No active yogurt to create product from.");
            return;
        }

        // 收集产品数据并保存 Prefab 引用（销毁前必须取出来）
        var product = YogurtProduct.Create(activeYogurt);
        var prefab = activeYogurt.Prefab;

        Destroy(activeYogurt.gameObject);
        activeYogurt = null;

        Instantiate(prefab, product, ProductParent.position, ProductParent);
    }

    /// <summary>
    /// 从 YogurtProduct 创建一个 YogurtInstance。
    /// 会实例化 Prefab 并注入产品数据。
    /// </summary>
    /// <param name="prefab">成品 Prefab（优先使用 YogurtBase 绑定值，为空则降级走 Resources.Load）</param>
    /// <param name="product">产品数据</param>
    /// <param name="position">生成位置（世界坐标）</param>
    /// <param name="parent">父 Transform（可选）</param>
    /// <returns>创建的 YogurtInstance 实例，失败返回 null</returns>
    public YogurtInstance Instantiate(
        GameObject prefab,
        YogurtProduct product,
        Vector3 position,
        Transform parent = null)
    {
        if (product == null)
        {
            Debug.LogError("[YogurtFactory] YogurtProduct is null.");
            return null;
        }

        // Prefab 必须绑定在 YogurtBase 上，无降级路径
        if (prefab == null)
        {
            Debug.LogError("[YogurtFactory] YogurtBase has no prefab bound. Cannot instantiate.");
            return null;
        }
        var instance = Object.Instantiate(prefab, position, Quaternion.identity, parent);
        var yogurtInstance = instance.GetComponent<YogurtInstance>();
        var yogurtData = instance.GetComponent<YogurtData>();
        var yogurtBase = instance.GetComponent<YogurtBase>();

        // 注入数据到 YogurtData 组件（兼容旧系统）
        if (yogurtData != null)
        {
            yogurtData.SetFlavor(product.Flavor);
            foreach (var tag in product.Tags)
            {
                yogurtData.AddTag(tag);
            }
        }

        // 注入数据到 YogurtBase 组件（新系统）
        if (yogurtBase != null)
        {
            yogurtBase.Product = product;
        }

        if (yogurtInstance == null)
        {
            Debug.LogWarning("[YogurtFactory] Created instance does not have YogurtInstance component.");
        }

        return yogurtInstance;
    }

    /// <summary>
    /// 从 YogurtBase 创建一个 YogurtInstance。
    /// 会创建新的 YogurtProduct 并复制数据。
    /// </summary>
    /// <param name="yogurtBase">源 YogurtBase 实例</param>
    /// <param name="position">生成位置（世界坐标）</param>
    /// <param name="parent">父 Transform（可选）</param>
    /// <param name="prefabPath">Prefab 路径（可选，默认使用 BaseYogurt）</param>
    /// <returns>创建的 YogurtInstance 实例，失败返回 null</returns>
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

        var product = YogurtProduct.Create(yogurtBase);
        return Instantiate(yogurtBase.Prefab, product, position, parent);
    }

    /// <summary>
    /// 创建一个空的酸奶实例（使用默认数据）。
    /// </summary>
    /// <param name="prefab">成品 Prefab（必传）</param>
    /// <param name="position">生成位置（世界坐标）</param>
    /// <param name="parent">父 Transform（可选）</param>
    /// <returns>创建的 YogurtInstance 实例，失败返回 null</returns>
    public YogurtInstance CreateDefault(
        GameObject prefab,
        Vector3 position,
        Transform parent = null)
    {
        var product = new YogurtProduct();
        return Instantiate(prefab, product, position, parent);
    }
}
