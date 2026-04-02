using UnityEngine;

namespace YogurtCulture.GameLoop
{
    public class MorningOpHandler : PhaseHandlerBase
    {
        public override GamePhase Phase => GamePhase.MorningOp;
        public override float Duration => 10f;

        public override void OnPhaseEnter(GameLoopData data)
        {
            base.OnPhaseEnter(data);
            OrderManager.Instance.ResetAutoAddTimer();
            OrderManager.Instance.TriggerInitialOrder();
            OrderManager.Instance.OnAutoAddTick += () => OrderManager.Instance.TempAddOrder();
        }

        public override void OnPhaseUpdate(GameLoopData data, float deltaTime)
        {
            base.OnPhaseUpdate(data, deltaTime);
            // if (Duration <= phaseTimer) GameLoopManager.Instance.TransitToNext();
        }

        public override void OnPhaseExit(GameLoopData data)
        {
            base.OnPhaseExit(data);
            OrderManager.Instance.OnAutoAddTick -= OrderManager.Instance.TempAddOrder;
            var manager = GameLoopManager.Instance.npcManager;
            manager.GetComponent<NpcManager>().ClearAllNpcs();
            manager.SetActive(false);
        }
    }
}
