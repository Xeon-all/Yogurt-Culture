using UnityEngine;

namespace YogurtCulture.GameLoop
{
    public class SettlementHandler : PhaseHandlerBase
    {
        public override GamePhase Phase => GamePhase.Settlement;
        public override float Duration => 15f;
        
        public override void OnPhaseEnter(GameLoopData data)
        {
            base.OnPhaseEnter(data);
            // TODO: 显示结算面板、计算当日收入
        }
        
        public override void OnPhaseExit(GameLoopData data)
        {
            base.OnPhaseExit(data);
            // TODO: 隐藏结算面板
        }
    }
}
