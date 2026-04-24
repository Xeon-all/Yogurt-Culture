using UnityEngine;
using UnityEngine.EventSystems;

public class YogurtSpawner : MonoBehaviour, IPointerClickHandler
{
    public YogurtItem Item;
    public void OnPointerClick(PointerEventData eventData)
    {
        YogurtFactory.Instance.CreateBaseYogurt(Item);
        YogurtGameBoard.Instance.Consume<YogurtData>(Item.Data.ID, 1);
    }   
}