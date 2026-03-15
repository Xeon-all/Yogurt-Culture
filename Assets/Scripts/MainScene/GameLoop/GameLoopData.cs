using UnityEngine;

namespace YogurtCulture.GameLoop
{
    [System.Serializable]
    public class GameLoopData
    {
        public GamePhase currentPhase;
        public int dayNumber = 1;           // 第几天
        public float phaseTimer;             // 阶段计时器
        public float money = 0f;             // 当前金钱
        public int customersServed = 0;      // 已服务顾客数
        public float satisfaction = 100f;   // 顾客满意度
        
        // 当前阶段持续时间配置（秒）
        public float GetPhaseDuration(GamePhase phase)
        {
            return phase switch
            {
                GamePhase.Preparation => 30f,
                GamePhase.MorningOp => 60f,
                GamePhase.NoonBreak => 20f,
                GamePhase.AfternoonOp => 60f,
                GamePhase.Settlement => 15f,
                GamePhase.NightAction => 10f,
                _ => 30f
            };
        }
    }
}
