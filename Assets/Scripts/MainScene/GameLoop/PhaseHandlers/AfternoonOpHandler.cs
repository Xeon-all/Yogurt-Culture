using UnityEngine;

namespace YogurtCulture.GameLoop
{
    public class AfternoonOpHandler : PhaseHandlerBase
    {
        public override GamePhase Phase => GamePhase.AfternoonOp;
        public override float Duration => 60f;
        
        public override void OnPhaseEnter(GameLoopData data)
        {
            base.OnPhaseEnter(data);
            GameLoopManager.Instance.npcManager.SetActive(true);
        }
        
        public override void OnPhaseUpdate(GameLoopData data, float deltaTime)
        {
            base.OnPhaseUpdate(data, deltaTime);
            if(Duration <= phaseTimer) GameLoopManager.Instance.TransitToNext();
        }

        public override void OnPhaseExit(GameLoopData data)
        {
            base.OnPhaseExit(data);
            var manager = GameLoopManager.Instance.npcManager;
            manager.GetComponent<NpcManager>().ClearAllNpcs();
            manager.SetActive(false);
        }
    }
}
