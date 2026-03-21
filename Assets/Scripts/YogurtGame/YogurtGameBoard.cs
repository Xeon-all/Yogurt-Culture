using System;
using System.Collections.Generic;
using UnityEngine;
using Excel2Unity;

/// <summary>
/// YogurtGameBoard：负责加载和管理经营过程中会用到的数据表缓存。
/// 策略：Awake 时从 Resources/DataTable/JsonData 读取 JSON，并用自动生成的表类反序列化后缓存。
/// </summary>
public class YogurtGameBoard : MonoBehaviour
{
    public static YogurtGameBoard Instance { get; private set; }

    [Header("Data")]
    [Tooltip("JsonData 加载路径（相对 Resources）。默认：DataTable/JsonData")]
    [SerializeField] private string jsonDataResourcesPath = "DataTable/JsonData";

    // tableShortName -> (id -> row)
    private readonly Dictionary<string, Dictionary<string, TableDataBase>> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    // ToppingTags: id -> List<YogurtTag>
    private readonly Dictionary<string, List<YogurtTag>> _toppingTagsCache =
        new(StringComparer.OrdinalIgnoreCase);

    // Topping激活状态: id -> isActive
    private readonly Dictionary<string, bool> _toppingActiveCache =
        new(StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAll();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// 根据表名（支持简写，如 Topping）与 ID 获取一条记录。
    /// </summary>
    public T Get<T>(string tableName, string id) where T : TableDataBase
    {
        if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(id)) return null;

        string shortName = ToShortTableName(tableName);
        if (!_cache.TryGetValue(shortName, out var table)) return null;
        if (!table.TryGetValue(id, out var row)) return null;
        return row as T;
    }

    /// <summary>
    /// 获取 Topping 的标签列表（自动解析 Tags 字段）
    /// </summary>
    public List<YogurtTag> GetToppingTags(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return _toppingTagsCache.TryGetValue(id, out var tags) ? tags : null;
    }

    /// <summary>
    /// 是否存在某张表（支持简写）。
    /// </summary>
    public bool HasTable(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName)) return false;
        return _cache.ContainsKey(ToShortTableName(tableName));
    }

    /// <summary>
    /// 获取整张表的所有数据
    /// </summary>
    public List<T> GetAll<T>(string tableName) where T : TableDataBase
    {
        if (string.IsNullOrWhiteSpace(tableName)) return null;

        string shortName = ToShortTableName(tableName);
        if (!_cache.TryGetValue(shortName, out var table)) return null;

        var result = new List<T>();
        foreach (var row in table.Values)
        {
            if (row is T typedRow)
            {
                result.Add(typedRow);
            }
        }
        return result;
    }

    private void LoadAll()
    {
        _cache.Clear();
        _toppingTagsCache.Clear();
        _toppingActiveCache.Clear();

        // 加载各表数据
        LoadTable<ToppingData>("Topping", ParseToppingTags);
        LoadTable<UpgradeData>("Upgrade");

        // 初始化所有 Topping 为未激活状态
        InitializeToppingActiveStatus();
    }

    /// <summary>
    /// 加载单张表数据
    /// </summary>
    private void LoadTable<T>(string shortTableName, Action<T> onRowLoaded = null) where T : TableDataBase
    {
        string jsonAssetName = $"{shortTableName}Data";
        string resPath = $"{jsonDataResourcesPath}/{jsonAssetName}";
        TextAsset json = Resources.Load<TextAsset>(resPath);
        if (json == null) return;

        var rows = JsonArrayUtility.FromJsonArray<T>(json.text);
        if (rows == null) return;

        var map = new Dictionary<string, TableDataBase>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows)
        {
            if (row == null) continue;
            if (string.IsNullOrWhiteSpace(row.ID)) continue;
            map[row.ID] = row;

            onRowLoaded?.Invoke(row);
        }

        _cache[shortTableName] = map;
    }

    /// <summary>
    /// 解析 ToppingData 的 Tags 字段
    /// </summary>
    private void ParseToppingTags(ToppingData topping)
    {
        if (topping == null || string.IsNullOrWhiteSpace(topping.Tags)) return;

        var tags = YogurtData.ParseTags(topping.Tags);
        _toppingTagsCache[topping.ID] = tags;
    }

    /// <summary>
    /// 初始化所有 Topping 为未激活状态
    /// </summary>
    private void InitializeToppingActiveStatus()
    {
        if (!_cache.TryGetValue("Topping", out var table)) return;

        foreach (var row in table.Values)
        {
            if (row is ToppingData topping && !string.IsNullOrWhiteSpace(topping.ID))
            {
                _toppingActiveCache[topping.ID] = false;
            }
        }
    }

    /// <summary>
    /// 获取 Topping 的激活状态
    /// </summary>
    public bool GetToppingActive(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        return _toppingActiveCache.TryGetValue(id, out var isActive) && isActive;
    }

    /// <summary>
    /// 设置 Topping 的激活状态
    /// </summary>
    public void SetToppingActive(string id, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        _toppingActiveCache[id] = isActive;
    }

    /// <summary>
    /// 获取所有已激活的 Topping 数据
    /// </summary>
    public List<ToppingData> GetAllActiveToppings()
    {
        var result = new List<ToppingData>();

        if (!_cache.TryGetValue("Topping", out var table)) return result;

        foreach (var row in table.Values)
        {
            if (row is ToppingData topping)
            {
                if (GetToppingActive(topping.ID))
                {
                    result.Add(topping);
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 获取所有 Topping 数据及其激活状态
    /// </summary>
    public List<ToppingData> GetAllToppings()
    {
        var result = new List<ToppingData>();

        if (!_cache.TryGetValue("Topping", out var table)) return result;

        foreach (var row in table.Values)
        {
            if (row is ToppingData topping)
            {
                result.Add(topping);
            }
        }
        return result;
    }

    private static string ToShortTableName(string tableName)
    {
        if (string.IsNullOrEmpty(tableName)) return tableName;
        if (tableName.EndsWith("Data", StringComparison.OrdinalIgnoreCase))
        {
            return tableName.Substring(0, tableName.Length - 4);
        }
        return tableName;
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
            catch
            {
                return null;
            }
        }
    }
}

