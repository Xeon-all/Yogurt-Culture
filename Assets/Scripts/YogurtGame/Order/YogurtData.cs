using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 酸奶产品标签枚举
/// 用于标识酸奶成品、原料、配料的特性
/// 注意：未知 Tag 会自动添加到枚举文件末尾
/// </summary>
public enum YogurtTag
{
    // 基础酸奶类型
    None = 0,

    // ========== 原料（Ingredient）特性 ==========
    // YogurtBase, NormalYogurt 等基础酸奶的 Tag

    // ========== 配料（Topping）特性 ==========
    // 各种 Topping 的 Tag

    // ========== 成品特性 ==========
    // 成品酸奶可能具有的特殊属性
    crispy = 1,
    cereal = 2,
}

/// <summary>
/// 酸奶产品的逻辑数据部分。
/// 负责存储和管理配料、口味等数据，不涉及任何画面/拖拽逻辑。
/// </summary>
public class YogurtData : MonoBehaviour
{
    /// <summary>
    /// Tag 名称到枚举值的映射表（静态缓存）
    /// </summary>
    private static readonly Dictionary<string, YogurtTag> _tagNameToEnum =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 是否已初始化映射表
    /// </summary>
    private static bool _isInitialized = false;

    /// <summary>
    /// 静态构造函数：初始化预定义的 Tag 映射
    /// </summary>
    static YogurtData()
    {
        InitTagMap();
    }

    /// <summary>
    /// 初始化 Tag 名称到枚举的映射
    /// </summary>
    private static void InitTagMap()
    {
        if (_isInitialized) return;

        foreach (YogurtTag tag in Enum.GetValues(typeof(YogurtTag)))
        {
            string name = tag.ToString();
            if (!string.IsNullOrEmpty(name))
            {
                _tagNameToEnum[name] = tag;
            }
        }

        _isInitialized = true;
    }

    [Header("配料标签")]
    [SerializeField] private List<YogurtTag> ingredientTags = new();

    [Header("口味")]
    [SerializeField] private float flavor = 0f;

    public void SetIngredients(IList<Ingredient> newIngredients)
    {
        ingredientTags.Clear();
        if (newIngredients == null) return;

        foreach (Ingredient ingredient in newIngredients)
        {
            if (ingredient != null)
            {
                ingredientTags.Add(YogurtTag.None);
            }
        }
    }

    public List<YogurtTag> GetIngredientTags()
    {
        return ingredientTags;
    }

    public void SetFlavor(float value)
    {
        flavor = value;
    }

    public float GetFlavor()
    {
        return flavor;
    }

    /// <summary>
    /// 通过过程数据设置配料和口味
    /// </summary>
    public void SetIngredientsByProcessData(YogurtProcessData processData)
    {
        if (processData == null) return;

        ingredientTags = processData.GetIngredientTags();
        flavor = processData.FlavorAccumulated;
    }

    /// <summary>
    /// 添加一个配料标签
    /// </summary>
    public void AddTag(YogurtTag tag)
    {
        if (!ingredientTags.Contains(tag))
        {
            ingredientTags.Add(tag);
        }
    }

    /// <summary>
    /// 检查是否包含指定标签
    /// </summary>
    public bool HasTag(YogurtTag tag)
    {
        return ingredientTags.Contains(tag);
    }

