using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class ToppingPreparation : SpawnDragger
{
    [SerializeField] private TextMeshProUGUI countText;
    private TextMeshProUGUI _tmp;

    public override void RestoreTopping(ToppingItem item)
    {
        YogurtGameBoard.Instance.RestoreTopping(item);
    }

    protected override ToppingItem ConstructItem()
    {
        return Item;
    }

    public void Refresh()
    {
        if (Item?.Data == null) return;
        Item.Count = YogurtGameBoard.Instance.GetToppingCount(Item.Data.ID);
        UpdateDisplay();
    }
    
    private const string SPRITEPATH = "Art/Yogurt/Topping/";
    private void UpdateDisplay()
    {
        if (Item == null) return;
        if(_tmp == null)
            _tmp = GetComponentInChildren<TextMeshProUGUI>();
        _tmp.text = Item.Data.Name;
        if(!string.IsNullOrEmpty(Item.Data.ItemIcon))
        {
            _tmp.text = "";
            var sprite = Resources.Load<Sprite>(SPRITEPATH + Item.Data.ItemIcon);
            if(sprite != null)
                GetComponent<Image>().sprite = sprite;
        }
        if (countText != null)
            countText.text = $"{Item.Count}";
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        YogurtGameBoard.Instance.ConsumeTopping(Item.Data.ID, Item.Count);
    }

    public string GetID() => Item?.Data?.ID;
}