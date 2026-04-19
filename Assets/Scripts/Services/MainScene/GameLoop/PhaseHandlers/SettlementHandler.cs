using UnityEngine;
using TMPro;
using YogurtCulture.GameLoop;

namespace YogurtCulture.GameLoop
{
    public class SettlementHandler : PhaseHandlerBase
    {
        public override GamePhase Phase => GamePhase.Settlement;
        public override float Duration => 15f;
        public SettlementHandler(GameLoopManager g) : base(g){}

        public override void OnPhaseEnter(GameLoopData data)
        {
            base.OnPhaseEnter(data);

            var ui = _game.SettlementUI;
            if (ui != null) ui.SetActive(true);

            BindUI(data);
            foreach(var action in _game.settleEnterAction)
                action?.Invoke();
        }

        

        public override void OnPhaseExit(GameLoopData data)
        {
            base.OnPhaseExit(data);
            data.dayNumber++;

            var ui = _game.SettlementUI;
            if (ui != null) ui.SetActive(false);
        }

        private void BindUI(GameLoopData data)
        {
            var gm = _game;

            if (gm.SettlementDayText != null)
                gm.SettlementDayText.text = $"{data.dayNumber}";

            if (gm.SettlementMoneyText != null)
                gm.SettlementMoneyText.text = $"{EconomyManager.Instance.Money:F0}";

            if (gm.SettlementSuccessText != null)
                gm.SettlementSuccessText.text = $"{data.todaySuccessOrders}";

            if (gm.SettlementFailText != null)
                gm.SettlementFailText.text = $"{data.todayFailOrders}";

            if (gm.SettlementSatisfactionFill != null)
                gm.SettlementSatisfactionFill.fillAmount = data.satisfaction / 100f;

            // if (gm.SettlementLevelText != null)
            //     gm.SettlementLevelText.text = $"{EconomyManager.Instance.Lv}";

            if (gm.SettlementReputationFill != null)
            {
                _game.StartCoroutine(EconomyManager.Instance.Reputation.HandleReputationGainDisplay(
                    gm.SettlementReputationFill,
                    gm.SettlementLevelText,
                    data.todayReputationGain
                ));
            }
        }
    }
}
