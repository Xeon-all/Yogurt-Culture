using UnityEngine;

namespace YogurtCulture.GameLoop
{
    public class PreparationHandler : PhaseHandlerBase
    {
        public override GamePhase Phase => GamePhase.Preparation;
        public override float Duration => 30f;
        private GameObject ui;
        public override void OnPhaseEnter(GameLoopData data)
        {
            base.OnPhaseEnter(data);
            
            ui = GameLoopManager.Instance.GetPreparationUI();
            if (ui != null) ui.SetActive(true);
        }
        
        public override void OnPhaseExit(GameLoopData data)
        {
            base.OnPhaseExit(data);
            ui = GameLoopManager.Instance.GetPreparationUI();
            if (ui != null) ui.SetActive(false);
        }
    }
}
