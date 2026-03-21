namespace YogurtCulture.GameLoop
{
    public enum GamePhase
    {
        Init,          // 初始化阶段（仅 Awake 时进入一次，不在 phaseOrder 中）
        Preparation,   // 准备阶段
        MorningOp,     // 上午营业
        NoonBreak,     // 午休
        AfternoonOp,   // 下午营业
        Settlement,    // 结算
        NightAction    // 夜间行动
    }
}
