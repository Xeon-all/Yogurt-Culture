using UnityEngine;
using UnityEngine.UI;
using YogurtCulture.GameLoop;

public class MorningOpHandler : PhaseHandlerBase
{
    public override GamePhase Phase => GamePhase.MorningOp;
    public override float Duration => 30f;
    public Image tempTimerUI;

    private GameLoopData _data;

    public override void OnPhaseEnter(GameLoopData data)
    {
        base.OnPhaseEnter(data);
        _data = data;
        data.todayEarnings = 0f;
        data.ordersCompleted = 0;
        data.satisfaction = 100f;
        data.todaySuccessOrders = 0;
        data.todayFailOrders = 0;
        data.todayReputationGain = 0f;
        data.todayLevelUps = 0;

        OrderManager.Instance.OnOrderCompleted += OnOrderCompleted;

        OrderManager.Instance.ResetAutoAddTimer();
        OrderManager.Instance.TriggerInitialOrder();
        OrderManager.Instance.OnAutoAddTick += OrderManager.Instance.TempAddOrder;
    }

    public override void OnPhaseUpdate(GameLoopData data, float deltaTime)
    {
        base.OnPhaseUpdate(data, deltaTime);
        tempTimerUI.fillAmount = 1f - phaseTimer / Duration;
        if (Duration <= phaseTimer) GameLoopManager.Instance.TransitToNext();
    }

    public override void OnPhaseExit(GameLoopData data)
    {
        base.OnPhaseExit(data);
        OrderManager.Instance.OnOrderCompleted -= OnOrderCompleted;

        OrderManager.Instance.OnAutoAddTick -= OrderManager.Instance.TempAddOrder;
        OrderManager.Instance.ClearAllOrders();

        var manager = GameLoopManager.Instance.npcManager;
        manager.GetComponent<NpcManager>().ClearAllNpcs();
        manager.SetActive(false);

        _data = null;
    }

    private void OnOrderCompleted(OrderResult result)
    {
        if (_data == null) return;
        _data.ordersCompleted++;
        _data.todayEarnings += result.GoldReward;

        if (result.IsSuccess)
        {
            _data.todaySuccessOrders++;
            _data.todayReputationGain += result.ReputationGain;
            _data.todayLevelUps += result.LevelChange;
        }
        else
        {
            _data.todayFailOrders++;
            _data.satisfaction = Mathf.Max(0, _data.satisfaction - 5f);
        }

        Debug.Log($"[统计] 订单完成 (成功: {result.IsSuccess})");
    }
}
