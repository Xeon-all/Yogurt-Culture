using UnityEngine;

namespace YogurtCulture.GameLoop
{
    /// <summary>
    /// 阶段处理器基类 - 提供默认实现
    /// </summary>
    public abstract class PhaseHandlerBase : IPhaseHandler
    {
        public abstract GamePhase Phase { get; }
        public virtual float Duration => 30f;
        protected float phaseTimer;
        
        public virtual void OnPhaseEnter(GameLoopData data)
        {
            // Debug.Log($"[Handler] {Phase} Enter");
            phaseTimer = 0f;
        }
        
        public virtual void OnPhaseUpdate(GameLoopData data, float deltaTime)
        {
            // 默认无更新逻辑
            phaseTimer += deltaTime;
        }
        
        public virtual void OnPhaseExit(GameLoopData data)
        {
            // Debug.Log($"[Handler] {Phase} Exit");
        }
    }
}
