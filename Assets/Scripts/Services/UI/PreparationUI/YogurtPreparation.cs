using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class YogurtPreparation : 
    MonoBehaviour, IPreparationItem<YogurtData>, IPointerClickHandler
{
    private YogurtItem Item;
    [SerializeField] private TextMeshProUGUI countText;
    private TextMeshProUGUI _tmp;
    public void SetUpItem(Itembase<YogurtData> item)
    {
        Item = item as YogurtItem;
        UpdateDisplay();
    }
    public void UpdateDisplay()
    {
        if (Item == null) return;
        if (Item?.Data == null) return;
        Item.Count = YogurtGameBoard.Instance.Get<YogurtData>(Item.Data.ID).Count;
        if(_tmp == null)
            _tmp = GetComponentInChildren<TextMeshProUGUI>();
        _tmp.text = Item.Data.Name;
        if(!string.IsNullOrEmpty(Item.Data.ItemIcon))
        {
            _tmp.text = "";
            var sprite = Resources.Load<Sprite>(YogurtGameBoard.YOGURT_SPRITE + Item.Data.ItemIcon);
            if(sprite != null)
                GetComponent<Image>().sprite = sprite;
        }
        if (countText != null)
            countText.text = $"{Item.Count}";
    }
    public string GetID() => Item?.Data?.ID;

    public void OnPointerClick(PointerEventData eventData)
    {
        YogurtFactory.Instance.YogurtSpawner.SetUp(Item);
    }
}