using System;
using System.Collections.Generic;
using UnityEngine;
using Excel2Unity;
using System.Linq;
using YogurtCulture.GameLoop;
using VContainer;

/// <summary>
/// YogurtGameBoard：负责加载和管理经营过程中会用到的数据表缓存。
/// 策略：Awake 时从 Resources/DataTable/JsonData 读取 JSON，并用自动生成的表类反序列化后缓存。
/// </summary>
[DefaultExecutionOrder(-1)]
public class YogurtGameBoard : Singleton<YogurtGameBoard>
{
    #region 静态路径常量（供全局统一访问）

    /// <summary>默认酸奶 Prefab 路径（相对于 Resources）</summary>
    public const string BASE_YOGURT_PREFAB = "Prefabs/GameFunc/BaseYogurt";

    /// <summary>拖拽器 Prefab 目录路径（相对于 Resources）</summary>
    public const string DRAGGER_PREFAB = "Prefabs/GameFunc/";

    /// <summary>Topping 资源目录路径（相对于 Resources）</summary>
    public const string TOPPING_SPRITE = "Art/Yogurt/Topping/";
    public const string YOGURT_SPRITE = "Art/Yogurt/Yogurt/";

    #endregion

    // public static YogurtGameBoard Instance { get; private set; }

    [Header("Data")]
    [Tooltip("JsonData 加载路径（相对 Resources）。默认：DataTable/JsonData")]
    [SerializeField] private string jsonDataResourcesPath = "DataTable/JsonData";

    private Dictionary<string, IDatabase> _databaseCache = new(StringComparer.OrdinalIgnoreCase);
    #region 事件初始化
    private IEventBus _eventBus;
    [Inject]
    public void Construct(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }
    #endregion
    protected override void Awake()
    {
        base.Awake();
        LoadAll();
    }

    private void LoadAll()
    {
        _databaseCache.Clear();

        // 加载各表数据
        LoadTable<ToppingRuntimeDatabase, ToppingData>();
        // LoadTable<UpgradeData>();
        LoadTable<YogurtRuntimeDatabase, YogurtDatabase>();
    }

    /// <summary>
    /// 加载单张表数据
    /// </summary>
    private void LoadTable<T, T2>()
    {
        string name = typeof(T2).Name;
        string resPath = $"{jsonDataResourcesPath}/{name}";
        TextAsset json = Resources.Load<TextAsset>(resPath);
        if (json == null) return;

        var rows = JsonArrayUtility.FromJsonArray<T2>(json.text);
        if (rows == null) return;

        _databaseCache[name] = Activator.CreateInstance(
            typeof(T),
            new object[]{rows}
        ) as IDatabase;
    }

    /// <summary>
    /// 样例Get<YogurtDatabase>("YogurtDatabase", "baseYogurt")
    /// 返回baseYogurt的ItemBase数据
    /// </summary>
    public Itembase<T> Get<T>(string id) where T : TableDataBase
    {
        return _databaseCache[typeof(T).Name][id] as Itembase<T>;
    }

    /// <summary>
    /// 获取整张表的所有数据
    /// </summary>
    public List<Itembase<T>> GetAll<T>() where T : TableDataBase
    {
        return _databaseCache[typeof(T).Name].GetAllItems().OfType<Itembase<T>>().ToList();
    }

    /// <summary>
    /// 获取所有已激活的数据
    /// </summary>
    public List<Itembase<T>> GetAllActive<T>() where T : TableDataBase
    {
        return _databaseCache[typeof(T).Name].GetAllActiveItems().OfType<Itembase<T>>().ToList();
    }

    public void Restore<T>(string id, int amount) where T : TableDataBase
    {
        _databaseCache[typeof(T).Name].Restore(id, amount);
        _eventBus.Publish(new OnItemRestore{type = typeof(T), id = id, amount = amount});
    }

    public void Consume<T>(string id, int amount) where T : TableDataBase
    {
        _databaseCache[typeof(T).Name].Consume(id, amount);
        _eventBus.Publish(new OnItemConsume{type = typeof(T), id = id, amount = amount});
    }

    private static class JsonArrayUtility
    {
        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }

        public static T[] FromJsonArray<T>(string jsonArray)
        {
            if (string.IsNullOrWhiteSpace(jsonArray)) return null;

            string wrapped = "{\"Items\":" + jsonArray + "}";
            try
            {
                return JsonUtility.FromJson<Wrapper<T>>(wrapped)?.Items;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonArrayUtility] Deserialize failed for type {typeof(T).Name}. Error: {ex.Message}\nJSON: {jsonArray}");
                return null;
            }
        }
    }
}

public struct OnItemConsume
{
    public Type type;
    public string id;
    public int amount;
}
public struct OnItemRestore
{
    public Type type;
    public string id;
    public int amount;
}