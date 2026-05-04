using UnityEngine;
using UnityEngine.EventSystems;
using YogurtCulture.GameLoop;

public class YogurtSpawner : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SpriteRenderer sr;
    private YogurtItem Item;
    void OnEnable()
    {
        SetUp(null);
    }
    public void SetUp(YogurtItem item)
    {
        Item = item;
        if(item == null) sr.sprite = null;
        else sr.sprite = Resources.Load<Sprite>(YogurtGameBoard.YOGURT_SPRITE + item.Data.GrooveName);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if(GameLoopManager.Instance.CurrentPhase == GamePhase.Preparation || Item == null)
            return;
        YogurtFactory.Instance.CreateBaseYogurt(Item);
        YogurtGameBoard.Instance.Consume<YogurtData>(Item.Data.ID, 1);
    }   
}