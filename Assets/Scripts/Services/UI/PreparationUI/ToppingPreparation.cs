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
        YogurtGameBoard.Instance.Restore<ToppingData>(item.Data.ID, item.Count);
    }

    protected override ToppingItem ConstructItem()
    {
        return Item;
    }

    public void Refresh()
    {
        if (Item?.Data == null) return;
        Item.Count = YogurtGameBoard.Instance.Get<ToppingData>(Item.Data.ID).Count;
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (Item == null) return;
        if(_tmp == null)
            _tmp = GetComponentInChildren<TextMeshProUGUI>();
        _tmp.text = Item.Data.Name;
        if(!string.IsNullOrEmpty(Item.Data.ItemIcon))
        {
            _tmp.text = "";
            var sprite = Resources.Load<Sprite>(YogurtGameBoard.TOPPING_SPRITE + Item.Data.ItemIcon);
            if(sprite != null)
                GetComponent<Image>().sprite = sprite;
        }
        if (countText != null)
            countText.text = $"{Item.Count}";
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        YogurtGameBoard.Instance.Consume<ToppingData>(Item.Data.ID, Item.Count);
    }

    public string GetID() => Item?.Data?.ID;
}