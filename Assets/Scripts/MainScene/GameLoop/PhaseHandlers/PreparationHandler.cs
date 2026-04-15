using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace YogurtCulture.GameLoop
{
    public class PreparationHandler : PhaseHandlerBase
    {
        public override GamePhase Phase => GamePhase.Preparation;
        public override float Duration => 30f;
        private List<GameObject> uiList;
        public override void OnPhaseEnter(GameLoopData data)
        {
            base.OnPhaseEnter(data);
            
            DisableIndicator();
            uiList = GameLoopManager.Instance.GetPreparationUI();
            uiList[0].GetComponent<PreparationUI>().InitData();
            foreach(var ui in uiList)
                if (ui != null) ui.SetActive(true);
            foreach(var action in GameLoopManager.Instance.preparationAction)
                action?.Invoke();
        }
        
        public override void OnPhaseExit(GameLoopData data)
        {
            base.OnPhaseExit(data);
            uiList = GameLoopManager.Instance.GetPreparationUI();
            if (uiList[0] != null) uiList[0].SetActive(false);
        }

        private void DisableIndicator()
        {
            var indicators = UnityEngine.Object.FindObjectsOfType<BuildingIndicator>();
            foreach(var i in indicators)
                i.enabled = false;
        }
    }
}
