namespace YogurtCulture.GameLoop
{
    /// <summary>
    /// 阶段处理器接口 - 实现依赖倒置
    /// </summary>
    public interface IPhaseHandler
    {
        GamePhase Phase { get; }
        float Duration { get; }
        
        void OnPhaseEnter(GameLoopData data);
        void OnPhaseUpdate(GameLoopData data, float deltaTime);
        void OnPhaseExit(GameLoopData data);
    }
}
