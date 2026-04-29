using UnityEngine;
using UnityEngine.EventSystems;
using YogurtCulture.GameLoop;

public class YogurtSpawner : MonoBehaviour, IPointerClickHandler
{
    public YogurtItem Item;
    public void OnPointerClick(PointerEventData eventData)
    {
        if(GameLoopManager.Instance.CurrentPhase == GamePhase.Preparation)
            return;
        YogurtFactory.Instance.CreateBaseYogurt(Item);
        YogurtGameBoard.Instance.Consume<YogurtData>(Item.Data.ID, 1);
    }   
}