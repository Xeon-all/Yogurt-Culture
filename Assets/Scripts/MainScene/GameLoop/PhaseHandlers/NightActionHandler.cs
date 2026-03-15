using UnityEngine;

namespace YogurtCulture.GameLoop
{
    public class NightActionHandler : PhaseHandlerBase
    {
        public override GamePhase Phase => GamePhase.NightAction;
        public override float Duration => 10f;
        
        public override void OnPhaseEnter(GameLoopData data)
        {
            base.OnPhaseEnter(data);
            // TODO: 显示夜间行动界面（升级、进货等）
        }
        
        public override void OnPhaseExit(GameLoopData data)
        {
            base.OnPhaseExit(data);
            data.dayNumber++; // 进入下一天
            // TODO: 隐藏夜间行动界面
        }
    }
}
