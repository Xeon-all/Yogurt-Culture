using UnityEngine;
using UnityEngine.UI;

namespace YogurtCulture.GameLoop
{
    public class MorningOpHandler : PhaseHandlerBase
    {
        public override GamePhase Phase => GamePhase.MorningOp;
        public override float Duration => 10f;
        public Image tempTimerUI;

        public override void OnPhaseEnter(GameLoopData data)
        {
            base.OnPhaseEnter(data);
            data.todayEarnings = 0f;
            data.ordersCompleted = 0;
            data.satisfaction = 100f;

            data.OnOrderCompleted += OnOrderCompleted;
            data.OnMoneyChanged += OnMoneyChanged;

            OrderManager.Instance.ResetAutoAddTimer();
            OrderManager.Instance.TriggerInitialOrder();
            OrderManager.Instance.OnAutoAddTick += () => OrderManager.Instance.TempAddOrder();
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
            data.OnOrderCompleted -= OnOrderCompleted;
            data.OnMoneyChanged -= OnMoneyChanged;

            OrderManager.Instance.OnAutoAddTick -= OrderManager.Instance.TempAddOrder;
            var manager = GameLoopManager.Instance.npcManager;
            manager.GetComponent<NpcManager>().ClearAllNpcs();
            manager.SetActive(false);
        }

        private void OnOrderCompleted(int delta, bool success)
        {
            Debug.Log($"[统计] 完成订单 +{delta} (成功: {success})");
        }

        private void OnMoneyChanged(float amount)
        {
            Debug.Log($"[统计] 收入 +{amount}");
        }
    }
}
