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
    private const string DEFAULT_YOGURT_PREFAB_PATH = "Prefabs/BaseYogurt";

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

    /// <summary>
    /// 创建 BaseYogurt 预制体。
    /// 点击按钮后调用，在绑定位置生成 Prefab，销毁旧实例，全局唯一 activeYogurt。
    /// </summary>
    /// <param name="spawnPoint">生成位置（世界坐标）</param>
    /// <param name="parent">父 Transform（可选）</param>
    /// <param name="prefabPath">Prefab 路径（可选，默认使用 BaseYogurt）</param>
    public void CreateBaseYogurt(Transform spawnPoint, Transform parent = null, string prefabPath = DEFAULT_YOGURT_PREFAB_PATH)
    {
        // 销毁旧的制作中实例
        if (activeYogurt != null)
        {
            Destroy(activeYogurt.gameObject);
            activeYogurt = null;
        }

        // 加载并实例化 Prefab
        var prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[YogurtFactory] Failed to load prefab at path: {prefabPath}");
            return;
        }

        Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        var instance = Instantiate(prefab, position, Quaternion.identity, parent);
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
    public YogurtInstance CreateYogurtProduct(Transform spawnPoint = null, Transform parent = null)
    {
        if (activeYogurt == null)
        {
            Debug.LogWarning("[YogurtFactory] No active yogurt to create product from.");
            return null;
        }

        // 收集 activeYogurt 的产品数据
        var product = YogurtProduct.Create(activeYogurt);

        // 销毁制作中实例
        Destroy(activeYogurt.gameObject);
        activeYogurt = null;

        // 使用 activeYogurt 上绑定的 Prefab（优先），否则降级为默认路径
        string prefabPath = DEFAULT_YOGURT_PREFAB_PATH;
        if (product != null && product.Tags.Count > 0)
        {
            // 成品优先使用绑定 Prefab
            prefabPath = DEFAULT_YOGURT_PREFAB_PATH;
        }

        Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        return Instantiate(product, position, parent, prefabPath);
    }

    /// <summary>
    /// 从 YogurtProduct 创建一个 YogurtInstance。
    /// 会实例化 Prefab 并注入产品数据。
    /// </summary>
    /// <param name="product">产品数据</param>
    /// <param name="position">生成位置（世界坐标）</param>
    /// <param name="parent">父 Transform（可选）</param>
    /// <param name="prefabPath">Prefab 路径（可选，默认使用 BaseYogurt）</param>
    /// <returns>创建的 YogurtInstance 实例，失败返回 null</returns>
    public YogurtInstance Instantiate(
        YogurtProduct product,
        Vector3 position,
        Transform parent = null,
        string prefabPath = DEFAULT_YOGURT_PREFAB_PATH)
    {
        if (product == null)
        {
            Debug.LogError("[YogurtFactory] YogurtProduct is null.");
            return null;
        }

        // 加载 Prefab
        var prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[YogurtFactory] Failed to load prefab at path: {prefabPath}");
            return null;
        }

        // 实例化
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
        Transform parent = null,
        string prefabPath = DEFAULT_YOGURT_PREFAB_PATH)
    {
        if (yogurtBase == null)
        {
            Debug.LogError("[YogurtFactory] YogurtBase is null.");
            return null;
        }

        var product = YogurtProduct.Create(yogurtBase);
        return Instantiate(product, position, parent, prefabPath);
    }

    /// <summary>
    /// 创建一个空的酸奶实例（使用默认数据）。
    /// </summary>
    /// <param name="position">生成位置（世界坐标）</param>
    /// <param name="parent">父 Transform（可选）</param>
    /// <param name="prefabPath">Prefab 路径（可选，默认使用 BaseYogurt）</param>
    /// <returns>创建的 YogurtInstance 实例，失败返回 null</returns>
    public YogurtInstance CreateDefault(
        Vector3 position,
        Transform parent = null,
        string prefabPath = DEFAULT_YOGURT_PREFAB_PATH)
    {
        var product = new YogurtProduct();
        return Instantiate(product, position, parent, prefabPath);
    }
}
