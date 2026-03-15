using UnityEngine;

namespace YogurtCulture.GameLoop
{
    public class NoonBreakHandler : PhaseHandlerBase
    {
        public override GamePhase Phase => GamePhase.NoonBreak;
        public override float Duration => 20f;
        
        public override void OnPhaseEnter(GameLoopData data)
        {
            base.OnPhaseEnter(data);
            // TODO: 显示午休 UI
        }
        
        public override void OnPhaseExit(GameLoopData data)
        {
            base.OnPhaseExit(data);
            // TODO: 隐藏午休 UI
        }
    }
}
