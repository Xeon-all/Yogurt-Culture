using System.Collections.Generic;
using UnityEngine;
using VContainer;

public struct OnYogurtSpawn
{
    public Transform yogurt;
}

/// <summary>
/// 酸奶工厂：负责管理制作中的酸奶实例（activeYogurt），
/// 以及将 YogurtData 数据转化为 YogurtInstance 画面表现。
/// 遵循单一职责原则，数据逻辑与表现逻辑分离。
/// </summary>
public class YogurtFactory : Singleton<YogurtFactory>
{
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
    public YogurtSpawner YogurtSpawner;
    private IEventBus _eventBus;
    [Inject]
    public void Construct(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    /// <summary>
    /// 生成制作中酸奶
    /// </summary>
    public void CreateBaseYogurt(YogurtItem item)
    {
        if (activeYogurt != null)
        {
            Destroy(activeYogurt.gameObject);
            activeYogurt = null;
        }

        var prefab = Resources.Load<GameObject>(YogurtGameBoard.BASE_YOGURT_PREFAB);
        if (prefab == null)
        {
            Debug.LogError($"[YogurtFactory] Failed to load prefab at path: {YogurtGameBoard.BASE_YOGURT_PREFAB}");
            return;
        }

        var instance = Instantiate(prefab, BaseParent.position, Quaternion.identity, BaseParent);
        activeYogurt = instance.GetComponent<YogurtBase>();
        activeYogurt.InitWithYogurtItem(item);
        if (activeYogurt == null)
        {
            Debug.LogError("[YogurtFactory] Created prefab does not have YogurtBase component.");
            Destroy(instance);
            return;
        }
    }
    /// <summary>
    /// 生成酸奶成品实例。
    /// </summary>
    public void CreateYogurtProduct()
    {
        if (activeYogurt == null)
        {
            Debug.LogWarning("[YogurtFactory] No active yogurt to create product from.");
            return;
        }

        var prefab = activeYogurt.Prefab;
        activeYogurt.gameObject.SetActive(false);

        _Instantiate(prefab, ProductParent.position, ProductParent);

        Destroy(activeYogurt.gameObject);
        activeYogurt = null;
    }
    /// <summary>
    /// 从 YogurtData 创建一个 YogurtInstance。
    /// </summary>
    public YogurtInstance _Instantiate(
        GameObject prefab,
        Vector3 position,
        Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogError("[YogurtFactory] YogurtBase has no prefab bound. Cannot instantiate.");
            return null;
        }
        var instance = Instantiate(prefab, position, Quaternion.identity, parent);
        _eventBus.Publish(new OnYogurtSpawn{yogurt = instance.transform});
        var yogurtInstance = instance.GetComponent<YogurtInstance>();
        yogurtInstance.InitWithYogurt(activeYogurt);

        if (yogurtInstance == null)
        {
            Debug.LogWarning("[YogurtFactory] Created instance does not have YogurtInstance component.");
        }

        return yogurtInstance;
    }
}

public class ProductData
{
    [Header("配料标签")]
    [SerializeField] private List<TagData> ingredientTags = new();

    [SerializeField] private int flavor = 0;
    public int Flavor => flavor;
    #region 数据处理
    /// <summary>
    /// 添加一个配料标签（TagData）
    /// </summary>
    public void AddTag(TagData tagData)
    {
        int existingIdx = ingredientTags.FindIndex(t => t.Tag == tagData.Tag);
        if (existingIdx >= 0)
        {
            var existing = ingredientTags[existingIdx];
            ingredientTags[existingIdx] = new TagData(existing.Tag, existing.Value + tagData.Value);
        }
        else
        {
            ingredientTags.Add(tagData);
        }
    }
    public void AdjustFlavor(int amount)
    {
        flavor += amount;
    }
    public List<TagData> GetTags()
    {
        return ingredientTags;
    }
    public int GetTagValue(YogurtTag tag)
    {
        var idx = ingredientTags.FindIndex(t => t.Tag == tag);
        return idx >= 0 ? ingredientTags[idx].Value : 0;
    }
    #endregion
}