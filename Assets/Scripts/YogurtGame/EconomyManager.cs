using System;
using UnityEngine;
using TMPro;

/// <summary>
/// 金币管理系统（声望/等级由 ReputationSystem 成员管理）。
/// </summary>
public class EconomyManager : Singleton<EconomyManager>
{
    [Header("起始数值")]
    [Tooltip("初始金币")]
    [SerializeField] private float startingMoney = 100f;

    [Header("声望/等级")]
    [SerializeField] private ReputationSystem reputationSystem = new();

    [Header("UI绑定")]
    [Tooltip("显示金币值的TextMeshPro组件")]
    [SerializeField] private TextMeshProUGUI moneyText;

    private float _money;

    public event Action<float> OnMoneyChanged;

    public ReputationSystem Reputation => reputationSystem;
    public float Money => _money;

    protected override void Awake()
    {
        base.Awake();
        SetMoney(startingMoney);
        reputationSystem.Init();

        OrderManager.Instance.OnOrderCompleted += OnOrderResult;
    }

    private void OnOrderResult(OrderResult result)
    {
        reputationSystem.HandleOrderResult(result);
        if (result.IsSuccess)
            AddMoney(result.GoldReward);
    }

    public void SetMoney(float newMoney)
    {
        if (Mathf.Approximately(newMoney, _money)) return;
        _money = newMoney;
        OnMoneyChanged?.Invoke(_money);
    }

    public void AddMoney(float delta)
    {
        SetMoney(_money + delta);
    }

    public bool TrySpend(float cost)
    {
        if (cost <= 0f) return true;
        if (_money >= cost)
        {
            SetMoney(_money - cost);
            return true;
        }
        return false;
    }

    public void ResetMoney()
    {
        SetMoney(startingMoney);
    }

    private void UpdateMoneyText(float newMoney)
    {
        if (moneyText != null)
            moneyText.text = $"{newMoney:F0}";
    }
}
