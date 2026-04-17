using UnityEngine;

namespace YogurtCulture.GameLoop
{
    [System.Serializable]
    public class GameLoopData
    {
        public GamePhase currentPhase;
        public int dayNumber = 1;
        public float phaseTimer;
        public float todayEarnings = 0f;
        public int ordersCompleted = 0;
        public float satisfaction = 100f;

        public int todaySuccessOrders = 0;
        public int todayFailOrders = 0;
        public int todayReputationGain = 0;
        public int todayLevelUps = 0;

        public event System.Action<int, bool> OnOrderCompleted;
        public event System.Action<float> OnMoneyChanged;

        public void AddOrderCompleted(bool success)
        {
            ordersCompleted++;
            OnOrderCompleted?.Invoke(1, success);
            if (!success) satisfaction = Mathf.Max(0, satisfaction - 5f);
        }

        public void AddEarnings(float amount)
        {
            todayEarnings += amount;
            OnMoneyChanged?.Invoke(amount);
        }

        public float GetPhaseDuration(GamePhase phase)
        {
            return phase switch
            {
                GamePhase.Preparation => 30f,
                GamePhase.MorningOp => 90f,
                GamePhase.NoonBreak => 20f,
                GamePhase.AfternoonOp => 60f,
                GamePhase.Settlement => 15f,
                GamePhase.NightAction => 10f,
                _ => 30f
            };
        }
    }
}
