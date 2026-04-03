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
            Debug.Log($"=== 今日结算 ===");
            Debug.Log($"完成订单: {data.ordersCompleted}");
            Debug.Log($"满意度: {data.satisfaction}");
            Debug.Log($"今日收入: {data.todayEarnings}");
        }
        
        public override void OnPhaseExit(GameLoopData data)
        {
            base.OnPhaseExit(data);
        }
    }
}