    /// <summary>
    /// 解析逗号分隔的 Tag 字符串，返回对应的枚举列表
    /// 自动处理未知 Tag：若遇到未在枚举中定义的 Tag，会自动添加到枚举文件末尾
    /// </summary>
    /// <param name="tagString">逗号分隔的 Tag 字符串，如 "crispy,cereal"</param>
    /// <returns>解析后的 YogurtTag 列表</returns>
    public static List<YogurtTag> ParseTags(string tagString)
    {
        var result = new List<YogurtTag>();

        if (string.IsNullOrWhiteSpace(tagString))
        {
            return result;
        }

        string[] parts = tagString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i] = parts[i].Trim();
        }

        foreach (string part in parts)
        {
            if (string.IsNullOrWhiteSpace(part)) continue;

            YogurtTag tag = GetOrCreateTag(part);
            result.Add(tag);
        }

        return result;
    }

    /// <summary>
    /// 根据 Tag 名称获取对应枚举值
    /// 若不存在则自动添加到枚举文件
    /// </summary>
    private static YogurtTag GetOrCreateTag(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName)) return YogurtTag.None;

        // 尝试从缓存获取
        if (_tagNameToEnum.TryGetValue(tagName, out YogurtTag existing))
        {
            return existing;
        }

        // 未知 Tag，自动添加到枚举文件
        YogurtTag newTag = AddTagToEnumFile(tagName);
        return newTag;
    }

    /// <summary>
    /// 动态添加新 Tag 到枚举文件
    /// </summary>
    private static YogurtTag AddTagToEnumFile(string tagName)
    {
#if UNITY_EDITOR
        string enumFilePath = GetEnumFilePath();
        if (string.IsNullOrEmpty(enumFilePath) || !File.Exists(enumFilePath))
        {
            Debug.LogError($"[YogurtData] Cannot find YogurtData.cs file: {enumFilePath}");
            return YogurtTag.None;
        }

        string[] lines = File.ReadAllLines(enumFilePath, Encoding.UTF8);
        var sb = new StringBuilder();
        bool foundClosingBrace = false;
        int insertLineIndex = -1;

        // 找到枚举的结束大括号
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            sb.AppendLine(line);

            if (line.Trim() == "}")
            {
                foundClosingBrace = true;
                insertLineIndex = i;
                break;
            }
        }

        if (!foundClosingBrace || insertLineIndex < 0)
        {
            Debug.LogError("[YogurtData] Cannot find enum closing brace");
            return YogurtTag.None;
        }

        // 验证 Tag 名称是否合法
        if (!IsValidIdentifier(tagName))
        {
            Debug.LogError($"[YogurtData] Invalid tag name: {tagName}");
            return YogurtTag.None;
        }

        // 生成新枚举值（使用下一个可用值）
        int nextValue = _tagNameToEnum.Count > 0 ? Mathf.Max(_tagNameToEnum.Count, 1) : 0;
        string newEnumLine = $"    {tagName} = {nextValue},";

        // 插入新行
        var newLines = new List<string>();
        for (int i = 0; i < insertLineIndex; i++)
        {
            newLines.Add(lines[i]);
        }
        newLines.Add(newEnumLine);
        for (int i = insertLineIndex; i < lines.Length; i++)
        {
            newLines.Add(lines[i]);
        }

        File.WriteAllLines(enumFilePath, newLines.ToArray(), Encoding.UTF8);

        // 刷新 AssetDatabase
        AssetDatabase.Refresh();

        // 重新初始化映射表
        _tagNameToEnum.Clear();
        _isInitialized = false;
        InitTagMap();

        Debug.Log($"[YogurtData] Auto-added new tag: {tagName} = {nextValue}");

        // 返回新添加的枚举值
        return _tagNameToEnum.TryGetValue(tagName, out YogurtTag newTag) ? newTag : YogurtTag.None;
#else
        Debug.LogError("[YogurtData] Cannot add tags at runtime in build. Please predefine all tags.");
        return YogurtTag.None;
#endif
    }

    /// <summary>
    /// 获取枚举文件路径
    /// </summary>
    private static string GetEnumFilePath()
    {
        // 假设 YogurtData.cs 在 Assets/Scripts/YogurtGame/ 目录下
        return Path.Combine(Application.dataPath, "Scripts", "YogurtGame", "Order", "YogurtData.cs");
    }

    /// <summary>
    /// 验证是否为有效的 C# 标识符
    /// </summary>
    private static bool IsValidIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;

        // C# 标识符不能以数字开头
        if (char.IsDigit(name[0])) return false;

        // 必须只包含字母、数字、下划线
        return Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
    }
}
