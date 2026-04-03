using System;
using UnityEngine;

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
    [SerializeField] private float startingReputation = 0f;

    /// <summary>
    /// 当前等级（只读外部访问）
    /// </summary>
    public int CurrentLevel => _level;

    /// <summary>
    /// 当前声望（只读外部访问）
    /// </summary>
    public float CurrentReputation => _reputation;

    private int _level = 1;
    private float _reputation;

    /// <summary>
    /// 静态引用，供 OrderManager 在计算升级时查询当前等级。
    /// </summary>
    public static ReputationSystem Instance { get; private set; }

    public event Action<int> OnLevelChanged;
    public event Action<float> OnReputationChanged;

    public void Init()
    {
        Instance = this;
        _level = startingLevel;
        _reputation = startingReputation;
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
}
