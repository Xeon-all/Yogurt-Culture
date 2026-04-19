using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 声望与等级系统。作为 EconomyManager 的成员使用。
/// </summary>
[Serializable]
public class ReputationSystem
{
    [Header("起始数值")]
    [Tooltip("初始等级")]
    [SerializeField] private int startingLevel = 1;
    [Tooltip("初始声望")]
    [SerializeField] private int startingReputation = 0;

    /// <summary>
    /// 当前等级（只读外部访问）
    /// </summary>
    public int CurrentLevel => _level;

    /// <summary>
    /// 当前声望（只读外部访问）
    /// </summary>
    public int CurrentReputation => _reputation;

    private int _level = 1;
    private int _reputation;
    private const int LEVEL_BASE_COST = 50;
    private const float LEVEL_MULTIPLIER = 1.5f;
    [SerializeField] private AnimationCurve easeOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float levelUpDuration = 0.5f;
    /// <summary>
    /// 静态引用，供 OrderManager 在计算升级时查询当前等级。
    /// </summary>
    public static ReputationSystem Instance { get; private set; }

    public event Action<int> OnLevelChanged;
    public event Action<int> OnReputationChanged;

    public void Init()
    {
        Instance = this;
        _level = startingLevel;
        _reputation = startingReputation;
    }
    public IEnumerator HandleReputationGainDisplay(Image img, TextMeshProUGUI LvTxt, int gain)
    {
        LvTxt.text = _level.ToString();
        var nowFill = _reputation / GetExpForLevel(_level);
        SetFill(img, nowFill);
        while(gain >= GetExpForLevel(_level))
        {
            yield return AnimateFill(img, nowFill, 1f, easeOutCurve, levelUpDuration);
            gain -= GetExpForLevel(_level);
            _level ++;
            LvTxt.text = _level.ToString();
            SetFill(img, 0f);
        }
        float targetFill = (float)gain / GetExpForLevel(_level);
        yield return AnimateFill(img, 0f, targetFill, easeOutCurve, levelUpDuration);
    }
    private IEnumerator AnimateFill(Image img, float from, float to, AnimationCurve curve, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curvedT = curve.Evaluate(t);
            float value = Mathf.Lerp(from, to, curvedT);
            SetFill(img, value);
            yield return null;
        }
        SetFill(img, to);
    }

    private void SetFill(Image img, float value)
    {
        // 假设材质属性名为 "_Fill"
        img.material.SetFloat("_Fill", value);
    }
    public void HandleOrderResult(OrderResult result)
    {
        if (result.IsSuccess)
        {
            _reputation += result.ReputationGain;

            for (int i = 0; i < result.LevelChange; i++)
            {
                _level++;
                OnLevelChanged?.Invoke(_level);
            }
        }
        else
        {
            _reputation = Mathf.Max(0, _reputation - result.ReputationLoss);
        }

        OnReputationChanged?.Invoke(_reputation);
    }
    public static int GetExpForLevel(int level)
    {
        if (level <= 1) return LEVEL_BASE_COST;
        int cost = LEVEL_BASE_COST;
        for (int i = 2; i < level; i++)
            cost = (int)Mathf.Floor(cost * LEVEL_MULTIPLIER + LEVEL_BASE_COST);
        return cost;
    }
}
