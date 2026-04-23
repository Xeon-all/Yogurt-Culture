using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Tag 相关功能的静态工具类
/// </summary>
public static class YogurtTagSystem
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
    /// 枚举文件路径（相对于 Application.dataPath）
    /// </summary>
    private const string EnumFileRelativePath = "Scripts/Services/YogurtGame/Database/YogurtTagSystem.cs";

    /// <summary>
    /// 静态构造函数：初始化预定义的 Tag 映射
    /// </summary>
    static YogurtTagSystem()
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

    /// <summary>
    /// 解析 Tag 字符串，返回 TagData 列表
    /// 格式：不同 Tag 用分号分隔，单个 Tag 用逗号分隔，如 "sweet,2;sour,4"
    /// 自动处理未知 Tag：若遇到未在枚举中定义的 Tag，会自动添加到枚举文件末尾
    /// </summary>
    public static List<TagData> ParseTags(string tagString)
    {
        var result = new List<TagData>();

        if (string.IsNullOrWhiteSpace(tagString))
        {
            return result;
        }

        string[] entries = tagString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string entry in entries)
        {
            string trimmed = entry.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            string[] kv = trimmed.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (kv.Length == 0) continue;

            string tagName = kv[0].Trim();
            int intValue = kv.Length > 1 && int.TryParse(kv[1].Trim(), out int parsed) ? parsed : 0;

            YogurtTag tag = GetOrCreateTag(tagName);
            result.Add(new TagData(tag, intValue));
        }

        return result;
    }

    /// <summary>
    /// 根据 Tag 名称获取对应枚举值，若不存在则自动添加到枚举文件
    /// </summary>
    private static YogurtTag GetOrCreateTag(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName)) return YogurtTag.None;

        if (_tagNameToEnum.TryGetValue(tagName, out YogurtTag existing))
        {
            return existing;
        }

        YogurtTag newTag = AddTagToEnumFile(tagName);
        return newTag;
    }

    /// <summary>
    /// 动态添加新 Tag 到枚举文件
    /// </summary>
    private static YogurtTag AddTagToEnumFile(string tagName)
    {
#if UNITY_EDITOR
        string enumFilePath = Path.Combine(Application.dataPath, EnumFileRelativePath);
        if (string.IsNullOrEmpty(enumFilePath) || !File.Exists(enumFilePath))
        {
            Debug.LogError($"[YogurtTagSystem] Cannot find YogurtTagSystem.cs file: {enumFilePath}");
            return YogurtTag.None;
        }

        string[] lines = File.ReadAllLines(enumFilePath, Encoding.UTF8);
        var sb = new StringBuilder();
        bool foundClosingBrace = false;
        int insertLineIndex = -1;

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
            Debug.LogError("[YogurtTagSystem] Cannot find enum closing brace");
            return YogurtTag.None;
        }

        if (!IsValidIdentifier(tagName))
        {
            Debug.LogError($"[YogurtTagSystem] Invalid tag name: {tagName}");
            return YogurtTag.None;
        }

        int maxValue = 0;
        var assignedValues = new HashSet<int>();
        foreach (YogurtTag existingTag in Enum.GetValues(typeof(YogurtTag)))
        {
            int val = Convert.ToInt32(existingTag);
            if (assignedValues.Contains(val))
            {
                Debug.LogWarning($"[YogurtTagSystem] Duplicate enum value detected: {existingTag} = {val}. YogurtTagSystem.cs may need cleanup.");
            }
            else
            {
                assignedValues.Add(val);
                if (val > maxValue) maxValue = val;
            }
        }

        int nextValue = maxValue + 1;
        string newEnumLine = $"    {tagName} = {nextValue},";

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

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        _tagNameToEnum.Clear();
        _isInitialized = false;
        InitTagMap();

        Debug.Log($"[YogurtTagSystem] Auto-added new tag: {tagName} = {nextValue}");

        return _tagNameToEnum.TryGetValue(tagName, out YogurtTag newTag) ? newTag : YogurtTag.None;
#else
        Debug.LogError("[YogurtTagSystem] Cannot add tags at runtime in build. Please predefine all tags.");
        return YogurtTag.None;
#endif
    }

    /// <summary>
    /// 验证是否为有效的 C# 标识符
    /// </summary>
    private static bool IsValidIdentifier(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        if (char.IsDigit(name[0])) return false;
        return Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
    }
}

/// <summary>
/// 单个 Tag 的信息，包含枚举值和一个整数
/// </summary>
[Serializable]
public struct TagData
{
    public YogurtTag Tag;
    public int Value;

    public TagData(YogurtTag tag, int value)
    {
        Tag = tag;
        Value = value;
    }
}

/// <summary>
/// 酸奶产品标签枚举
/// 用于标识酸奶成品、原料、配料的特性
/// 注意：未知 Tag 会自动添加到枚举文件末尾
/// </summary>
public enum YogurtTag
{
    // 基础酸奶类型
    None = 0,
    crispy = 1,
    sweet = 3,
    rich = 4,
    fruity = 5,
    sour = 6,
}
